using Claims.Domain.Entities;
using Claims.Domain.Enums;

namespace Claims.Application.Covers.Models;

public sealed class CoverDto
{
    public required string Id { get; init; }
    public required DateTime StartDate { get; init; }
    public required DateTime EndDate { get; init; }
    public required CoverType Type { get; init; }
    public required decimal Premium { get; init; }

    public static CoverDto From(Cover cover) => new()
    {
        Id = cover.Id,
        StartDate = cover.StartDate,
        EndDate = cover.EndDate,
        Type = cover.Type,
        Premium = cover.Premium
    };
}
