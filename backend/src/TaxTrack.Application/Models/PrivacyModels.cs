using TaxTrack.Domain.Common;

namespace TaxTrack.Application.Models;

public sealed class CreateDataSubjectRequestCommand
{
    public Guid RequesterUserId { get; init; }
    public Guid? CompanyId { get; init; }
    public DataSubjectRequestType RequestType { get; init; }
    public string? Reason { get; init; }
}

public sealed record DataSubjectRequestResponse(
    Guid RequestId,
    Guid? CompanyId,
    DataSubjectRequestType RequestType,
    DataSubjectRequestStatus Status,
    string? Reason,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    string? ResolutionNote);
