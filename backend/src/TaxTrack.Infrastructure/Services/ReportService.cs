using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TaxTrack.Application.Exceptions;
using TaxTrack.Application.Interfaces;
using TaxTrack.Application.Models;
using TaxTrack.Domain.Common;
using TaxTrack.Domain.Entities;
using TaxTrack.Infrastructure.Data;
using TaxTrack.Infrastructure.Options;

namespace TaxTrack.Infrastructure.Services;

public sealed class ReportService(
    TaxTrackDbContext dbContext,
    ICompanyAccessService companyAccessService,
    IAuditService auditService,
    IOptions<ReportDownloadOptions> reportDownloadOptions) : IReportService
{
    private static readonly JsonSerializerOptions DownloadJsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly ReportDownloadOptions _reportDownloadOptions = reportDownloadOptions.Value;

    public async Task<ReportResponse?> GenerateAsync(
        Guid userId,
        Guid companyId,
        string correlationId,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken)
    {
        var result = await GetLatestResultAsync(userId, companyId, cancellationToken);
        if (result is null)
        {
            return null;
        }

        return BuildReportResponse(userId, companyId, result, includeDownloadOptions: true);
    }

    public async Task<ReportDownloadResponse?> DownloadJsonAsync(
        Guid userId,
        Guid companyId,
        Guid reportId,
        string format,
        string token,
        string correlationId,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(format, "json", StringComparison.OrdinalIgnoreCase))
        {
            throw new ForbiddenException("Invalid or expired report download token.");
        }

        var validated = ValidateToken(token, userId, companyId, reportId, format);
        if (!validated)
        {
            throw new ForbiddenException("Invalid or expired report download token.");
        }

        var canAccess = await companyAccessService.CanAccessCompanyAsync(userId, companyId, cancellationToken);
        if (!canAccess)
        {
            throw new ForbiddenException("User cannot access this company report.");
        }

        var result = await dbContext.TaxRiskResults
            .Include(x => x.Alerts)
            .FirstOrDefaultAsync(x => x.CompanyId == companyId && x.Id == reportId, cancellationToken);

        if (result is null)
        {
            return null;
        }

        var response = BuildReportResponse(userId, companyId, result, includeDownloadOptions: false);
        await auditService.LogAsync(
            userId,
            companyId,
            AuditEventType.ReportDownloaded,
            correlationId,
            new { reportId = result.Id, format = "json" },
            ipAddress,
            userAgent,
            cancellationToken);

        return new ReportDownloadResponse(
            $"taxtrack-report-{companyId:N}-{result.Id:N}.json",
            "application/json",
            response);
    }

    private async Task<TaxRiskResult?> GetLatestResultAsync(Guid userId, Guid companyId, CancellationToken cancellationToken)
    {
        var canAccess = await companyAccessService.CanAccessCompanyAsync(userId, companyId, cancellationToken);
        if (!canAccess)
        {
            throw new ForbiddenException("User cannot access this company report.");
        }

        return await dbContext.TaxRiskResults
            .Include(x => x.Alerts)
            .Where(x => x.CompanyId == companyId)
            .OrderByDescending(x => x.GeneratedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private ReportResponse BuildReportResponse(
        Guid userId,
        Guid companyId,
        TaxRiskResult result,
        bool includeDownloadOptions)
    {
        var alerts = result.Alerts
            .Select(x => new RiskAlertResponse(
                x.RuleCode,
                x.RuleClass,
                x.Type,
                x.Description,
                x.Severity,
                x.Recommendation,
                x.EvidenceJson))
            .ToArray();

        var riskSummary = new RiskResultResponse(
            result.Id,
            result.CompanyId,
            result.RiskScore,
            result.RegulatoryScore,
            result.HeuristicScore,
            result.RiskLevel,
            result.TaxPolicyVersion,
            result.PolicyEffectiveDate,
            result.EvidenceCompleteness,
            result.InsufficientEvidence,
            alerts,
            result.GeneratedAtUtc);

        var reportId = result.Id;
        var downloadOptions = includeDownloadOptions
            ? BuildDownloadOptions(userId, companyId, reportId)
            : [];

        return new ReportResponse(
            companyId,
            reportId,
            result.GeneratedAtUtc,
            riskSummary,
            alerts,
            downloadOptions);
    }

    private IReadOnlyCollection<ReportDownloadMetadata> BuildDownloadOptions(Guid userId, Guid companyId, Guid reportId)
    {
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(30);
        var token = CreateToken(userId, companyId, reportId, "json", expiresAtUtc);
        var url = $"/api/report/{companyId}/download?reportId={reportId}&format=json&token={Uri.EscapeDataString(token)}";

        return [new ReportDownloadMetadata("json", url, expiresAtUtc)];
    }

    private string CreateToken(Guid userId, Guid companyId, Guid reportId, string format, DateTime expiresAtUtc)
    {
        var payload = new ReportDownloadTokenPayload(
            userId,
            companyId,
            reportId,
            format.ToLowerInvariant(),
            expiresAtUtc);

        var payloadJson = JsonSerializer.Serialize(payload);
        var payloadSegment = Base64UrlEncoder.Encode(payloadJson);
        var signature = ComputeSignature(payloadSegment);
        return $"{payloadSegment}.{signature}";
    }

    private bool ValidateToken(string token, Guid userId, Guid companyId, Guid reportId, string format)
    {
        var parts = token.Split('.', 2, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
        {
            return false;
        }

        var payloadSegment = parts[0];
        var suppliedSignature = parts[1];
        var expectedSignature = ComputeSignature(payloadSegment);

        var suppliedBytes = Encoding.UTF8.GetBytes(suppliedSignature);
        var expectedBytes = Encoding.UTF8.GetBytes(expectedSignature);
        if (suppliedBytes.Length != expectedBytes.Length ||
            !CryptographicOperations.FixedTimeEquals(suppliedBytes, expectedBytes))
        {
            return false;
        }

        ReportDownloadTokenPayload? payload;
        try
        {
            var payloadJson = Base64UrlEncoder.Decode(payloadSegment);
            payload = JsonSerializer.Deserialize<ReportDownloadTokenPayload>(payloadJson);
        }
        catch
        {
            return false;
        }

        if (payload is null)
        {
            return false;
        }

        if (payload.ExpiresAtUtc <= DateTime.UtcNow)
        {
            return false;
        }

        return payload.UserId == userId
               && payload.CompanyId == companyId
               && payload.ReportId == reportId
               && string.Equals(payload.Format, format, StringComparison.OrdinalIgnoreCase);
    }

    private string ComputeSignature(string payloadSegment)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_reportDownloadOptions.SigningKey));
        var signatureBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(payloadSegment));
        return Base64UrlEncoder.Encode(signatureBytes);
    }

    private sealed record ReportDownloadTokenPayload(
        Guid UserId,
        Guid CompanyId,
        Guid ReportId,
        string Format,
        DateTime ExpiresAtUtc);
}
