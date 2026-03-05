namespace TaxTrack.Infrastructure.Options;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "TaxTrack";
    public string Audience { get; set; } = "TaxTrack.Client";
    public string SigningKey { get; set; } = "ChangeThisInProduction_AtLeast32Chars!";
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 7;
}
