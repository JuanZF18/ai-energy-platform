using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace EnergyPlatform.Api.Tests;

[Collection(PlatformCollection.Name)]
public sealed class AnomalyStatusTests(PlatformFixture platform)
{
    [Fact]
    public async Task Records_who_changed_the_status_and_the_note()
    {
        await platform.CompletedAnalysisAsync();
        using var client = await platform.SignedInClientAsync();
        var id = await AnomalyIdAsync(client, "M-104");

        var response = await client.PatchAsJsonAsync($"/api/anomalies/{id}", new { status = "INVESTIGATING", note = "Operación confirma la nueva línea" });

        response.EnsureSuccessStatusCode();
        var detail = (await response.Content.ReadFromJsonAsync<JsonNode>())!;
        Assert.Equal("INVESTIGATING", detail["summary"]!["status"]!.GetValue<string>());
        var change = detail["history"]!.AsArray().Last()!;
        Assert.Equal("Operación confirma la nueva línea", change["note"]!.GetValue<string>());
        Assert.Equal("demo@vatio.app", change["changedBy"]!.GetValue<string>());
    }

    [Fact]
    public async Task Rejects_a_transition_the_workflow_does_not_allow()
    {
        await platform.CompletedAnalysisAsync();
        using var client = await platform.SignedInClientAsync();
        var id = await AnomalyIdAsync(client, "M-112");
        (await client.PatchAsJsonAsync($"/api/anomalies/{id}", new { status = "RESOLVED" })).EnsureSuccessStatusCode();

        var response = await client.PatchAsJsonAsync($"/api/anomalies/{id}", new { status = "INVESTIGATING" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Answers_not_found_for_an_unknown_anomaly()
    {
        using var client = await platform.SignedInClientAsync();

        var response = await client.GetAsync($"/api/anomalies/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static async Task<Guid> AnomalyIdAsync(HttpClient client, string meterId)
    {
        var anomalies = (await client.GetFromJsonAsync<JsonArray>("/api/anomalies"))!;
        return anomalies.Single(anomaly => anomaly!["meterId"]!.GetValue<string>() == meterId)!["id"]!.GetValue<Guid>();
    }
}
