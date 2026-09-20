using Claims.Application.Claims.Models;
using Claims.Application.Exceptions;
using Claims.Domain.Entities;

namespace Claims.Application.Claims;

public static class ClaimValidator
{
    public const decimal MaxDamageCost = 100_000m;

    public static void Validate(CreateClaimRequest request, Cover cover)
    {
        var errors = new List<string>();

        if (request.DamageCost > MaxDamageCost)
        {
            errors.Add("DamageCost cannot exceed 100.000.");
        }

        if (request.DamageCost <= 0)
        {
            errors.Add("DamageCost must be greater than 0.");
        }

        if (!cover.ContainsDate(request.Created))
        {
            errors.Add("Created date must be within the period of the related Cover.");
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors.ToArray());
        }
    }
}
