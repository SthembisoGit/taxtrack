using TaxTrack.Domain.Common;

namespace TaxTrack.Application.Models;

public sealed class AnalyzeRiskCommand
{
    public Guid UserId { get; init; }
    public Guid CompanyId { get; init; }
    public DateOnly? PeriodStart { get; init; }
    public DateOnly? PeriodEnd { get; init; }
    public required string IdempotencyKey { get; init; }
}

public sealed record AnalyzeAcceptedResponse(
    Guid AnalysisId,
    Guid CompanyId,
    RiskAnalysisJobStatus Status,
    DateTime QueuedAtUtc);

public sealed record RiskAnalysisJobStatusResponse(
    Guid AnalysisId,
    Guid CompanyId,
    RiskAnalysisJobStatus Status,
    Guid? ResultId,
    DateTime UpdatedAtUtc);

public sealed record RiskAlertResponse(
    string RuleCode,
    RuleClass RuleClass,
    string Type,
    string Description,
    AlertSeverity Severity,
    string Recommendation,
    string EvidenceJson);

public sealed record RiskResultResponse(
    Guid ResultId,
    Guid CompanyId,
    int RiskScore,
    int RegulatoryScore,
    int HeuristicScore,
    RiskLevel RiskLevel,
    string TaxPolicyVersion,
    DateOnly PolicyEffectiveDate,
    int EvidenceCompleteness,
    bool InsufficientEvidence,
    IReadOnlyCollection<RiskAlertResponse> Alerts,
    DateTime GeneratedAtUtc);

public sealed record ReportDownloadMetadata(string Format, string Url, DateTime ExpiresAtUtc);

public sealed record ReportResponse(
    Guid CompanyId,
    Guid ReportId,
    DateTime GeneratedAtUtc,
    RiskResultResponse RiskSummary,
    IReadOnlyCollection<RiskAlertResponse> Alerts,
    IReadOnlyCollection<ReportDownloadMetadata> DownloadOptions);
