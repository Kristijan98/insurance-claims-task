namespace Claims.Services;

/// <summary>
/// Calculates the premium for a cover.
/// </summary>
/// <remarks>
/// The day rate is 1250 multiplied by a vessel-type factor. Days are then priced in three tiers:
/// the first 30 days at the full rate, the next 150 days with a small discount,
/// and every day after that with a larger discount.
/// </remarks>
public static class PremiumCalculator
{
    private const decimal BaseDayRate = 1250m;

    private const int FullRateDays = 30;
    private const int FirstDiscountEndDay = 180;

    /// <summary>
    /// Returns the total premium for a cover of the given type and period.
    /// The period length is the number of calendar days between <paramref name="startDate"/>
    /// and <paramref name="endDate"/>; time of day is ignored. A zero or negative period returns 0.
    /// </summary>
    public static decimal Compute(DateTime startDate, DateTime endDate, CoverType coverType)
    {
        var totalDays = (endDate.Date - startDate.Date).Days;
        if (totalDays <= 0)
        {
            return 0m;
        }

        var dayRate = BaseDayRate * GetTypeMultiplier(coverType);

        var fullRateDays = Math.Min(totalDays, FullRateDays);
        var firstDiscountDays = Math.Clamp(totalDays - FullRateDays, 0, FirstDiscountEndDay - FullRateDays);
        var secondDiscountDays = Math.Max(totalDays - FirstDiscountEndDay, 0);

        return dayRate * fullRateDays
             + dayRate * GetFirstDiscountFactor(coverType) * firstDiscountDays
             + dayRate * GetSecondDiscountFactor(coverType) * secondDiscountDays;
    }

    private static decimal GetTypeMultiplier(CoverType coverType) => coverType switch
    {
        CoverType.Yacht => 1.1m,
        CoverType.PassengerShip => 1.2m,
        CoverType.Tanker => 1.5m,
        _ => 1.3m
    };

    /// <summary>Price factor for days 31 to 180.</summary>
    private static decimal GetFirstDiscountFactor(CoverType coverType) =>
        coverType == CoverType.Yacht ? 0.95m : 0.98m;

    /// <summary>Price factor for day 181 onwards.</summary>
    private static decimal GetSecondDiscountFactor(CoverType coverType) =>
        coverType == CoverType.Yacht ? 0.92m : 0.97m;
}
