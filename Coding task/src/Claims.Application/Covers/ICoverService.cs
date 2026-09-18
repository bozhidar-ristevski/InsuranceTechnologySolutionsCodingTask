using Claims.Application.Covers.Models;
using Claims.Domain.Enums;

namespace Claims.Application.Covers;

public interface ICoverService
{
    Task<IReadOnlyList<CoverDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<CoverDto> GetByIdAsync(string id, CancellationToken cancellationToken);
    Task<CoverDto> CreateAsync(CreateCoverRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(string id, CancellationToken cancellationToken);
    decimal ComputePremium(DateTime startDate, DateTime endDate, CoverType coverType);
}
