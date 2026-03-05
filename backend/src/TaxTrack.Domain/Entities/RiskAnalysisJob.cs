using TaxTrack.Domain.Common;

namespace TaxTrack.Domain.Entities;

public sealed class RiskAnalysisJob
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CompanyId { get; set; }
    public Guid RequestedByUserId { get; set; }
    public RiskAnalysisJobStatus Status { get; set; } = RiskAnalysisJobStatus.Queued;
    public Guid? ResultId { get; set; }
    public DateOnly? PeriodStart { get; set; }
    public DateOnly? PeriodEnd { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
