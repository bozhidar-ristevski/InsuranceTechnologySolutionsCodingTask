using Claims.Application.Abstractions;
using Claims.Application.Covers.Models;
using Claims.Application.Exceptions;

namespace Claims.Application.Covers;

public static class CoverValidator
{
    public static void Validate(CreateCoverRequest request, IClock clock)
    {
        var errors = new List<string>();
        var today = clock.UtcNow.Date;
        var startDate = request.StartDate.Date;
        var endDate = request.EndDate.Date;

        if (startDate < today)
        {
            errors.Add("StartDate cannot be in the past.");
        }

        if (endDate > startDate.AddYears(1))
        {
            errors.Add("Total insurance period cannot exceed 1 year.");
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors.ToArray());
        }
    }
}
