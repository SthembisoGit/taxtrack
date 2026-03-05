using TaxTrack.Application.Models;

namespace TaxTrack.Application.Interfaces;

public interface IReportService
{
    Task<ReportResponse?> GenerateAsync(
        Guid userId,
        Guid companyId,
        string correlationId,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken);
}
