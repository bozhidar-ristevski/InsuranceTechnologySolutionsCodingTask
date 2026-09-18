using Claims.Domain.Enums;

namespace Claims.Domain.Entities;

public class Claim
{
    private Claim()
    {
    }

    public string Id { get; private set; } = null!;
    public string CoverId { get; private set; } = null!;
    public DateTime Created { get; private set; }
    public string Name { get; private set; } = null!;
    public ClaimType Type { get; private set; }
    public decimal DamageCost { get; private set; }

    public static Claim Create(
        string coverId,
        DateTime created,
        string name,
        ClaimType type,
        decimal damageCost)
    {
        return new Claim
        {
            Id = Guid.NewGuid().ToString(),
            CoverId = coverId,
            Created = created.Date,
            Name = name,
            Type = type,
            DamageCost = damageCost
        };
    }
}
