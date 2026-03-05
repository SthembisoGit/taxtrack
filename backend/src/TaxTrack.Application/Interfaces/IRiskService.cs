using TaxTrack.Application.Models;

namespace TaxTrack.Application.Interfaces;

public interface IRiskService
{
    Task<AnalyzeAcceptedResponse> AnalyzeAsync(
        AnalyzeRiskCommand command,
        string correlationId,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken);

    Task<RiskAnalysisJobStatusResponse?> GetStatusAsync(Guid userId, Guid analysisId, CancellationToken cancellationToken);

    Task<RiskResultResponse?> GetLatestResultAsync(Guid userId, Guid companyId, CancellationToken cancellationToken);
}
