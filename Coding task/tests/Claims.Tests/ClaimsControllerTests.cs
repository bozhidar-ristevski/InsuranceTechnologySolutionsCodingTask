using System.Net;
using System.Net.Http.Json;
using Claims.Application.Claims.Models;
using Claims.Application.Covers.Models;
using Claims.Domain.Enums;
using Xunit;

namespace Claims.Tests;

[Collection(nameof(ApiCollection))]
public sealed class ClaimsControllerTests
{
    private readonly HttpClient _client;

    public ClaimsControllerTests(ApiFixture fixture)
    {
        _client = fixture.Client;
    }

    [Fact]
    public async Task Get_returns_claims_list()
    {
        var response = await _client.GetAsync("/Claims", TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var claims = await response.Content.ReadFromJsonAsync<List<ClaimDto>>(ApiFixture.JsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(claims);
    }

    [Fact]
    public async Task Get_by_id_returns_not_found_when_missing()
    {
        var response = await _client.GetAsync("/Claims/missing", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_get_and_delete_claim()
    {
        var cover = await CreateCoverAsync();
        var payload = new CreateClaimRequest
        {
            CoverId = cover.Id,
            Created = cover.StartDate,
            Name = "Collision",
            Type = ClaimType.Collision,
            DamageCost = 2500
        };

        var createResponse = await _client.PostAsJsonAsync("/Claims", payload, ApiFixture.JsonOptions, TestContext.Current.CancellationToken);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<ClaimDto>(ApiFixture.JsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(created);
        Assert.Equal(payload.DamageCost, created.DamageCost);

        var getResponse = await _client.GetAsync($"/Claims/{created.Id}", TestContext.Current.CancellationToken);
        getResponse.EnsureSuccessStatusCode();
        var fetched = await getResponse.Content.ReadFromJsonAsync<ClaimDto>(ApiFixture.JsonOptions, TestContext.Current.CancellationToken);
        Assert.Equal(created.Id, fetched!.Id);

        var deleteResponse = await _client.DeleteAsync($"/Claims/{created.Id}", TestContext.Current.CancellationToken);
        deleteResponse.EnsureSuccessStatusCode();

        var missing = await _client.GetAsync($"/Claims/{created.Id}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
    }

    [Fact]
    public async Task Create_returns_not_found_when_cover_is_missing()
    {
        var response = await _client.PostAsJsonAsync("/Claims", new CreateClaimRequest
        {
            CoverId = "missing-cover",
            Created = DateTime.UtcNow.Date,
            Name = "Fire",
            Type = ClaimType.Fire,
            DamageCost = 10
        }, ApiFixture.JsonOptions, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_returns_bad_request_when_damage_cost_exceeds_limit()
    {
        var cover = await CreateCoverAsync();

        var response = await _client.PostAsJsonAsync("/Claims", new CreateClaimRequest
        {
            CoverId = cover.Id,
            Created = cover.StartDate,
            Name = "Collision",
            Type = ClaimType.Collision,
            DamageCost = 100_000.01m
        }, ApiFixture.JsonOptions, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<CoverDto> CreateCoverAsync()
    {
        var today = DateTime.UtcNow.Date;
        var response = await _client.PostAsJsonAsync("/Covers", new CreateCoverRequest
        {
            StartDate = today,
            EndDate = today.AddDays(30),
            Type = CoverType.Yacht
        }, ApiFixture.JsonOptions, TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var cover = await response.Content.ReadFromJsonAsync<CoverDto>(ApiFixture.JsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(cover);
        return cover;
    }
}
