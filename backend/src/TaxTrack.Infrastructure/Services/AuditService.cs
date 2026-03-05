using System.Text.Json;
using TaxTrack.Application.Interfaces;
using TaxTrack.Domain.Common;
using TaxTrack.Domain.Entities;
using TaxTrack.Infrastructure.Data;

namespace TaxTrack.Infrastructure.Services;

public sealed class AuditService(TaxTrackDbContext dbContext) : IAuditService
{
    public async Task LogAsync(
        Guid actorUserId,
        Guid? companyId,
        AuditEventType eventType,
        string correlationId,
        object metadata,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken)
    {
        dbContext.AuditLogEvents.Add(new AuditLogEvent
        {
            ActorUserId = actorUserId,
            CompanyId = companyId,
            EventType = eventType,
            CorrelationId = correlationId,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            MetadataJson = JsonSerializer.Serialize(metadata)
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
