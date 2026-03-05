using TaxTrack.Domain.Common;

namespace TaxTrack.Domain.Entities;

public sealed class UploadJob
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CompanyId { get; set; }
    public Guid RequestedByUserId { get; set; }
    public DatasetType DatasetType { get; set; }
    public UploadJobStatus Status { get; set; } = UploadJobStatus.Queued;
    public int AcceptedRows { get; set; }
    public int RejectedRows { get; set; }
    public int EvidenceCompleteness { get; set; }
    public bool InsufficientEvidence { get; set; }
    public string? ValidationErrorsJson { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
