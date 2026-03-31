namespace TaxTrack.Infrastructure.Options;

public sealed class ReportDownloadOptions
{
    public const string SectionName = "ReportDownloads";

    public string SigningKey { get; set; } = "ChangeThisForProduction_ReportDownloads_AtLeast32Chars!";
}
