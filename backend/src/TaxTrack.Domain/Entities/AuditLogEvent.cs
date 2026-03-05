using TaxTrack.Domain.Common;

namespace TaxTrack.Domain.Entities;

public sealed class AuditLogEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ActorUserId { get; set; }
    public Guid? CompanyId { get; set; }
    public AuditEventType EventType { get; set; }
    public DateTime EventTimeUtc { get; set; } = DateTime.UtcNow;
    public required string CorrelationId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string MetadataJson { get; set; } = "{}";
}
