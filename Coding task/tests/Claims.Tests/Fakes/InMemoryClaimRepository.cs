using Claims.Application.Abstractions;
using Claims.Domain.Entities;

namespace Claims.Tests.Fakes;

public sealed class InMemoryClaimRepository : IClaimRepository
{
    private readonly Dictionary<string, Claim> _items = new();

    public Task<IReadOnlyList<Claim>> GetAllAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Claim>>(_items.Values.ToList());

    public Task<Claim?> GetByIdAsync(string id, CancellationToken cancellationToken) =>
        Task.FromResult(_items.GetValueOrDefault(id));

    public Task AddAsync(Claim claim, CancellationToken cancellationToken)
    {
        _items[claim.Id] = claim;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Claim claim, CancellationToken cancellationToken)
    {
        _items.Remove(claim.Id);
        return Task.CompletedTask;
    }
}
