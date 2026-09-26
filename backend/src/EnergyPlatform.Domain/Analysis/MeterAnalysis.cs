using EnergyPlatform.Domain.Events;
using EnergyPlatform.Domain.Meters;
using EnergyPlatform.Domain.Readings;

namespace EnergyPlatform.Domain.Analysis;

public sealed class MeterAnalysis(string meterId, IEnumerable<Reading> readings, IEnumerable<MeterEvent> events)
{
    public string MeterId { get; } = meterId;
    public IReadOnlyList<Reading> Readings { get; } = [.. readings.OrderBy(reading => reading.Timestamp)];
    public IReadOnlyList<MeterEvent> Events { get; } = [.. events.OrderBy(meterEvent => meterEvent.Timestamp)];

    public HourlyProfile? DetectionProfile { get; internal set; }
    public IReadOnlyList<ConsumptionShift> Shifts { get; internal set; } = [];
    public DataQualityIssue? DataQualityIssue { get; internal set; }
    public ConsumptionSnapshot? Snapshot { get; internal set; }
    public IReadOnlyDictionary<ConsumptionShift, ElectricalChange> ElectricalChanges { get; internal set; } =
        new Dictionary<ConsumptionShift, ElectricalChange>();
    public IReadOnlyDictionary<ConsumptionShift, IReadOnlyList<RelatedEvent>> ShiftEvents { get; internal set; } =
        new Dictionary<ConsumptionShift, IReadOnlyList<RelatedEvent>>();
    public IReadOnlyList<RelatedEvent> DataQualityEvents { get; internal set; } = [];

    public ConsumptionSnapshot RequiredSnapshot =>
        Snapshot ?? throw new InvalidOperationException($"El medidor {MeterId} todavía no tiene baseline.");
}
