using Claims.Application.Abstractions;
using Claims.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Claims.Infrastructure.Persistence;

public sealed class ClaimRepository : IClaimRepository
{
    private readonly ClaimsDbContext _context;

    public ClaimRepository(ClaimsDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Claim>> GetAllAsync(CancellationToken cancellationToken) =>
        await _context.Claims.AsNoTracking().ToListAsync(cancellationToken);

    public Task<Claim?> GetByIdAsync(string id, CancellationToken cancellationToken) =>
        _context.Claims.SingleOrDefaultAsync(claim => claim.Id == id, cancellationToken);

    public async Task AddAsync(Claim claim, CancellationToken cancellationToken)
    {
        _context.Claims.Add(claim);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Claim claim, CancellationToken cancellationToken)
    {
        _context.Claims.Remove(claim);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
