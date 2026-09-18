using Claims.Domain.Enums;

namespace Claims.Domain.Entities;

public class Cover
{
    private Cover()
    {
    }

    public string Id { get; private set; } = null!;
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public CoverType Type { get; private set; }
    public decimal Premium { get; private set; }

    public bool ContainsDate(DateTime date)
    {
        var day = date.Date;
        return day >= StartDate.Date && day <= EndDate.Date;
    }

    public static Cover Create(
        DateTime startDate,
        DateTime endDate,
        CoverType type,
        decimal premium)
    {
        return new Cover
        {
            Id = Guid.NewGuid().ToString(),
            StartDate = startDate.Date,
            EndDate = endDate.Date,
            Type = type,
            Premium = premium
        };
    }
}
