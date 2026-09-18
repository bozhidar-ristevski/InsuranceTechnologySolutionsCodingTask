using Claims.Domain.Enums;

namespace Claims.Domain.Services;

public interface IPremiumCalculator
{
    decimal Compute(DateTime startDate, DateTime endDate, CoverType coverType);
}
