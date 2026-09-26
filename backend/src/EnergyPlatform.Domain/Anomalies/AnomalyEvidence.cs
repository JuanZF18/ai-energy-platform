namespace EnergyPlatform.Domain.Anomalies;

public sealed record AnomalyEvidence(
    EvidenceWindow Window,
    ConsumptionEvidence Consumption,
    IReadOnlyList<VariableChange> ChangedVariables,
    IReadOnlyList<EventEvidence> Events,
    DataQualityEvidence? DataQuality,
    ConfidenceBreakdown Confidence,
    IReadOnlyList<string> Facts);

public sealed record EvidenceWindow(DateTime Start, DateTime End, int Hours, bool IsOngoing);

public sealed record ConsumptionEvidence(
    double BaselineDailyKwh,
    double ObservedDailyKwh,
    double VariationPercent,
    double MeanHourlyDeviationPercent);

public sealed record VariableChange(string Variable, string Unit, double Before, double After, double ChangePercent);

public sealed record EventEvidence(string Type, DateTime Timestamp, string Description, bool ExplainsChange);

public sealed record DataQualityEvidence(
    int SuspectReadings,
    double MinimumVoltage,
    double MaximumVoltage,
    IReadOnlyList<double> RepeatedPowerFactors,
    int? RepeatsEveryHours);

public sealed record ConfidenceBreakdown(double Signal, double Persistence, double Corroboration)
{
    public double Score => Math.Round(0.5 + 0.45 * (0.4 * Signal + 0.3 * Persistence + 0.3 * Corroboration), 2);
}
