using Microsoft.EntityFrameworkCore;
using TaxTrack.Application.Exceptions;
using TaxTrack.Application.Interfaces;
using TaxTrack.Application.Models;
using TaxTrack.Infrastructure.Data;

namespace TaxTrack.Infrastructure.Services;

public sealed class AuditQueryService(
    TaxTrackDbContext dbContext,
    ICompanyAccessService companyAccessService) : IAuditQueryService
{
    public async Task<IReadOnlyCollection<AuditLogEventResponse>> ListAsync(
        Guid userId,
        Guid? companyId,
        int limit,
        CancellationToken cancellationToken)
    {
        var safeLimit = Math.Clamp(limit, 1, 100);

        IQueryable<Domain.Entities.AuditLogEvent> query = dbContext.AuditLogEvents.AsNoTracking();

        if (companyId.HasValue)
        {
            var canAccess = await companyAccessService.CanAccessCompanyAsync(userId, companyId.Value, cancellationToken);
            if (!canAccess)
            {
                throw new ForbiddenException("User cannot access this company's audit log.");
            }

            query = query.Where(x => x.CompanyId == companyId.Value);
        }
        else
        {
            query = query.Where(x => x.ActorUserId == userId && x.CompanyId == null);
        }

        var events = await query
            .OrderByDescending(x => x.EventTimeUtc)
            .Take(safeLimit)
            .Join(
                dbContext.Users.AsNoTracking(),
                audit => audit.ActorUserId,
                user => user.Id,
                (audit, user) => new AuditLogEventResponse(
                    audit.Id,
                    audit.ActorUserId,
                    user.Email,
                    audit.CompanyId,
                    audit.EventType,
                    audit.EventTimeUtc,
                    audit.CorrelationId,
                    audit.MetadataJson,
                    audit.IpAddress,
                    audit.UserAgent))
            .ToArrayAsync(cancellationToken);

        return events;
    }
}
