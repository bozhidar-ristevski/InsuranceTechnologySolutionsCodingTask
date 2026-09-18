using Claims.Application.Claims.Models;
using Claims.Domain.Entities;

namespace Claims.Application.Claims;

public static class ClaimMappings
{
    public static ClaimDto ToDto(this Claim claim) => new()
    {
        Id = claim.Id,
        CoverId = claim.CoverId,
        Created = claim.Created,
        Name = claim.Name,
        Type = claim.Type,
        DamageCost = claim.DamageCost
    };
}
