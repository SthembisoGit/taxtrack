using TaxTrack.Domain.Common;

namespace TaxTrack.Domain.Entities;

public sealed class DataSubjectRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RequesterUserId { get; set; }
    public Guid? CompanyId { get; set; }
    public DataSubjectRequestType RequestType { get; set; }
    public DataSubjectRequestStatus Status { get; set; } = DataSubjectRequestStatus.Received;
    public string? Reason { get; set; }
    public string? ResolutionNote { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
