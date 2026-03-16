using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using TaxTrack.Application.Exceptions;
using TaxTrack.Application.Interfaces;
using TaxTrack.Application.Models;
using TaxTrack.Domain.Common;
using TaxTrack.Domain.Entities;
using TaxTrack.Infrastructure.Data;
using TaxTrack.Infrastructure.Options;
using TaxTrack.Infrastructure.Services;

namespace TaxTrack.Tests;

public sealed class ServiceIntegrationTests
{
    [Fact]
    public async Task Upload_Idempotency_ReusesExistingUploadJob()
    {
        await using var dbContext = CreateDbContext();
        var (companyId, userId, registrationNumber) = await SeedCompanyAsync(dbContext);
        var uploadService = new UploadService(
            dbContext,
            new CompanyAccessService(dbContext),
            new NoOpAuditService(),
            NullLogger<UploadService>.Instance);

        var payload = BuildTransactionsCsv(registrationNumber);
        using var firstStream = new MemoryStream(payload);
        using var secondStream = new MemoryStream(payload);

        var firstResponse = await uploadService.UploadAsync(
            new UploadCommand
            {
                UserId = userId,
                CompanyId = companyId,
                DatasetType = DatasetType.Transactions,
                FileName = "transactions.csv",
                Content = firstStream,
                IdempotencyKey = "upload-idempotency-001"
            },
            "corr-upload-1",
            "127.0.0.1",
            "xunit",
            CancellationToken.None);

        var secondResponse = await uploadService.UploadAsync(
            new UploadCommand
            {
                UserId = userId,
                CompanyId = companyId,
                DatasetType = DatasetType.Transactions,
                FileName = "transactions.csv",
                Content = secondStream,
                IdempotencyKey = "upload-idempotency-001"
            },
            "corr-upload-2",
            "127.0.0.1",
            "xunit",
            CancellationToken.None);

        Assert.Equal(firstResponse.UploadId, secondResponse.UploadId);
        Assert.Equal(1, await dbContext.FinancialTransactions.CountAsync());
    }

    [Fact]
    public async Task Analyze_Idempotency_ReusesExistingAnalysisJob()
    {
        await using var dbContext = CreateDbContext();
        var (companyId, userId, _) = await SeedCompanyAsync(dbContext);
        var riskService = new RiskService(
            dbContext,
            new CompanyAccessService(dbContext),
            new NoOpAuditService(),
            Options.Create(new TaxPolicyOptions()),
            NullLogger<RiskService>.Instance);

        var firstResponse = await riskService.AnalyzeAsync(
            new AnalyzeRiskCommand
            {
                UserId = userId,
                CompanyId = companyId,
                IdempotencyKey = "analyze-idempotency-001"
            },
            "corr-analyze-1",
            "127.0.0.1",
            "xunit",
            CancellationToken.None);

        var secondResponse = await riskService.AnalyzeAsync(
            new AnalyzeRiskCommand
            {
                UserId = userId,
                CompanyId = companyId,
                IdempotencyKey = "analyze-idempotency-001"
            },
            "corr-analyze-2",
            "127.0.0.1",
            "xunit",
            CancellationToken.None);

        Assert.Equal(firstResponse.AnalysisId, secondResponse.AnalysisId);
        Assert.Equal(1, await dbContext.RiskAnalysisJobs.CountAsync());
    }

    [Fact]
    public async Task Upload_UnauthorizedUser_IsDenied()
    {
        await using var dbContext = CreateDbContext();
        var (companyId, _, registrationNumber) = await SeedCompanyAsync(dbContext);
        var unauthorizedUserId = Guid.NewGuid();
        dbContext.Users.Add(new User
        {
            Id = unauthorizedUserId,
            Email = "outsider@taxtrack.test",
            PasswordHash = "hash",
            Role = UserRole.Viewer
        });
        await dbContext.SaveChangesAsync();

        var uploadService = new UploadService(
            dbContext,
            new CompanyAccessService(dbContext),
            new NoOpAuditService(),
            NullLogger<UploadService>.Instance);
        using var stream = new MemoryStream(BuildTransactionsCsv(registrationNumber));

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            uploadService.UploadAsync(
                new UploadCommand
                {
                    UserId = unauthorizedUserId,
                    CompanyId = companyId,
                    DatasetType = DatasetType.Transactions,
                    FileName = "transactions.csv",
                    Content = stream,
                    IdempotencyKey = "upload-forbidden-001"
                },
                "corr-upload-denied",
                "127.0.0.1",
                "xunit",
                CancellationToken.None));
    }

    [Fact]
    public async Task Analyze_UnauthorizedUser_IsDenied()
    {
        await using var dbContext = CreateDbContext();
        var (companyId, _, _) = await SeedCompanyAsync(dbContext);
        var unauthorizedUserId = Guid.NewGuid();
        dbContext.Users.Add(new User
        {
            Id = unauthorizedUserId,
            Email = "outsider2@taxtrack.test",
            PasswordHash = "hash",
            Role = UserRole.Viewer
        });
        await dbContext.SaveChangesAsync();

        var riskService = new RiskService(
            dbContext,
            new CompanyAccessService(dbContext),
            new NoOpAuditService(),
            Options.Create(new TaxPolicyOptions()),
            NullLogger<RiskService>.Instance);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            riskService.AnalyzeAsync(
                new AnalyzeRiskCommand
                {
                    UserId = unauthorizedUserId,
                    CompanyId = companyId,
                    IdempotencyKey = "analyze-forbidden-001"
                },
                "corr-analyze-denied",
                "127.0.0.1",
                "xunit",
                CancellationToken.None));
    }

    private static TaxTrackDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TaxTrackDbContext>()
            .UseInMemoryDatabase($"taxtrack-service-tests-{Guid.NewGuid()}")
            .Options;
        return new TaxTrackDbContext(options);
    }

    private static async Task<(Guid CompanyId, Guid UserId, string RegistrationNumber)> SeedCompanyAsync(TaxTrackDbContext dbContext)
    {
        var userId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        const string registrationNumber = "REG-2026-001";

        dbContext.Users.Add(new User
        {
            Id = userId,
            Email = "owner@integration.test",
            PasswordHash = "hash",
            Role = UserRole.Owner
        });

        dbContext.Companies.Add(new Company
        {
            Id = companyId,
            Name = "Integration Test Co",
            RegistrationNumber = registrationNumber,
            Industry = "Technology",
            TaxReference = "1234567890",
            OwnerUserId = userId
        });

        dbContext.CompanyMemberships.Add(new CompanyMembership
        {
            CompanyId = companyId,
            UserId = userId,
            Role = MembershipRole.Owner,
            CreatedByUserId = userId,
            IsActive = true
        });

        await dbContext.SaveChangesAsync();
        return (companyId, userId, registrationNumber);
    }

    private static byte[] BuildTransactionsCsv(string registrationNumber)
    {
        var csv = string.Join('\n',
            "contract_version,source_record_id,company_registration_number,transaction_date,ledger_category,description,amount,currency,vat_amount,direction,source_system,tax_invoice_number,supplier_vat_number,tax_invoice_date,vat201_reference",
            $"v1,txn-001,{registrationNumber},2026-01-31,Revenue,Sales invoice,1000.00,ZAR,0.00,credit,Manual,,,,");

        return Encoding.UTF8.GetBytes(csv);
    }

    private sealed class NoOpAuditService : IAuditService
    {
        public Task LogAsync(
            Guid actorUserId,
            Guid? companyId,
            AuditEventType eventType,
            string correlationId,
            object metadata,
            string? ipAddress,
            string? userAgent,
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
