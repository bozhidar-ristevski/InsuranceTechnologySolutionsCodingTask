using Claims.Domain.Entities;

namespace Claims.Application.Abstractions;

public interface ICoverRepository
{
    Task<IReadOnlyList<Cover>> GetAllAsync(CancellationToken cancellationToken);
    Task<Cover?> GetByIdAsync(string id, CancellationToken cancellationToken);
    Task AddAsync(Cover cover, CancellationToken cancellationToken);
    Task DeleteAsync(Cover cover, CancellationToken cancellationToken);
}
