using Claims.Domain.Enums;

namespace Claims.Domain.Services;

/// <summary>
/// Computes cover premium from the type of insured object and the length of the period.
/// </summary>
public sealed class PremiumCalculator : IPremiumCalculator
{
    public const decimal DailyBaseRate = 1250m;

    private const int DaysAtFullPrice = 30;
    private const int DaysAtFirstDiscount = 150;
    private const int DaysBeforeLongerStayDiscount = DaysAtFullPrice + DaysAtFirstDiscount;

    public decimal Compute(DateTime startDate, DateTime endDate, CoverType coverType)
    {
        var lengthOfCoverInDays = (endDate.Date - startDate.Date).Days;
        if (lengthOfCoverInDays <= 0)
        {
            return 0m;
        }

        var dailyRate = DailyBaseRate * PriceMultiplierFor(coverType);
        var discountAfterFirstMonth = DiscountAfterTheFirstMonth(coverType);
        var discountAfterSixMonths = discountAfterFirstMonth + ExtraDiscountForLongerStays(coverType);

        var totalPremium = 0m;
        for (var dayOfCover = 0; dayOfCover < lengthOfCoverInDays; dayOfCover++)
        {
            totalPremium += dayOfCover switch
            {
                < DaysAtFullPrice => dailyRate,
                < DaysBeforeLongerStayDiscount => dailyRate * (1m - discountAfterFirstMonth),
                _ => dailyRate * (1m - discountAfterSixMonths)
            };
        }

        return totalPremium;
    }

    private static decimal PriceMultiplierFor(CoverType coverType) => coverType switch
    {
        CoverType.Yacht => 1.10m,
        CoverType.PassengerShip => 1.20m,
        CoverType.Tanker => 1.50m,
        _ => 1.30m
    };

    private static decimal DiscountAfterTheFirstMonth(CoverType coverType) =>
        coverType == CoverType.Yacht ? 0.05m : 0.02m;

    private static decimal ExtraDiscountForLongerStays(CoverType coverType) =>
        coverType == CoverType.Yacht ? 0.03m : 0.01m;
}
