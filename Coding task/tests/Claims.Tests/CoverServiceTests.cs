using Claims.Application.Abstractions;
using Claims.Application.Covers;
using Claims.Application.Covers.Models;
using Claims.Application.Exceptions;
using Claims.Domain.Enums;
using Claims.Domain.Services;
using Claims.Tests.Fakes;
using Xunit;

namespace Claims.Tests;

public sealed class CoverServiceTests
{
    private readonly InMemoryCoverRepository _covers = new();
    private readonly RecordingAuditPublisher _audits = new();
    private readonly CoverService _sut;

    public CoverServiceTests()
    {
        _sut = new CoverService(
            _covers,
            new PremiumCalculator(),
            _audits,
            new FixedClock(new DateTime(2026, 9, 17, 12, 0, 0, DateTimeKind.Utc)));
    }

    [Fact]
    public async Task Create_rejects_start_date_in_the_past()
    {
        await Assert.ThrowsAsync<ValidationException>(() => _sut.CreateAsync(new CreateCoverRequest
        {
            StartDate = new DateTime(2026, 9, 16),
            EndDate = new DateTime(2026, 10, 16),
            Type = CoverType.Yacht
        }, CancellationToken.None));
    }

    [Fact]
    public async Task Create_rejects_period_longer_than_one_year()
    {
        await Assert.ThrowsAsync<ValidationException>(() => _sut.CreateAsync(new CreateCoverRequest
        {
            StartDate = new DateTime(2026, 9, 17),
            EndDate = new DateTime(2027, 9, 18),
            Type = CoverType.Tanker
        }, CancellationToken.None));
    }

    [Fact]
    public async Task Create_allows_start_date_of_today()
    {
        var created = await _sut.CreateAsync(new CreateCoverRequest
        {
            StartDate = new DateTime(2026, 9, 17),
            EndDate = new DateTime(2026, 10, 17),
            Type = CoverType.Yacht
        }, CancellationToken.None);

        Assert.Equal(new DateTime(2026, 9, 17), created.StartDate);
    }

    [Fact]
    public async Task Create_allows_exactly_one_year_and_computes_premium()
    {
        var created = await _sut.CreateAsync(new CreateCoverRequest
        {
            StartDate = new DateTime(2026, 9, 17),
            EndDate = new DateTime(2027, 9, 17),
            Type = CoverType.Yacht
        }, CancellationToken.None);

        Assert.True(created.Premium > 0);
        Assert.Contains(_audits.Items, item => item.Kind == "Cover" && item.HttpRequestType == IAuditPublisher.Post);
    }

    [Fact]
    public async Task Create_does_not_persist_or_audit_when_invalid()
    {
        await Assert.ThrowsAsync<ValidationException>(() => _sut.CreateAsync(new CreateCoverRequest
        {
            StartDate = new DateTime(2026, 9, 16),
            EndDate = new DateTime(2027, 9, 18),
            Type = CoverType.Yacht
        }, CancellationToken.None));

        Assert.Empty(await _covers.GetAllAsync(CancellationToken.None));
        Assert.Empty(_audits.Items);
    }

    [Fact]
    public async Task GetAll_returns_created_covers()
    {
        await _sut.CreateAsync(ValidCover(), CancellationToken.None);

        var covers = await _sut.GetAllAsync(CancellationToken.None);

        Assert.Single(covers);
        Assert.Equal(CoverType.Yacht, covers[0].Type);
    }

    [Fact]
    public async Task GetById_returns_created_cover()
    {
        var created = await _sut.CreateAsync(ValidCover(), CancellationToken.None);

        var fetched = await _sut.GetByIdAsync(created.Id, CancellationToken.None);

        Assert.Equal(created.Id, fetched.Id);
        Assert.Equal(created.Premium, fetched.Premium);
    }

    [Fact]
    public async Task GetById_throws_when_missing()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetByIdAsync("missing", CancellationToken.None));
    }

    [Fact]
    public async Task Delete_removes_cover_and_publishes_audit()
    {
        var created = await _sut.CreateAsync(ValidCover(), CancellationToken.None);

        await _sut.DeleteAsync(created.Id, CancellationToken.None);

        Assert.Empty(await _covers.GetAllAsync(CancellationToken.None));
        Assert.Contains(_audits.Items, item => item.Kind == "Cover" && item.HttpRequestType == IAuditPublisher.Delete && item.Id == created.Id);
    }

    [Fact]
    public async Task Delete_throws_when_missing()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.DeleteAsync("missing", CancellationToken.None));
        Assert.Empty(_audits.Items);
    }

    [Fact]
    public void ComputePremium_matches_calculator()
    {
        var start = new DateTime(2026, 9, 17);
        var end = new DateTime(2026, 10, 17);

        var premium = _sut.ComputePremium(start, end, CoverType.Tanker);

        Assert.Equal(new PremiumCalculator().Compute(start, end, CoverType.Tanker), premium);
    }

    private static CreateCoverRequest ValidCover() => new()
    {
        StartDate = new DateTime(2026, 9, 17),
        EndDate = new DateTime(2026, 10, 17),
        Type = CoverType.Yacht
    };
}
