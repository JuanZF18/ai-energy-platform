using System.Globalization;
using EnergyPlatform.Domain.Anomalies;
using EnergyPlatform.Domain.Events;
using EnergyPlatform.Domain.Readings;

namespace EnergyPlatform.Domain.Analysis;

public static class EvidenceParts
{
    public static string Fingerprint(string meterId, AnomalyType type, DateTime start) =>
        string.Create(CultureInfo.InvariantCulture, $"{meterId}:{type}:{start:yyyyMMddHH}");

    public static double Variation(double observed, double baseline) =>
        baseline > 0 ? Math.Round((observed - baseline) / baseline * 100, 1) : 0;

    public static EventEvidence ToEvidence(RelatedEvent related) => new(
        related.Event.Type.ToCode(),
        related.Event.Timestamp,
        related.Event.Description,
        related.Relation == EventRelation.ExplainsChange);

    public static IReadOnlyList<VariableChange> ChangedVariables(ElectricalChange change) =>
    [
        Compare("Consumo", "kWh/h", change.Before.ConsumptionKwh, change.During.ConsumptionKwh),
        Compare("Corriente", "A", change.Before.CurrentA, change.During.CurrentA),
        Compare("Factor de potencia", "", change.Before.PowerFactor, change.During.PowerFactor),
        Compare("Voltaje", "V", change.Before.VoltageV, change.During.VoltageV)
    ];

    public static IReadOnlyList<VariableChange> ChangedVariables(IReadOnlyCollection<Reading> normal, IReadOnlyCollection<Reading> suspect) =>
    [
        Compare("Voltaje", "V", normal.Average(r => r.VoltageV), suspect.Average(r => r.VoltageV)),
        Compare("Factor de potencia", "", normal.Average(r => r.PowerFactor), suspect.Average(r => r.PowerFactor)),
        Compare("Relación kWh / (V·I·FP)", "", normal.Average(r => r.ElectricalRatio), suspect.Average(r => r.ElectricalRatio))
    ];

    private static VariableChange Compare(string variable, string unit, double before, double after) => new(
        variable,
        unit,
        Math.Round(before, 3),
        Math.Round(after, 3),
        Variation(after, before));
}
