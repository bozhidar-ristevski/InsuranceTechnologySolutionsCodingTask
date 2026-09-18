using Claims.Domain.Enums;

namespace Claims.Application.Claims.Models;

public sealed class ClaimDto
{
    public required string Id { get; init; }
    public required string CoverId { get; init; }
    public required DateTime Created { get; init; }
    public required string Name { get; init; }
    public required ClaimType Type { get; init; }
    public required decimal DamageCost { get; init; }
}
