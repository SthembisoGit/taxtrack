using TaxTrack.Application.Models;

namespace TaxTrack.Application.Interfaces;

public interface IUploadService
{
    Task<UploadAcceptedResponse> UploadAsync(
        UploadCommand command,
        string correlationId,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken);

    Task<UploadStatusResponse?> GetUploadStatusAsync(Guid userId, Guid uploadId, CancellationToken cancellationToken);
}
