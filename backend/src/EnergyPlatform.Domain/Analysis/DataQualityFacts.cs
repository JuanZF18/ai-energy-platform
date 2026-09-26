using EnergyPlatform.Domain.Anomalies;
using EnergyPlatform.Domain.Events;
using static EnergyPlatform.Domain.Analysis.SpanishText;

namespace EnergyPlatform.Domain.Analysis;

public static class DataQualityFacts
{
    public static IReadOnlyList<string> Describe(
        DataQualityIssue issue,
        DataQualityEvidence evidence,
        ConsumptionEvidence consumption,
        IReadOnlyList<RelatedEvent> events,
        AnalysisOptions options)
    {
        var facts = new List<string>
        {
            $"{evidence.SuspectReadings} lecturas sospechosas desde el {Moment(issue.FirstSeen)} mientras el consumo se mantiene normal"
        };

        if (issue.Has(SuspicionReason.VoltageOutOfRange))
        {
            facts.Add($"Voltaje entre {Number(evidence.MinimumVoltage, 1)} y {Number(evidence.MaximumVoltage, 1)} V, fuera del rango {Number(options.MinimumVoltage)}–{Number(options.MaximumVoltage)} V");
        }

        if (evidence.RepeatedPowerFactors.Count > 0)
        {
            facts.Add($"El factor de potencia solo toma {evidence.RepeatedPowerFactors.Count} valores repetidos: {string.Join(" · ", evidence.RepeatedPowerFactors.Select(value => Number(value, 2)))}");
        }

        if (evidence.RepeatsEveryHours is { } hours)
        {
            facts.Add($"Las lecturas inconsistentes se repiten cada {hours} horas exactas");
        }

        facts.Add($"El consumo diario se mantiene estable ({SignedPercent(consumption.VariationPercent)} frente al baseline)");
        facts.AddRange(events
            .Where(related => related.Relation == EventRelation.ConfirmsIssue)
            .Select(related => $"El evento {related.Event.Type.ToCode()} lo confirma: \"{related.Event.Description}\""));

        return facts;
    }
}
