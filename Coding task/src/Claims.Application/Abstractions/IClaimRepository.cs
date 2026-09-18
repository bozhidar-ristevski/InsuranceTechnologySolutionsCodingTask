using Claims.Domain.Entities;

namespace Claims.Application.Abstractions;

public interface IClaimRepository
{
    Task<IReadOnlyList<Claim>> GetAllAsync(CancellationToken cancellationToken);
    Task<Claim?> GetByIdAsync(string id, CancellationToken cancellationToken);
    Task AddAsync(Claim claim, CancellationToken cancellationToken);
    Task DeleteAsync(Claim claim, CancellationToken cancellationToken);
}
