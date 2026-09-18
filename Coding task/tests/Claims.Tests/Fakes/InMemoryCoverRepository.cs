using Claims.Application.Abstractions;
using Claims.Domain.Entities;

namespace Claims.Tests.Fakes;

public sealed class InMemoryCoverRepository : ICoverRepository
{
    private readonly Dictionary<string, Cover> _items = new();

    public void Seed(Cover cover) => _items[cover.Id] = cover;

    public Task<IReadOnlyList<Cover>> GetAllAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Cover>>(_items.Values.ToList());

    public Task<Cover?> GetByIdAsync(string id, CancellationToken cancellationToken) =>
        Task.FromResult(_items.GetValueOrDefault(id));

    public Task AddAsync(Cover cover, CancellationToken cancellationToken)
    {
        _items[cover.Id] = cover;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Cover cover, CancellationToken cancellationToken)
    {
        _items.Remove(cover.Id);
        return Task.CompletedTask;
    }
}
