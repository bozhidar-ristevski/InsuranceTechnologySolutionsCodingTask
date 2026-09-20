using System.Net;
using System.Net.Http.Json;
using Claims.Application.Covers.Models;
using Claims.Domain.Enums;
using Claims.Domain.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Claims.Tests;

[Collection(nameof(ApiCollection))]
public sealed class CoversControllerTests
{
    private readonly HttpClient _client;

    public CoversControllerTests(ApiFixture fixture)
    {
        _client = fixture.Client;
    }

    [Fact]
    public async Task Get_returns_covers_list()
    {
        var response = await _client.GetAsync("/Covers", TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var covers = await response.Content.ReadFromJsonAsync<List<CoverDto>>(ApiFixture.JsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(covers);
    }

    [Fact]
    public async Task Get_by_id_returns_not_found_when_missing()
    {
        var response = await _client.GetAsync("/Covers/missing", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await AssertProblemDetailsAsync(response, StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task Create_get_and_delete_cover()
    {
        var today = DateTime.UtcNow.Date;
        var payload = new CreateCoverRequest
        {
            StartDate = today,
            EndDate = today.AddDays(20),
            Type = CoverType.Tanker
        };

        var createResponse = await _client.PostAsJsonAsync("/Covers", payload, ApiFixture.JsonOptions, TestContext.Current.CancellationToken);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<CoverDto>(ApiFixture.JsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(created);
        Assert.Equal(CoverType.Tanker, created.Type);
        Assert.True(created.Premium > 0);

        var getResponse = await _client.GetAsync($"/Covers/{created.Id}", TestContext.Current.CancellationToken);
        getResponse.EnsureSuccessStatusCode();
        var fetched = await getResponse.Content.ReadFromJsonAsync<CoverDto>(ApiFixture.JsonOptions, TestContext.Current.CancellationToken);
        Assert.Equal(created.Id, fetched!.Id);

        var deleteResponse = await _client.DeleteAsync($"/Covers/{created.Id}", TestContext.Current.CancellationToken);
        deleteResponse.EnsureSuccessStatusCode();

        var missing = await _client.GetAsync($"/Covers/{created.Id}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
    }

    [Fact]
    public async Task Create_returns_bad_request_when_start_date_is_in_the_past()
    {
        var response = await _client.PostAsJsonAsync("/Covers", new CreateCoverRequest
        {
            StartDate = DateTime.UtcNow.Date.AddDays(-1),
            EndDate = DateTime.UtcNow.Date.AddDays(10),
            Type = CoverType.Yacht
        }, ApiFixture.JsonOptions, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await AssertProblemDetailsAsync(response, StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Compute_returns_premium()
    {
        var start = DateTime.UtcNow.Date;
        var end = start.AddDays(10);
        var expected = new PremiumCalculator().Compute(start, end, CoverType.Yacht);

        var response = await _client.PostAsync(
            $"/Covers/compute?startDate={start:yyyy-MM-dd}&endDate={end:yyyy-MM-dd}&coverType=Yacht",
            null,
            TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var premium = await response.Content.ReadFromJsonAsync<decimal>(TestContext.Current.CancellationToken);
        Assert.Equal(expected, premium);
    }

    private static async Task AssertProblemDetailsAsync(HttpResponseMessage response, int statusCode)
    {
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(ApiFixture.JsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(problem);
        Assert.Equal(statusCode, problem.Status);
        Assert.False(string.IsNullOrWhiteSpace(problem.Title));
        Assert.False(string.IsNullOrWhiteSpace(problem.Detail));
    }
}
