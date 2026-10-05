using Claims.Domain;
using Xunit;

namespace Claims.UnitTests;

public class DomainRulesTests
{
    [Theory]
    [InlineData(CoverType.Yacht, 30, 1_375)]
    [InlineData(CoverType.PassengerShip, 30, 1_500)]
    [InlineData(CoverType.Tanker, 30, 1_875)]
    [InlineData(CoverType.ContainerShip, 30, 1_625)]
    public void Premium_first_band_uses_vessel_multiplier(CoverType type, int days, decimal dailyRate)
    {
        var start = new DateOnly(2026, 1, 1);
        Assert.Equal(dailyRate * days, PremiumCalculator.Calculate(start, start.AddDays(days), type));
    }

    [Theory]
    [InlineData(CoverType.Yacht, 1_375, 1_306.25, 1_265)]
    [InlineData(CoverType.PassengerShip, 1_500, 1_470, 1_455)]
    [InlineData(CoverType.Tanker, 1_875, 1_837.5, 1_818.75)]
    [InlineData(CoverType.BulkCarrier, 1_625, 1_592.5, 1_576.25)]
    public void Premium_uses_progressive_bands(CoverType type, decimal day1, decimal day2, decimal day3)
    {
        var start = new DateOnly(2026, 1, 1);
        var expected = 30 * day1 + 150 * day2 + 2 * day3;
        Assert.Equal(expected, PremiumCalculator.Calculate(start, start.AddDays(182), type));
    }

    [Fact] public void Premium_zero_days_is_zero() => Assert.Equal(0m, PremiumCalculator.Calculate(new(2026, 1, 1), new(2026, 1, 1), CoverType.Yacht));
    [Fact] public void Premium_rejects_negative_period() => Assert.Throws<DomainValidationException>(() => PremiumCalculator.Calculate(new(2026, 1, 2), new(2026, 1, 1), CoverType.Yacht));

}
