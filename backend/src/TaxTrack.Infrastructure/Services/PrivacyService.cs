using Microsoft.EntityFrameworkCore;
using TaxTrack.Application.Exceptions;
using TaxTrack.Application.Interfaces;
using TaxTrack.Application.Models;
using TaxTrack.Domain.Common;
using TaxTrack.Domain.Entities;
using TaxTrack.Infrastructure.Data;

namespace TaxTrack.Infrastructure.Services;

public sealed class PrivacyService(
    TaxTrackDbContext dbContext,
    ICompanyAccessService companyAccessService,
    IAuditService auditService) : IPrivacyService
{
    public async Task<IReadOnlyCollection<DataSubjectRequestResponse>> ListRequestsAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await dbContext.DataSubjectRequests
            .Where(x => x.RequesterUserId == userId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => Map(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<DataSubjectRequestResponse> CreateRequestAsync(
        CreateDataSubjectRequestCommand command,
        string correlationId,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken)
    {
        if (command.CompanyId.HasValue)
        {
            var canAccess = await companyAccessService.CanAccessCompanyAsync(
                command.RequesterUserId,
                command.CompanyId.Value,
                cancellationToken);
            if (!canAccess)
            {
                throw new ForbiddenException("User cannot create a data request for this company.");
            }
        }

        var request = new DataSubjectRequest
        {
            RequesterUserId = command.RequesterUserId,
            CompanyId = command.CompanyId,
            RequestType = command.RequestType,
            Reason = command.Reason
        };

        dbContext.DataSubjectRequests.Add(request);
        await dbContext.SaveChangesAsync(cancellationToken);

        var eventType = command.RequestType == DataSubjectRequestType.Export
            ? AuditEventType.DataExportRequested
            : AuditEventType.DataDeletionRequested;

        await auditService.LogAsync(
            command.RequesterUserId,
            command.CompanyId,
            eventType,
            correlationId,
            new { request.Id, command.RequestType },
            ipAddress,
            userAgent,
            cancellationToken);

        return Map(request);
    }

    public async Task<DataSubjectRequestResponse?> GetRequestAsync(Guid userId, Guid requestId, CancellationToken cancellationToken)
    {
        var request = await dbContext.DataSubjectRequests
            .FirstOrDefaultAsync(x => x.Id == requestId && x.RequesterUserId == userId, cancellationToken);

        return request is null ? null : Map(request);
    }

    private static DataSubjectRequestResponse Map(DataSubjectRequest request)
    {
        return new DataSubjectRequestResponse(
            request.Id,
            request.CompanyId,
            request.RequestType,
            request.Status,
            request.Reason,
            request.CreatedAtUtc,
            request.UpdatedAtUtc,
            request.ResolutionNote);
    }
}
