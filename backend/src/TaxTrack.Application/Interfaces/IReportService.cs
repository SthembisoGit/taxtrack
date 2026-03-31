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

    Task<ReportDownloadResponse?> DownloadJsonAsync(
        Guid userId,
        Guid companyId,
        Guid reportId,
        string format,
        string token,
        string correlationId,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken);
}
