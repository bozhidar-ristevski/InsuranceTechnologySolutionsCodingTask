using Claims.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using MongoDB.EntityFrameworkCore.Extensions;

namespace Claims.Infrastructure.Persistence;

public sealed class ClaimsDbContext : DbContext
{
    public ClaimsDbContext(DbContextOptions<ClaimsDbContext> options)
        : base(options)
    {
    }

    public DbSet<Claim> Claims => Set<Claim>();
    public DbSet<Cover> Covers => Set<Cover>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Claim>(entity =>
        {
            entity.ToCollection("claims");
            entity.HasKey(claim => claim.Id);
            entity.Property(claim => claim.CoverId).HasElementName("coverId");
            entity.Property(claim => claim.Created).HasElementName("created");
            entity.Property(claim => claim.Name).HasElementName("name");
            entity.Property(claim => claim.Type).HasElementName("claimType");
            entity.Property(claim => claim.DamageCost).HasElementName("damageCost");
        });

        modelBuilder.Entity<Cover>(entity =>
        {
            entity.ToCollection("covers");
            entity.HasKey(cover => cover.Id);
            entity.Property(cover => cover.StartDate).HasElementName("startDate");
            entity.Property(cover => cover.EndDate).HasElementName("endDate");
            entity.Property(cover => cover.Type).HasElementName("claimType");
            entity.Property(cover => cover.Premium).HasElementName("premium");
        });
    }
}
