using EnergyPlatform.Domain.Events;

namespace EnergyPlatform.Domain.Analysis;

public static class EventMatcher
{
    public static IReadOnlyList<RelatedEvent> ForShift(ConsumptionShift shift, IEnumerable<MeterEvent> events, AnalysisOptions options) =>
        events
            .Where(meterEvent => IsNear(meterEvent, shift.Start, options) || Overlaps(meterEvent, shift))
            .Select(meterEvent => new RelatedEvent(
                meterEvent,
                Explains(meterEvent, shift, options) ? EventRelation.ExplainsChange : EventRelation.DoesNotExplain))
            .ToList();

    public static IReadOnlyList<RelatedEvent> ForDataQualityIssue(DataQualityIssue issue, IEnumerable<MeterEvent> events, AnalysisOptions options) =>
        events
            .Where(meterEvent => meterEvent.Timestamp >= issue.FirstSeen - options.EventTolerance && meterEvent.Timestamp <= issue.LastSeen)
            .Select(meterEvent => new RelatedEvent(
                meterEvent,
                meterEvent.Type == MeterEventType.DataQuality ? EventRelation.ConfirmsIssue : EventRelation.DoesNotExplain))
            .ToList();

    private static bool Explains(MeterEvent meterEvent, ConsumptionShift shift, AnalysisOptions options) => meterEvent.Type switch
    {
        MeterEventType.OperationalChange => IsNear(meterEvent, shift.Start, options),
        MeterEventType.ScheduledOutage => shift.Direction == ShiftDirection.Down && FitsOutageWindow(meterEvent, shift, options),
        _ => false
    };

    private static bool IsNear(MeterEvent meterEvent, DateTime moment, AnalysisOptions options) =>
        (meterEvent.Timestamp - moment).Duration() <= options.EventTolerance;

    private static bool Overlaps(MeterEvent meterEvent, ConsumptionShift shift) =>
        meterEvent.Timestamp <= shift.End && EndOf(meterEvent) >= shift.Start;

    private static bool FitsOutageWindow(MeterEvent meterEvent, ConsumptionShift shift, AnalysisOptions options) =>
        shift.Start >= meterEvent.Timestamp - options.EventTolerance
        && shift.End <= EndOf(meterEvent) + options.EventTolerance;

    private static DateTime EndOf(MeterEvent meterEvent) => meterEvent.Timestamp + (meterEvent.DeclaredDuration ?? TimeSpan.Zero);
}
