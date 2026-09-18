using Claims.Domain.Enums;

namespace Claims.Application.Covers.Models;

public sealed class CreateCoverRequest
{
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public CoverType Type { get; init; }
}
