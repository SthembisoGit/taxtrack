namespace TaxTrack.Api.Contracts;

public sealed class AnalyzeRiskApiRequest
{
    public Guid CompanyId { get; init; }
    public DateOnly? PeriodStart { get; init; }
    public DateOnly? PeriodEnd { get; init; }
}
