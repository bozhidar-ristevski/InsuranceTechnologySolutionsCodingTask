using Claims.Domain.Enums;
using Claims.Domain.Services;
using Xunit;

namespace Claims.Tests;

public sealed class PremiumCalculatorTests
{
    private readonly PremiumCalculator _calculator = new();
    private static readonly DateTime Start = new(2026, 1, 1);

    [Theory]
    [InlineData(CoverType.Yacht, 1.10)]
    [InlineData(CoverType.PassengerShip, 1.20)]
    [InlineData(CoverType.Tanker, 1.50)]
    [InlineData(CoverType.ContainerShip, 1.30)]
    [InlineData(CoverType.BulkCarrier, 1.30)]
    public void First_days_use_type_multiplier_on_base_rate(CoverType type, decimal multiplier)
    {
        var end = Start.AddDays(10);

        var premium = _calculator.Compute(Start, end, type);

        Assert.Equal(10 * 1250m * multiplier, premium);
    }

    [Fact]
    public void Yacht_applies_five_percent_discount_after_first_30_days()
    {
        var end = Start.AddDays(40);

        var premium = _calculator.Compute(Start, end, CoverType.Yacht);

        var dayRate = 1250m * 1.10m;
        var expected = 30 * dayRate + 10 * dayRate * 0.95m;
        Assert.Equal(expected, premium);
    }

    [Fact]
    public void Other_types_apply_two_percent_discount_after_first_30_days()
    {
        var end = Start.AddDays(40);

        var premium = _calculator.Compute(Start, end, CoverType.Tanker);

        var dayRate = 1250m * 1.50m;
        var expected = 30 * dayRate + 10 * dayRate * 0.98m;
        Assert.Equal(expected, premium);
    }

    [Fact]
    public void Yacht_applies_additional_three_percent_discount_after_180_days()
    {
        var end = Start.AddDays(200);

        var premium = _calculator.Compute(Start, end, CoverType.Yacht);

        var dayRate = 1250m * 1.10m;
        var expected = 30 * dayRate + 150 * dayRate * 0.95m + 20 * dayRate * 0.92m;
        Assert.Equal(expected, premium);
    }

    [Fact]
    public void Other_types_apply_additional_one_percent_discount_after_180_days()
    {
        var end = Start.AddDays(200);

        var premium = _calculator.Compute(Start, end, CoverType.PassengerShip);

        var dayRate = 1250m * 1.20m;
        var expected = 30 * dayRate + 150 * dayRate * 0.98m + 20 * dayRate * 0.97m;
        Assert.Equal(expected, premium);
    }

    [Fact]
    public void Same_start_and_end_date_has_no_premium()
    {
        var premium = _calculator.Compute(Start, Start, CoverType.Yacht);
        Assert.Equal(0m, premium);
    }
}
