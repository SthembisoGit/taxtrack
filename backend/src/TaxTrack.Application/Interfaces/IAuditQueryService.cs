using TaxTrack.Application.Models;

namespace TaxTrack.Application.Interfaces;

public interface IAuditQueryService
{
    Task<IReadOnlyCollection<AuditLogEventResponse>> ListAsync(
        Guid userId,
        Guid? companyId,
        int limit,
        CancellationToken cancellationToken);
}
