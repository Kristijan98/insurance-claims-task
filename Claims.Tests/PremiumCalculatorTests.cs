using Claims.Services;
using Xunit;

namespace Claims.Tests;

public class PremiumCalculatorTests
{
    private static readonly DateTime Start = new(2025, 1, 1);

    [Theory]
    [InlineData(CoverType.Yacht, 1375)]
    [InlineData(CoverType.PassengerShip, 1500)]
    [InlineData(CoverType.Tanker, 1875)]
    [InlineData(CoverType.ContainerShip, 1625)]
    [InlineData(CoverType.BulkCarrier, 1625)]
    public void Compute_OneDay_AppliesVesselMultiplier(CoverType type, decimal expected)
    {
        var premium = PremiumCalculator.Compute(Start, Start.AddDays(1), type);

        Assert.Equal(expected, premium);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(-5, 0)]
    [InlineData(30, 48_750)]
    [InlineData(31, 50_342.5)]
    [InlineData(180, 287_625)]
    [InlineData(181, 289_201.25)]
    [InlineData(365, 579_231.25)]
    [InlineData(366, 580_807.5)]
    public void Compute_ContainerShip_AppliesTierDiscounts(int days, decimal expected)
    {
        var premium = PremiumCalculator.Compute(Start, Start.AddDays(days), CoverType.ContainerShip);

        Assert.Equal(expected, premium);
    }

    [Fact]
    public void Compute_Yacht_UsesYachtDiscounts()
    {
        // 30 * 1375 + 150 * 1375 * 0.95 + 1 * 1375 * 0.92
        var premium = PremiumCalculator.Compute(Start, Start.AddDays(181), CoverType.Yacht);

        Assert.Equal(238_452.5m, premium);
    }

    [Fact]
    public void Compute_IgnoresTimeOfDay()
    {
        var premium = PremiumCalculator.Compute(Start.AddHours(23), Start.AddDays(1).AddHours(1), CoverType.Tanker);

        Assert.Equal(1875m, premium);
    }
}
