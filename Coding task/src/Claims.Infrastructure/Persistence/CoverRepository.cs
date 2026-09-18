using Claims.Application.Abstractions;
using Claims.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Claims.Infrastructure.Persistence;

public sealed class CoverRepository : ICoverRepository
{
    private readonly ClaimsDbContext _context;

    public CoverRepository(ClaimsDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Cover>> GetAllAsync(CancellationToken cancellationToken) =>
        await _context.Covers.AsNoTracking().ToListAsync(cancellationToken);

    public Task<Cover?> GetByIdAsync(string id, CancellationToken cancellationToken) =>
        _context.Covers.SingleOrDefaultAsync(cover => cover.Id == id, cancellationToken);

    public async Task AddAsync(Cover cover, CancellationToken cancellationToken)
    {
        _context.Covers.Add(cover);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Cover cover, CancellationToken cancellationToken)
    {
        _context.Covers.Remove(cover);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
