namespace TaxTrack.Infrastructure.Options;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    public bool UseInMemory { get; set; }
}
