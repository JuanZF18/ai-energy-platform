using EnergyPlatform.Domain.Analysis;
using EnergyPlatform.Domain.Anomalies;
using EnergyPlatform.Domain.Events;
using EnergyPlatform.Domain.Readings;

namespace EnergyPlatform.Domain.Tests;

public sealed class DataVariationTests
{
    private static readonly string[] HealthyMeters = ["M-101", "M-102", "M-103", "M-105", "M-107", "M-108", "M-110", "M-111"];

    [Theory]
    [InlineData(1)]
    [InlineData(7)]
    [InlineData(42)]
    public void Keeps_the_same_classification_when_readings_have_small_noise(int seed)
    {
        var random = new Random(seed);
        var findings = Analyze(readings => readings.Select(reading => Scaled(reading, 1 + (random.NextDouble() - 0.5) * 0.06)));

        AssertExpectedCases(findings);
    }

    [Fact]
    public void Keeps_one_case_per_change_when_noise_breaks_the_change_for_a_few_hours()
    {
        var random = new Random(11);
        var findings = Analyze(readings => readings.Select(reading => Scaled(reading, 1 + (random.NextDouble() - 0.5) * 0.16)));

        AssertExpectedCases(findings);
    }

    [Fact]
    public void Keeps_the_same_classification_when_some_hours_are_missing()
    {
        var random = new Random(3);
        var findings = Analyze(readings => readings.Where(_ => random.NextDouble() > 0.03));

        AssertExpectedCases(findings);
    }

    [Fact]
    public void Ignores_the_order_and_duplicates_of_the_rows()
    {
        var random = new Random(5);
        var findings = Analyze(readings => readings.Concat(readings.Take(200)).OrderBy(_ => random.Next()));

        AssertExpectedCases(findings);
    }

    [Fact]
    public void Still_detects_a_smaller_real_increase()
    {
        var changeStart = new DateTime(2026, 9, 12, 14, 0, 0);
        var findings = Analyze(readings => readings.Select(reading =>
            reading.MeterId == "M-109" && reading.Timestamp >= changeStart ? Scaled(reading, 0.7) : reading));

        var finding = Assert.Single(findings, finding => finding.MeterId == "M-109" && finding.Type != AnomalyType.DataQuality);
        Assert.Equal(AnomalyType.RealAnomaly, finding.Type);
        Assert.Equal(changeStart, finding.Evidence.Window.Start);
    }

    [Fact]
    public void Treats_an_unexplained_outage_as_an_anomaly_when_its_event_is_missing()
    {
        var findings = Analyze(events: events => events.Where(meterEvent => meterEvent.MeterId != "M-106"));

        var finding = Assert.Single(findings, finding => finding.MeterId == "M-106");
        Assert.NotEqual(AnomalyType.FalsePositive, finding.Type);
    }

    [Fact]
    public void Treats_the_production_increase_as_real_when_its_event_is_missing()
    {
        var findings = Analyze(events: events => events.Where(meterEvent => meterEvent.MeterId != "M-104"));

        var finding = Assert.Single(findings, finding => finding.MeterId == "M-104");
        Assert.Equal(AnomalyType.RealAnomaly, finding.Type);
    }

    [Fact]
    public void Does_not_explain_a_change_with_an_event_of_another_meter()
    {
        var findings = Analyze(events: events => events.Select(meterEvent => meterEvent.MeterId == "M-104"
            ? new MeterEvent("M-105", meterEvent.Timestamp, meterEvent.Type, meterEvent.Description)
            : meterEvent));

        var finding = Assert.Single(findings, finding => finding.MeterId == "M-104");
        Assert.Equal(AnomalyType.RealAnomaly, finding.Type);
    }

    [Fact]
    public void Analyzes_a_meter_with_very_few_readings_without_failing()
    {
        var findings = Analyze(readings => readings.Where(reading => reading.MeterId != "M-101" || reading.Timestamp.Day == 1 && reading.Timestamp.Hour < 5));

        Assert.DoesNotContain(findings, finding => finding.MeterId == "M-101");
        Assert.Contains(findings, finding => finding.MeterId == "M-109");
    }

    [Fact]
    public void Analyzes_an_empty_fleet_without_failing()
    {
        var findings = new AnomalyEngine(AnalysisOptions.Default).Analyze([]);

        Assert.Empty(findings);
    }

    private static void AssertExpectedCases(IReadOnlyList<Finding> findings)
    {
        Assert.Equal(["M-109", "M-112", "M-104", "M-106"], findings.Select(finding => finding.MeterId));
        Assert.Equal(
            [AnomalyType.RealAnomaly, AnomalyType.DataQuality, AnomalyType.ExplainableAnomaly, AnomalyType.FalsePositive],
            findings.Select(finding => finding.Type));
        Assert.DoesNotContain(findings, finding => HealthyMeters.Contains(finding.MeterId));
    }

    private static IReadOnlyList<Finding> Analyze(
        Func<IReadOnlyList<Reading>, IEnumerable<Reading>>? readings = null,
        Func<IReadOnlyList<MeterEvent>, IEnumerable<MeterEvent>>? events = null) => ChallengeData.AnalyzeWith(readings, events);

    private static Reading Scaled(Reading reading, double factor) =>
        new(reading.MeterId, reading.Timestamp, reading.ConsumptionKwh * factor, reading.VoltageV, reading.CurrentA * factor, reading.PowerFactor);
}
