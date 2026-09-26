using EnergyPlatform.Domain.Analysis;
using EnergyPlatform.Domain.Anomalies;

namespace EnergyPlatform.Domain.Tests;

public sealed class ChallengeAcceptanceTests
{
    private static readonly Lazy<IReadOnlyList<Finding>> Findings =
        new(() => new AnomalyEngine(AnalysisOptions.Default).Analyze(ChallengeData.MetersToAnalyze()));

    [Fact]
    public void Prioritizes_the_four_cases_in_the_expected_order()
    {
        Assert.Equal(["M-109", "M-112", "M-104", "M-106"], Findings.Value.Select(finding => finding.MeterId));
    }

    [Theory]
    [InlineData("M-109", AnomalyType.RealAnomaly, Severity.High)]
    [InlineData("M-112", AnomalyType.DataQuality, Severity.High)]
    [InlineData("M-104", AnomalyType.ExplainableAnomaly, Severity.Medium)]
    [InlineData("M-106", AnomalyType.FalsePositive, Severity.Low)]
    public void Classifies_each_case_as_expected(string meterId, AnomalyType type, Severity severity)
    {
        var finding = Assert.Single(Findings.Value, finding => finding.MeterId == meterId);

        Assert.Equal(type, finding.Type);
        Assert.Equal(severity, finding.Severity);
    }

    [Fact]
    public void Reports_the_real_anomaly_with_high_confidence_and_evidence()
    {
        var finding = Findings.Value.Single(finding => finding.MeterId == "M-109");

        Assert.InRange(finding.Confidence, 0.85, 1);
        Assert.InRange(finding.Evidence.Consumption.VariationPercent, 100, 120);
        Assert.Equal(new DateTime(2026, 9, 12, 14, 0, 0), finding.Evidence.Window.Start);
        Assert.Contains(finding.Evidence.Events, evidence => evidence.Type == "UNKNOWN" && !evidence.ExplainsChange);
        Assert.Equal("Investigar medidor e instalación.", finding.RecommendedAction);
    }

    [Fact]
    public void Measures_the_explainable_change_against_the_baseline_before_it()
    {
        var finding = Findings.Value.Single(finding => finding.MeterId == "M-104");

        Assert.Equal(47.6, finding.Evidence.Consumption.VariationPercent, 0.5);
    }

    [Fact]
    public void Leaves_the_healthy_meters_without_findings()
    {
        var healthyMeters = new[] { "M-101", "M-102", "M-103", "M-105", "M-107", "M-108", "M-110", "M-111" };

        Assert.DoesNotContain(Findings.Value, finding => healthyMeters.Contains(finding.MeterId));
    }

    [Fact]
    public void Counts_four_cases_and_two_priority_ones()
    {
        Assert.Equal(4, Findings.Value.Count);
        Assert.Equal(2, Findings.Value.Count(finding => finding.IsPriority));
    }
}
