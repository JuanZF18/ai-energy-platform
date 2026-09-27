using EnergyPlatform.Domain.Anomalies;

namespace EnergyPlatform.Domain.Analysis;

public static class Scoring
{
    public static ConfidenceBreakdown ConfidenceForShift(ConsumptionShift shift, Classification classification, AnalysisOptions options) => new(
        Limit(Math.Abs(shift.MeanDeviation) / options.HighSeverityDeviation),
        Limit(shift.Hours / 24.0),
        classification.Corroboration);

    public static ConfidenceBreakdown ConfidenceForDataQuality(DataQualityIssue issue, Classification classification, AnalysisOptions options) => new(
        Limit(issue.Readings.Count / (options.MinimumSuspectReadings * 3.0)),
        Limit(issue.SpanHours / 24.0),
        classification.Corroboration);

    public static ConfidenceBreakdown ConfidenceForVoltage(VoltageIssue issue, Classification classification, AnalysisOptions options) => new(
        Limit(issue.LargestExcessPercent(options) / (options.MaximumVoltageRise * 100)),
        Limit(issue.SpanHours / 24.0),
        classification.Corroboration);

    public static double Priority(AnomalyType type, Severity severity, double confidence) =>
        Math.Round(SeverityWeight(severity) * TypeWeight(type) * confidence, 3);

    private static double SeverityWeight(Severity severity) => severity switch
    {
        Severity.High => 1.0,
        Severity.Medium => 0.6,
        _ => 0.3
    };

    private static double TypeWeight(AnomalyType type) => type switch
    {
        AnomalyType.RealAnomaly => 1.0,
        AnomalyType.DataQuality => 0.8,
        AnomalyType.ExplainableAnomaly => 0.5,
        _ => 0.1
    };

    private static double Limit(double value) => Math.Round(Math.Clamp(value, 0, 1), 2);
}
