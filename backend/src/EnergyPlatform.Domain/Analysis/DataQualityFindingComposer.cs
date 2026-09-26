using EnergyPlatform.Domain.Anomalies;
using EnergyPlatform.Domain.Readings;
using static EnergyPlatform.Domain.Analysis.SpanishText;

namespace EnergyPlatform.Domain.Analysis;

public static class DataQualityFindingComposer
{
    private const int MostDistinctValuesThatLookRepeated = 5;

    public static Finding Compose(MeterAnalysis meter, DataQualityIssue issue, AnalysisOptions options)
    {
        var events = meter.DataQualityEvents;
        var classification = AnomalyClassifier.ClassifyDataQuality(issue, events);
        var confidence = Scoring.ConfidenceForDataQuality(issue, classification, options);
        var suspects = issue.Readings.Select(suspect => suspect.Reading).ToList();
        var normal = meter.Readings.Except(suspects).ToList();
        var snapshot = meter.RequiredSnapshot;

        var consumption = new ConsumptionEvidence(
            Math.Round(snapshot.BaselineDailyKwh, 1),
            Math.Round(snapshot.CurrentDailyKwh, 1),
            EvidenceParts.Variation(snapshot.CurrentDailyKwh, snapshot.BaselineDailyKwh),
            0);

        var quality = new DataQualityEvidence(
            suspects.Count,
            suspects.Min(reading => reading.VoltageV),
            suspects.Max(reading => reading.VoltageV),
            RepeatedPowerFactors(suspects),
            issue.RepeatsEveryHours);

        var evidence = new AnomalyEvidence(
            new EvidenceWindow(issue.FirstSeen, issue.LastSeen, issue.SpanHours, issue.IsOngoing),
            consumption,
            EvidenceParts.ChangedVariables(normal, suspects),
            [.. events.Select(EvidenceParts.ToEvidence)],
            quality,
            confidence,
            DataQualityFacts.Describe(issue, quality, consumption, events, options));

        return new Finding(
            meter.MeterId,
            EvidenceParts.Fingerprint(meter.MeterId, classification.Type, issue.FirstSeen),
            classification.Type,
            classification.Severity,
            confidence.Score,
            Scoring.Priority(classification.Type, classification.Severity, confidence.Score),
            $"{suspects.Count} lecturas eléctricas inconsistentes desde el {Moment(issue.FirstSeen)} mientras el consumo se mantiene estable.",
            RecommendedActions.For(classification.Type),
            evidence);
    }

    private static IReadOnlyList<double> RepeatedPowerFactors(IReadOnlyCollection<Reading> suspects)
    {
        var distinctValues = suspects.Select(reading => Math.Round(reading.PowerFactor, 2)).Distinct().Order().ToList();
        return distinctValues.Count <= MostDistinctValuesThatLookRepeated ? distinctValues : [];
    }
}
