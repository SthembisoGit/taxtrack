using TaxTrack.Domain.Common;

namespace TaxTrack.Application.Interfaces;

public interface IAuditService
{
    Task LogAsync(
        Guid actorUserId,
        Guid? companyId,
        AuditEventType eventType,
        string correlationId,
        object metadata,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken);
}
