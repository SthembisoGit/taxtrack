using TaxTrack.Application.Models;

namespace TaxTrack.Application.Interfaces;

public interface IPrivacyService
{
    Task<DataSubjectRequestResponse> CreateRequestAsync(
        CreateDataSubjectRequestCommand command,
        string correlationId,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken);

    Task<DataSubjectRequestResponse?> GetRequestAsync(Guid userId, Guid requestId, CancellationToken cancellationToken);
}
