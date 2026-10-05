namespace Claims.Domain;

/// <summary>Calculates tiered daily premiums. The second and third bands receive cumulative discounts.</summary>
public static class PremiumCalculator
{
    public const decimal BaseDayRate = 1_250m;
    public static decimal Calculate(DateOnly start, DateOnly end, CoverType type)
    {
        var days = end.DayNumber - start.DayNumber;
        if (days < 0) throw new DomainValidationException("End date cannot be before start date.");
        var typeRate = type switch { CoverType.Yacht => 1.10m, CoverType.PassengerShip => 1.20m, CoverType.Tanker => 1.50m, _ => 1.30m };
        var yacht = type == CoverType.Yacht;
        var middleDiscount = yacht ? 0.05m : 0.02m;
        var finalDiscount = yacht ? 0.08m : 0.03m;
        var firstBandDays = Math.Min(days, 30);
        var secondBandDays = Math.Min(Math.Max(days - 30, 0), 150);
        var remainingDays = Math.Max(days - 180, 0);
        return BaseDayRate * typeRate * firstBandDays
            + BaseDayRate * typeRate * (1m - middleDiscount) * secondBandDays
            + BaseDayRate * typeRate * (1m - finalDiscount) * remainingDays;
    }
}
