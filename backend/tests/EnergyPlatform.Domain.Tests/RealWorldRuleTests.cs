using EnergyPlatform.Domain.Analysis;
using EnergyPlatform.Domain.Anomalies;
using EnergyPlatform.Domain.Readings;

namespace EnergyPlatform.Domain.Tests;

public sealed class RealWorldRuleTests
{
    private static readonly DateTime QuietAfternoon = new(2026, 9, 5, 15, 0, 0);

    [Fact]
    public void Reports_a_severe_consumption_spike_from_its_first_hour()
    {
        var findings = ChallengeData.AnalyzeWith(readings => readings.Select(reading =>
            IsHealthyMeterAt(reading, QuietAfternoon) ? WithLoad(reading, 1.8) : reading));

        var spike = Assert.Single(findings, finding => finding.MeterId == "M-101");
        Assert.Equal(AnomalyType.RealAnomaly, spike.Type);
        Assert.Equal(Severity.High, spike.Severity);
        Assert.Equal(1, spike.Evidence.Window.Hours);
        AssertChallengeCasesRemain(findings);
    }

    [Fact]
    public void Waits_for_three_hours_before_reporting_a_moderate_rise()
    {
        var findings = ChallengeData.AnalyzeWith(readings => readings.Select(reading =>
            IsHealthyMeterAt(reading, QuietAfternoon) || IsHealthyMeterAt(reading, QuietAfternoon.AddHours(1)) ? WithLoad(reading, 1.35) : reading));

        Assert.DoesNotContain(findings, finding => finding.MeterId == "M-101");
    }

    [Fact]
    public void Reports_a_real_overvoltage_from_its_first_readings()
    {
        var findings = ChallengeData.AnalyzeWith(readings => readings.Select(reading =>
            IsHealthyMeterAt(reading, QuietAfternoon) || IsHealthyMeterAt(reading, QuietAfternoon.AddHours(1)) ? WithVoltage(reading, 248) : reading));

        var overvoltage = Assert.Single(findings, finding => finding.MeterId == "M-101");
        Assert.Equal(AnomalyType.RealAnomaly, overvoltage.Type);
        Assert.Equal(Severity.High, overvoltage.Severity);
        Assert.Equal(2, overvoltage.Evidence.Voltage!.ReadingsOutsideStandard);
        Assert.Equal(VoltageFindingComposer.RecommendedAction, overvoltage.RecommendedAction);
        Assert.Contains("por encima del máximo de 231 V", overvoltage.Reason);
        AssertChallengeCasesRemain(findings);
    }

    [Fact]
    public void Reports_a_single_hour_of_real_undervoltage()
    {
        var findings = ChallengeData.AnalyzeWith(readings => readings.Select(reading =>
            IsHealthyMeterAt(reading, QuietAfternoon) ? WithVoltage(reading, 190) : reading));

        var undervoltage = Assert.Single(findings, finding => finding.MeterId == "M-101");
        Assert.Equal(1, undervoltage.Evidence.Voltage!.ReadingsOutsideStandard);
        Assert.Contains("por debajo del mínimo de 198 V", undervoltage.Reason);
    }

    [Fact]
    public void Accepts_a_low_voltage_that_the_colombian_standard_allows()
    {
        var findings = ChallengeData.AnalyzeWith(readings => readings.Select(reading =>
            IsHealthyMeterAt(reading, QuietAfternoon) || IsHealthyMeterAt(reading, QuietAfternoon.AddHours(1)) ? WithVoltage(reading, 204) : reading));

        Assert.DoesNotContain(findings, finding => finding.MeterId == "M-101");
    }

    [Fact]
    public void Keeps_impossible_readings_as_a_meter_problem_instead_of_a_voltage_problem()
    {
        var findings = ChallengeData.AnalyzeWith();

        var meterProblem = Assert.Single(findings, finding => finding.MeterId == "M-112");
        Assert.Equal(AnomalyType.DataQuality, meterProblem.Type);
        Assert.Null(meterProblem.Evidence.Voltage);
    }

    private static bool IsHealthyMeterAt(Reading reading, DateTime timestamp) => reading.MeterId == "M-101" && reading.Timestamp == timestamp;

    private static Reading WithLoad(Reading reading, double factor) =>
        new(reading.MeterId, reading.Timestamp, reading.ConsumptionKwh * factor, reading.VoltageV, reading.CurrentA * factor, reading.PowerFactor);

    private static Reading WithVoltage(Reading reading, double voltage) =>
        new(reading.MeterId, reading.Timestamp, reading.ConsumptionKwh * voltage / reading.VoltageV, voltage, reading.CurrentA, reading.PowerFactor);

    private static void AssertChallengeCasesRemain(IReadOnlyList<Finding> findings)
    {
        Assert.Contains(findings, finding => finding.MeterId == "M-109" && finding.Type == AnomalyType.RealAnomaly);
        Assert.Contains(findings, finding => finding.MeterId == "M-112" && finding.Type == AnomalyType.DataQuality);
        Assert.Contains(findings, finding => finding.MeterId == "M-104" && finding.Type == AnomalyType.ExplainableAnomaly);
        Assert.Contains(findings, finding => finding.MeterId == "M-106" && finding.Type == AnomalyType.FalsePositive);
    }
}
