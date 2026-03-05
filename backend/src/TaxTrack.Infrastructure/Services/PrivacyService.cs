using Microsoft.EntityFrameworkCore;
using TaxTrack.Application.Interfaces;
using TaxTrack.Application.Models;
using TaxTrack.Domain.Common;
using TaxTrack.Domain.Entities;
using TaxTrack.Infrastructure.Data;

namespace TaxTrack.Infrastructure.Services;

public sealed class PrivacyService(
    TaxTrackDbContext dbContext,
    IAuditService auditService) : IPrivacyService
{
    public async Task<DataSubjectRequestResponse> CreateRequestAsync(
        CreateDataSubjectRequestCommand command,
        string correlationId,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken)
    {
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
            request.RequestType,
            request.Status,
            request.CreatedAtUtc,
            request.UpdatedAtUtc,
            request.ResolutionNote);
    }
}
