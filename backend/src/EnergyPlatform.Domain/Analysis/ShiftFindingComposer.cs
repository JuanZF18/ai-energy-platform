using EnergyPlatform.Domain.Anomalies;
using EnergyPlatform.Domain.Events;
using static EnergyPlatform.Domain.Analysis.SpanishText;

namespace EnergyPlatform.Domain.Analysis;

public static class ShiftFindingComposer
{
    public static Finding Compose(MeterAnalysis meter, ConsumptionShift shift, AnalysisOptions options)
    {
        var change = meter.ElectricalChanges[shift];
        var events = meter.ShiftEvents[shift];
        var classification = AnomalyClassifier.ClassifyShift(shift, change, events, options);
        var confidence = Scoring.ConfidenceForShift(shift, classification, options);
        var consumption = ConsumptionDuring(meter, shift);
        var explainingEvent = events.FirstOrDefault(related => related.Relation == EventRelation.ExplainsChange)?.Event;

        var evidence = new AnomalyEvidence(
            new EvidenceWindow(shift.Start, shift.End, shift.Hours, shift.IsOngoing),
            consumption,
            EvidenceParts.ChangedVariables(change),
            [.. events.Select(EvidenceParts.ToEvidence)],
            null,
            confidence,
            ShiftFacts.Describe(shift, change, events, options));

        return new Finding(
            meter.MeterId,
            EvidenceParts.Fingerprint(meter.MeterId, classification.Type, shift.Start),
            classification.Type,
            classification.Severity,
            confidence.Score,
            Scoring.Priority(classification.Type, classification.Severity, confidence.Score),
            Reason(classification.Type, shift, consumption, explainingEvent),
            RecommendedActions.For(classification.Type),
            evidence);
    }

    private static ConsumptionEvidence ConsumptionDuring(MeterAnalysis meter, ConsumptionShift shift)
    {
        var snapshot = meter.RequiredSnapshot;
        var observed = shift.IsOngoing
            ? snapshot.CurrentDailyKwh
            : meter.Readings.Where(reading => reading.Timestamp.Date == shift.Start.Date).Sum(reading => reading.ConsumptionKwh);

        return new ConsumptionEvidence(
            Math.Round(snapshot.BaselineDailyKwh, 1),
            Math.Round(observed, 1),
            EvidenceParts.Variation(observed, snapshot.BaselineDailyKwh),
            Math.Round(shift.MeanDeviation * 100, 1));
    }

    private static string Reason(AnomalyType type, ConsumptionShift shift, ConsumptionEvidence consumption, MeterEvent? explainingEvent)
    {
        var variation = shift.IsOngoing ? consumption.VariationPercent : consumption.MeanHourlyDeviationPercent;
        var side = variation >= 0 ? "por encima" : "por debajo";

        return type switch
        {
            AnomalyType.FalsePositive =>
                $"Caída de {Percent(consumption.MeanHourlyDeviationPercent)} durante {shift.Hours} h el {Day(shift.Start)}, explicada por {EventName(explainingEvent)} (\"{explainingEvent?.Description}\").",
            AnomalyType.ExplainableAnomaly =>
                $"Consumo {Percent(variation)} {side} de lo esperado desde el {Moment(shift.Start)}, coincide con {EventName(explainingEvent)} (\"{explainingEvent?.Description}\").",
            _ =>
                $"Consumo {Percent(variation)} {side} de lo esperado desde el {Moment(shift.Start)}, sin un evento operativo que lo explique."
        };
    }

    private static string EventName(MeterEvent? meterEvent) => meterEvent?.Type.WithIndefiniteArticle() ?? "un evento registrado";
}
