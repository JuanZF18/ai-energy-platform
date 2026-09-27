using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace EnergyPlatform.Api.Tests;

[Collection(PlatformCollection.Name)]
public sealed class AnalysisFlowTests(PlatformFixture platform)
{
    [Fact]
    public async Task Completes_the_seven_stages_with_the_expected_headline()
    {
        var run = await platform.CompletedAnalysisAsync();

        Assert.Equal("COMPLETED", run["status"]!.GetValue<string>());
        Assert.Equal(7, run["stages"]!.AsArray().Count);
        Assert.All(run["stages"]!.AsArray(), stage => Assert.Equal("DONE", stage!["state"]!.GetValue<string>()));
        Assert.Equal("4 anomalías detectadas · 2 requieren atención prioritaria", run["headline"]!.GetValue<string>());
    }

    [Fact]
    public async Task Ranks_the_four_cases_of_the_dataset_as_the_challenge_expects()
    {
        await platform.CompletedAnalysisAsync();
        using var client = await platform.SignedInClientAsync();

        var anomalies = (await client.GetFromJsonAsync<JsonArray>("/api/anomalies"))!
            .Select(anomaly => (
                Meter: anomaly!["meterId"]!.GetValue<string>(),
                Type: anomaly["type"]!.GetValue<string>(),
                Severity: anomaly["severity"]!.GetValue<string>()))
            .ToList();

        Assert.Equal(
            [
                ("M-109", "REAL_ANOMALY", "HIGH"),
                ("M-112", "DATA_QUALITY", "HIGH"),
                ("M-104", "EXPLAINABLE_ANOMALY", "MEDIUM"),
                ("M-106", "FALSE_POSITIVE", "LOW")
            ],
            anomalies);
    }

    [Fact]
    public async Task Dashboard_summary_reflects_the_analysis()
    {
        await platform.CompletedAnalysisAsync();
        using var client = await platform.SignedInClientAsync();

        var summary = (await client.GetFromJsonAsync<JsonNode>("/api/dashboard/summary"))!;

        Assert.Equal(12, summary["meters"]!["total"]!.GetValue<int>());
        Assert.Equal(4, summary["anomalies"]!["cases"]!.GetValue<int>());
        Assert.Equal(2, summary["anomalies"]!["highPriority"]!.GetValue<int>());
        Assert.Equal("COMPLETED", summary["lastAnalysis"]!["status"]!.GetValue<string>());
    }

    [Fact]
    public async Task Investigation_of_the_top_case_includes_evidence_explanation_and_action()
    {
        await platform.CompletedAnalysisAsync();
        using var client = await platform.SignedInClientAsync();
        var top = (await client.GetFromJsonAsync<JsonArray>("/api/anomalies?limit=1"))![0]!;

        var detail = (await client.GetFromJsonAsync<JsonNode>($"/api/anomalies/{top["id"]!.GetValue<Guid>()}"))!;

        Assert.Equal("M-109", detail["summary"]!["meterId"]!.GetValue<string>());
        Assert.NotEmpty(detail["evidence"]!["facts"]!.AsArray());
        Assert.NotEmpty(detail["evidence"]!["changedVariables"]!.AsArray());
        Assert.Equal("TEMPLATE", detail["explanation"]!["source"]!.GetValue<string>());
        Assert.Equal("Investigar medidor e instalación.", detail["summary"]!["recommendedAction"]!.GetValue<string>());
    }
}
