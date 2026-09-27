using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace EnergyPlatform.Api.Tests;

[Collection(PlatformCollection.Name)]
public sealed class MeterEndpointTests(PlatformFixture platform)
{
    [Fact]
    public async Task Lists_the_twelve_meters_loaded_from_the_challenge_data()
    {
        using var client = await platform.SignedInClientAsync();

        var meters = await client.GetFromJsonAsync<JsonArray>("/api/meters");

        Assert.Equal(12, meters!.Count);
    }

    [Fact]
    public async Task Searches_by_meter_id()
    {
        using var client = await platform.SignedInClientAsync();

        var meters = await client.GetFromJsonAsync<JsonArray>("/api/meters?search=M-109");

        var meter = Assert.Single(meters!);
        Assert.Equal("M-109", meter!["meterId"]!.GetValue<string>());
    }

    [Fact]
    public async Task Sorts_by_consumption_from_highest_to_lowest()
    {
        using var client = await platform.SignedInClientAsync();

        var meters = await client.GetFromJsonAsync<JsonArray>("/api/meters?sortBy=consumption&direction=descending");

        var consumption = meters!.Select(meter => meter!["currentDailyKwh"]!.GetValue<double>()).ToList();
        Assert.Equal(consumption.OrderByDescending(value => value), consumption);
    }

    [Fact]
    public async Task Rejects_an_unknown_filter_value()
    {
        using var client = await platform.SignedInClientAsync();

        var response = await client.GetAsync("/api/meters?status=roto");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Returns_fourteen_days_of_hourly_readings_for_a_meter()
    {
        using var client = await platform.SignedInClientAsync();

        var readings = await client.GetFromJsonAsync<JsonArray>("/api/meters/M-109/readings");

        Assert.Equal(14 * 24, readings!.Count);
    }

    [Fact]
    public async Task Answers_not_found_for_an_unknown_meter()
    {
        using var client = await platform.SignedInClientAsync();

        var response = await client.GetAsync("/api/meters/M-999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
