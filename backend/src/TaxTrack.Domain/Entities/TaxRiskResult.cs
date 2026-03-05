using TaxTrack.Domain.Common;

namespace TaxTrack.Domain.Entities;

public sealed class TaxRiskResult
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CompanyId { get; set; }
    public int RiskScore { get; set; }
    public int RegulatoryScore { get; set; }
    public int HeuristicScore { get; set; }
    public RiskLevel RiskLevel { get; set; }
    public required string TaxPolicyVersion { get; set; }
    public DateOnly PolicyEffectiveDate { get; set; }
    public int EvidenceCompleteness { get; set; }
    public bool InsufficientEvidence { get; set; }
    public DateTime GeneratedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<RiskAlert> Alerts { get; set; } = new List<RiskAlert>();
}
