using TaxTrack.Domain.Common;

namespace TaxTrack.Application.Models;

public sealed record AuditLogEventResponse(
    Guid EventId,
    Guid ActorUserId,
    string ActorEmail,
    Guid? CompanyId,
    AuditEventType EventType,
    DateTime EventTimeUtc,
    string CorrelationId,
    string MetadataJson,
    string? IpAddress,
    string? UserAgent);
