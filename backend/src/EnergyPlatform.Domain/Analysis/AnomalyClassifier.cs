using EnergyPlatform.Domain.Anomalies;
using EnergyPlatform.Domain.Events;

namespace EnergyPlatform.Domain.Analysis;

public static class AnomalyClassifier
{
    public static Classification ClassifyShift(
        ConsumptionShift shift,
        ElectricalChange change,
        IReadOnlyList<RelatedEvent> events,
        AnalysisOptions options)
    {
        var explainingEvent = events.FirstOrDefault(related => related.Relation == EventRelation.ExplainsChange)?.Event;
        var isDegraded = change.ShowsDegradation(options);

        if (explainingEvent?.Type == MeterEventType.ScheduledOutage)
        {
            return new Classification(AnomalyType.FalsePositive, Severity.Low,
                [true, shift.Direction == ShiftDirection.Down, !shift.IsOngoing, MatchesDeclaredDuration(shift, explainingEvent, options)]);
        }

        if (explainingEvent?.Type == MeterEventType.OperationalChange && !isDegraded)
        {
            return new Classification(AnomalyType.ExplainableAnomaly, Severity.Medium,
                [true, change.CurrentFollowsConsumption, !isDegraded, shift.IsOngoing]);
        }

        var severity = Math.Abs(shift.MeanDeviation) >= options.HighSeverityDeviation || isDegraded
            ? Severity.High
            : Severity.Medium;

        return new Classification(AnomalyType.RealAnomaly, severity,
            [change.CurrentFollowsConsumption, isDegraded, explainingEvent is null, shift.IsOngoing]);
    }

    public static Classification ClassifyDataQuality(DataQualityIssue issue, IReadOnlyList<RelatedEvent> events) =>
        new(AnomalyType.DataQuality, issue.IsOngoing ? Severity.High : Severity.Medium,
        [
            issue.Has(SuspicionReason.VoltageOutOfRange),
            issue.Has(SuspicionReason.AtypicalPowerFactor) || issue.Has(SuspicionReason.InconsistentElectricalRatio),
            events.Any(related => related.Relation == EventRelation.ConfirmsIssue),
            issue.RepeatsEveryHours is not null
        ]);

    private static bool MatchesDeclaredDuration(ConsumptionShift shift, MeterEvent outage, AnalysisOptions options) =>
        outage.DeclaredDuration is { } duration
        && Math.Abs(shift.Hours - duration.TotalHours) <= options.EventTolerance.TotalHours;
}
