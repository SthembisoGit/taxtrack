using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TaxTrack.Application.Exceptions;
using TaxTrack.Application.Interfaces;
using TaxTrack.Application.Models;
using TaxTrack.Domain.Common;
using TaxTrack.Domain.Entities;
using TaxTrack.Infrastructure.Data;
using TaxTrack.Infrastructure.Services;

namespace TaxTrack.Tests;

public sealed class UploadServiceTests
{
    [Fact]
    public async Task UploadAsync_TranslatesUniqueConstraintFailuresIntoConflict()
    {
        var options = new DbContextOptionsBuilder<TaxTrackDbContext>()
            .UseInMemoryDatabase($"upload-service-tests-{Guid.NewGuid()}")
            .AddInterceptors(new ThrowUniqueConstraintOnSaveInterceptor())
            .Options;

        await using var dbContext = new TaxTrackDbContext(options);
        var userId = Guid.NewGuid();
        var companyId = Guid.NewGuid();

        dbContext.Users.Add(new User
        {
            Id = userId,
            Email = "owner@taxtrack.test",
            PasswordHash = "hash",
            Role = UserRole.Owner
        });
        dbContext.Companies.Add(new Company
        {
            Id = companyId,
            Name = "Acme Holdings",
            RegistrationNumber = "2018/123456/07",
            Industry = "Technology",
            TaxReference = "1234567890",
            OwnerUserId = userId
        });
        await dbContext.SaveChangesAsync();

        var companyAccessService = new Mock<ICompanyAccessService>();
        companyAccessService
            .Setup(x => x.CanAccessCompanyAsync(userId, companyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var auditService = new Mock<IAuditService>();
        var uploadService = new UploadService(
            dbContext,
            companyAccessService.Object,
            auditService.Object,
            NullLogger<UploadService>.Instance);

        await using var content = new MemoryStream(Encoding.UTF8.GetBytes(string.Join('\n',
            "contract_version,source_record_id,company_registration_number,transaction_date,ledger_category,description,amount,currency,vat_amount,direction,source_system,tax_invoice_number,supplier_vat_number,tax_invoice_date,vat201_reference",
            "v1,txn-001,2018/123456/07,2026-01-31,Revenue,Sales invoice,1000.00,ZAR,0.00,credit,Manual,,,,")));

        var command = new UploadCommand
        {
            UserId = userId,
            CompanyId = companyId,
            DatasetType = DatasetType.Transactions,
            IdempotencyKey = "idem-upload-conflict-001",
            FileName = "transactions.csv",
            Content = content
        };

        var exception = await Assert.ThrowsAsync<ConflictException>(() => uploadService.UploadAsync(
            command,
            "corr-upload-conflict",
            "127.0.0.1",
            "xUnit",
            CancellationToken.None));

        Assert.Equal(
            "Upload import collided with existing records for this company. Review duplicate source_record_id values and retry.",
            exception.Message);
    }

    private sealed class ThrowUniqueConstraintOnSaveInterceptor : SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            var context = eventData.Context;
            if (context is not null && context.ChangeTracker.Entries<FinancialTransaction>().Any())
            {
                throw new DbUpdateException(
                    "Unique constraint violation.",
                    new Microsoft.Data.Sqlite.SqliteException(19, 2067));
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}
