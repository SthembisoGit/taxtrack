using TaxTrack.Domain.Common;

namespace TaxTrack.Application.Models;

public sealed class UploadCommand
{
    public Guid UserId { get; init; }
    public Guid CompanyId { get; init; }
    public DatasetType DatasetType { get; init; }
    public required string FileName { get; init; }
    public required Stream Content { get; init; }
    public required string IdempotencyKey { get; init; }
}

public sealed record UploadAcceptedResponse(
    Guid UploadId,
    Guid CompanyId,
    DatasetType DatasetType,
    UploadJobStatus Status,
    DateTime ReceivedAtUtc);

public sealed record UploadStatusResponse(
    Guid UploadId,
    Guid CompanyId,
    UploadJobStatus Status,
    int AcceptedRows,
    int RejectedRows,
    int EvidenceCompleteness,
    bool InsufficientEvidence,
    DateTime UpdatedAtUtc);

public sealed record ValidationIssue(int RowNumber, string ColumnName, string ErrorCode, string Message);
