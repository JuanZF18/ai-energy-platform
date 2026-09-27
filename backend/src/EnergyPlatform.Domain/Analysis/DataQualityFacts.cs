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

        var outsideStandard = issue.Readings.Where(suspect => suspect.Reasons.HasFlag(SuspicionReason.VoltageOutOfRange)).ToList();
        if (outsideStandard.Count > 0)
        {
            facts.Add($"{outsideStandard.Count} de esas lecturas tienen el voltaje fuera del rango {Number(options.MinimumVoltage)}–{Number(options.MaximumVoltage)} V de la norma NTC 1340 (entre {Number(outsideStandard.Min(suspect => suspect.Reading.VoltageV), 1)} y {Number(outsideStandard.Max(suspect => suspect.Reading.VoltageV), 1)} V)");
        }

        if (issue.Has(SuspicionReason.InconsistentElectricalRatio))
        {
            facts.Add("En varias lecturas el consumo no cuadra con voltaje × corriente × factor de potencia: los valores no pueden ser reales al mismo tiempo");
        }

        if (evidence.RepeatedPowerFactors.Count > 0)
        {
            facts.Add($"El factor de potencia solo toma {evidence.RepeatedPowerFactors.Count} valores repetidos: {string.Join(" · ", evidence.RepeatedPowerFactors.Select(value => Number(value, 2)))}");
        }

        if (evidence.RepeatsEveryHours is { } hours)
        {
            facts.Add($"Las lecturas inconsistentes se repiten cada {hours} horas exactas");
        }

        facts.Add($"El consumo diario se mantiene estable ({SignedPercent(consumption.VariationPercent)} frente al consumo esperado)");
        facts.AddRange(events
            .Where(related => related.Relation == EventRelation.ConfirmsIssue)
            .Select(related => $"{Capitalized(related.Event.Type.WithIndefiniteArticle())} del {Moment(related.Event.Timestamp)} lo confirma (registro original: \"{related.Event.Description}\")"));

        return facts;
    }
}
