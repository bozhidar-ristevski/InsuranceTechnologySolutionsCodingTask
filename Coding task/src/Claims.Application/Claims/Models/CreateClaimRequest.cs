using Claims.Domain.Enums;

namespace Claims.Application.Claims.Models;

public sealed class CreateClaimRequest
{
    public string CoverId { get; init; } = null!;
    public DateTime Created { get; init; }
    public string Name { get; init; } = null!;
    public ClaimType Type { get; init; }
    public decimal DamageCost { get; init; }
}
