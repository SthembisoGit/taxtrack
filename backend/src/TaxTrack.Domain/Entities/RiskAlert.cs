using TaxTrack.Domain.Common;

namespace TaxTrack.Domain.Entities;

public sealed class RiskAlert
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RiskResultId { get; set; }
    public required string RuleCode { get; set; }
    public RuleClass RuleClass { get; set; }
    public required string Type { get; set; }
    public required string Description { get; set; }
    public AlertSeverity Severity { get; set; }
    public required string Recommendation { get; set; }
    public required string EvidenceJson { get; set; }

    public TaxRiskResult? RiskResult { get; set; }
}
