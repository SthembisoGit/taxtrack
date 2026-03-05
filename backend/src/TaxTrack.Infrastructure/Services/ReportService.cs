using Microsoft.EntityFrameworkCore;
using TaxTrack.Application.Exceptions;
using TaxTrack.Application.Interfaces;
using TaxTrack.Application.Models;
using TaxTrack.Domain.Common;
using TaxTrack.Infrastructure.Data;

namespace TaxTrack.Infrastructure.Services;

public sealed class ReportService(
    TaxTrackDbContext dbContext,
    ICompanyAccessService companyAccessService,
    IAuditService auditService) : IReportService
{
    public async Task<ReportResponse?> GenerateAsync(
        Guid userId,
        Guid companyId,
        string correlationId,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken)
    {
        var canAccess = await companyAccessService.CanAccessCompanyAsync(userId, companyId, cancellationToken);
        if (!canAccess)
        {
            throw new ForbiddenException("User cannot access this company report.");
        }

        var result = await dbContext.TaxRiskResults
            .Include(x => x.Alerts)
            .Where(x => x.CompanyId == companyId)
            .OrderByDescending(x => x.GeneratedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (result is null)
        {
            return null;
        }

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

        var reportId = Guid.NewGuid();
        var response = new ReportResponse(
            companyId,
            reportId,
            DateTime.UtcNow,
            riskSummary,
            alerts,
            [
                new ReportDownloadMetadata("json", $"/api/report/{companyId}?format=json&reportId={reportId}", DateTime.UtcNow.AddMinutes(30)),
                new ReportDownloadMetadata("pdf", $"/api/report/{companyId}?format=pdf&reportId={reportId}", DateTime.UtcNow.AddMinutes(30))
            ]);

        await auditService.LogAsync(
            userId,
            companyId,
            AuditEventType.ReportDownloaded,
            correlationId,
            new { reportId, resultId = result.Id },
            ipAddress,
            userAgent,
            cancellationToken);

        return response;
    }
}
