namespace TaxTrack.Infrastructure.Options;

public sealed class TaxPolicyOptions
{
    public const string SectionName = "TaxPolicy";

    public string Version { get; set; } = "ZA-2026.01";
    public DateOnly EffectiveDate { get; set; } = new(2026, 2, 25);
    public decimal VatStandardRate { get; set; } = 0.15m;
    public decimal VatCompulsoryRegistrationThresholdZar { get; set; } = 2_300_000m;
    public decimal VatVoluntaryRegistrationThresholdZar { get; set; } = 120_000m;
    public int Emp201DueDayOfMonth { get; set; } = 7;
    public List<DateOnly> PublicHolidayDates { get; set; } = [];
}
