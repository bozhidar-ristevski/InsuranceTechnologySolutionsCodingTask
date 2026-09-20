using Claims.Application.Abstractions;
using Claims.Application.Covers.Models;
using Claims.Application.Exceptions;
using Claims.Domain.Enums;

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

        if (endDate <= startDate)
        {
            errors.Add("EndDate must be greater than StartDate.");
        }

        if (endDate > startDate.AddYears(1))
        {
            errors.Add("Total insurance period cannot exceed 1 year.");
        }

        if (!Enum.IsDefined<CoverType>((CoverType)request.Type))
        {
            string enumName = typeof(CoverType).Name;

            var allowedValues = Enum.GetValues<CoverType>()
                .Select(e => $"{e} ({Convert.ChangeType(e, e.GetTypeCode())})");
            string allowedValuesList = string.Join(", ", allowedValues);

            errors.Add($"Value '{request.Type}' is not a valid underlying value for the enum '{enumName}'. Allowed values are: {allowedValuesList}.");
            
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors.ToArray());
        }
    }
}
