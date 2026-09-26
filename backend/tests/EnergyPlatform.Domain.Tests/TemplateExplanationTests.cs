using EnergyPlatform.Application.Explanations;
using EnergyPlatform.Domain.Analysis;
using EnergyPlatform.Domain.Anomalies;

namespace EnergyPlatform.Domain.Tests;

public sealed class TemplateExplanationTests
{
    private static readonly Lazy<IReadOnlyList<Finding>> Findings =
        new(() => new AnomalyEngine(AnalysisOptions.Default).Analyze(ChallengeData.MetersToAnalyze()));

    private static readonly string[] EnglishTerms = ["baseline", "REAL_ANOMALY", "DATA_QUALITY", "OPERATIONAL_CHANGE", "SCHEDULED_OUTAGE", "UNKNOWN", "High", "Medium"];

    public static TheoryData<string> Meters => ["M-109", "M-112", "M-104", "M-106"];

    [Theory]
    [MemberData(nameof(Meters))]
    public async Task Writes_a_complete_explanation_for_every_case(string meterId)
    {
        var written = await WriteAsync(meterId);

        Assert.Equal(ExplanationSource.Template, written.Source);
        Assert.False(string.IsNullOrWhiteSpace(written.Explanation.Summary));
        Assert.NotEmpty(written.Explanation.WhatChanged);
        Assert.NotEmpty(written.Explanation.PossibleCauses);
        Assert.NotEmpty(written.Explanation.NextSteps);
    }

    [Theory]
    [MemberData(nameof(Meters))]
    public async Task Uses_plain_spanish_without_codes(string meterId)
    {
        var written = await WriteAsync(meterId);
        var text = string.Join(" ", [written.Explanation.Summary, .. written.Explanation.PossibleCauses, .. written.Explanation.NextSteps]);

        Assert.DoesNotContain(EnglishTerms, term => text.Contains(term, StringComparison.Ordinal));
    }

    [Fact]
    public async Task Names_the_explaining_event_first_for_the_explainable_case()
    {
        var written = await WriteAsync("M-104");

        Assert.Contains("New production line activated", written.Explanation.PossibleCauses[0]);
    }

    [Fact]
    public async Task Recommends_not_escalating_the_false_positive()
    {
        var written = await WriteAsync("M-106");

        Assert.Equal("No escalar", written.Explanation.NextSteps[0]);
    }

    [Fact]
    public async Task Formats_a_zero_change_without_duplicated_digits()
    {
        var written = await WriteAsync("M-104");

        Assert.DoesNotContain(written.Explanation.WhatChanged, line => line.Contains("(00%)"));
    }

    private static Task<WrittenExplanation> WriteAsync(string meterId) =>
        new TemplateExplanationWriter().WriteAsync(Findings.Value.Single(finding => finding.MeterId == meterId), CancellationToken.None);
}
