using TaxTrack.Domain.Common;
using TaxTrack.Infrastructure.Services;

namespace TaxTrack.Tests;

public sealed class RiskLevelTests
{
    [Theory]
    [InlineData(0, RiskLevel.Low)]
    [InlineData(40, RiskLevel.Low)]
    [InlineData(41, RiskLevel.Medium)]
    [InlineData(70, RiskLevel.Medium)]
    [InlineData(71, RiskLevel.High)]
    [InlineData(100, RiskLevel.High)]
    public void DetermineRiskLevel_MapsBoundaries(int riskScore, RiskLevel expected)
    {
        var actual = RiskService.DetermineRiskLevel(riskScore);
        Assert.Equal(expected, actual);
    }
}
