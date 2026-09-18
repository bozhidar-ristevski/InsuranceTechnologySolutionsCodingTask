using Claims.Application.Abstractions;
using Claims.Application.Claims;
using Claims.Application.Claims.Models;
using Claims.Application.Exceptions;
using Claims.Domain.Entities;
using Claims.Domain.Enums;
using Claims.Tests.Fakes;
using Xunit;

namespace Claims.Tests;

public sealed class ClaimServiceTests
{
    private readonly InMemoryClaimRepository _claims = new();
    private readonly InMemoryCoverRepository _covers = new();
    private readonly RecordingAuditPublisher _audits = new();
    private readonly ClaimService _sut;

    public ClaimServiceTests()
    {
        _sut = new ClaimService(_claims, _covers, _audits);
    }

    [Fact]
    public async Task Create_rejects_damage_cost_above_limit()
    {
        var cover = SeedCover();

        var exception = await Assert.ThrowsAsync<ValidationException>(() => _sut.CreateAsync(new CreateClaimRequest
        {
            CoverId = cover.Id,
            Created = cover.StartDate,
            Name = "Hull",
            Type = ClaimType.Collision,
            DamageCost = 100_000.01m
        }, CancellationToken.None));

        Assert.Contains(exception.Errors, error => error.Contains("100.000", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Create_accepts_damage_cost_at_the_limit()
    {
        var cover = SeedCover();

        var created = await _sut.CreateAsync(new CreateClaimRequest
        {
            CoverId = cover.Id,
            Created = cover.StartDate,
            Name = "Hull",
            Type = ClaimType.Collision,
            DamageCost = 100_000m
        }, CancellationToken.None);

        Assert.Equal(100_000m, created.DamageCost);
    }

    [Fact]
    public async Task Create_rejects_created_date_before_cover_start()
    {
        var cover = SeedCover();

        var exception = await Assert.ThrowsAsync<ValidationException>(() => _sut.CreateAsync(new CreateClaimRequest
        {
            CoverId = cover.Id,
            Created = cover.StartDate.AddDays(-1),
            Name = "Hull",
            Type = ClaimType.Fire,
            DamageCost = 10
        }, CancellationToken.None));

        Assert.Contains(exception.Errors, error => error.Contains("within the period", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Create_rejects_created_date_outside_cover_period()
    {
        var cover = SeedCover();

        await Assert.ThrowsAsync<ValidationException>(() => _sut.CreateAsync(new CreateClaimRequest
        {
            CoverId = cover.Id,
            Created = cover.EndDate.AddDays(1),
            Name = "Hull",
            Type = ClaimType.Fire,
            DamageCost = 10
        }, CancellationToken.None));
    }

    [Fact]
    public async Task Create_persists_claim_and_publishes_audit_without_waiting_on_io_details()
    {
        var cover = SeedCover();

        var created = await _sut.CreateAsync(new CreateClaimRequest
        {
            CoverId = cover.Id,
            Created = cover.StartDate.AddDays(2),
            Name = "Collision",
            Type = ClaimType.Collision,
            DamageCost = 5000
        }, CancellationToken.None);

        Assert.Equal(cover.Id, created.CoverId);
        Assert.Equal(5000, created.DamageCost);
        Assert.Contains(_audits.Items, item => item.Kind == "Claim" && item.HttpRequestType == IAuditPublisher.Post);
        Assert.Single(await _claims.GetAllAsync(CancellationToken.None));
    }

    [Fact]
    public async Task Create_accepts_created_date_on_cover_end()
    {
        var cover = SeedCover();

        var created = await _sut.CreateAsync(new CreateClaimRequest
        {
            CoverId = cover.Id,
            Created = cover.EndDate,
            Name = "Grounding",
            Type = ClaimType.Grounding,
            DamageCost = 1
        }, CancellationToken.None);

        Assert.Equal(cover.EndDate, created.Created);
    }

    [Fact]
    public async Task Create_throws_when_related_cover_is_missing()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.CreateAsync(new CreateClaimRequest
        {
            CoverId = "missing-cover",
            Created = new DateTime(2026, 6, 1),
            Name = "Hull",
            Type = ClaimType.Fire,
            DamageCost = 10
        }, CancellationToken.None));

        Assert.Empty(await _claims.GetAllAsync(CancellationToken.None));
        Assert.Empty(_audits.Items);
    }

    [Fact]
    public async Task Create_does_not_persist_or_audit_when_invalid()
    {
        var cover = SeedCover();

        await Assert.ThrowsAsync<ValidationException>(() => _sut.CreateAsync(new CreateClaimRequest
        {
            CoverId = cover.Id,
            Created = cover.EndDate.AddDays(1),
            Name = "Hull",
            Type = ClaimType.Fire,
            DamageCost = 100_000.01m
        }, CancellationToken.None));

        Assert.Empty(await _claims.GetAllAsync(CancellationToken.None));
        Assert.Empty(_audits.Items);
    }

    [Fact]
    public async Task GetAll_returns_created_claims()
    {
        var cover = SeedCover();
        await _sut.CreateAsync(ValidClaim(cover), CancellationToken.None);

        var claims = await _sut.GetAllAsync(CancellationToken.None);

        Assert.Single(claims);
        Assert.Equal("Collision", claims[0].Name);
    }

    [Fact]
    public async Task GetById_returns_created_claim()
    {
        var cover = SeedCover();
        var created = await _sut.CreateAsync(ValidClaim(cover), CancellationToken.None);

        var fetched = await _sut.GetByIdAsync(created.Id, CancellationToken.None);

        Assert.Equal(created.Id, fetched.Id);
        Assert.Equal(created.CoverId, fetched.CoverId);
        Assert.Equal(created.DamageCost, fetched.DamageCost);
    }

    [Fact]
    public async Task GetById_throws_when_missing()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetByIdAsync("missing", CancellationToken.None));
    }

    [Fact]
    public async Task Delete_removes_claim_and_publishes_audit()
    {
        var cover = SeedCover();
        var created = await _sut.CreateAsync(ValidClaim(cover), CancellationToken.None);

        await _sut.DeleteAsync(created.Id, CancellationToken.None);

        Assert.Empty(await _claims.GetAllAsync(CancellationToken.None));
        Assert.Contains(_audits.Items, item => item.Kind == "Claim" && item.HttpRequestType == IAuditPublisher.Delete && item.Id == created.Id);
    }

    [Fact]
    public async Task Delete_throws_when_missing()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.DeleteAsync("missing", CancellationToken.None));
        Assert.Empty(_audits.Items);
    }

    private static CreateClaimRequest ValidClaim(Cover cover) => new()
    {
        CoverId = cover.Id,
        Created = cover.StartDate.AddDays(2),
        Name = "Collision",
        Type = ClaimType.Collision,
        DamageCost = 5000
    };

    private Cover SeedCover()
    {
        var cover = Cover.Create(new DateTime(2026, 6, 1), new DateTime(2026, 8, 1), CoverType.Yacht, 1);
        _covers.Seed(cover);
        return cover;
    }
}
