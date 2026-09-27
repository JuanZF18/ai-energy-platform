using EnergyPlatform.Domain.Anomalies;

namespace EnergyPlatform.Domain.Analysis;

public sealed class AnomalyEngine(AnalysisOptions options)
{
    public void BuildBaselines(IEnumerable<MeterAnalysis> meters)
    {
        foreach (var meter in meters)
        {
            meter.DetectionProfile = HourlyProfile.From(meter.Readings);
        }
    }

    public void DetectDeviations(IEnumerable<MeterAnalysis> meters)
    {
        foreach (var meter in meters)
        {
            meter.Shifts = ConsumptionShiftDetector.Detect(meter.Readings, ProfileOf(meter), options);
            meter.DataQualityIssue = DataQualityIssue.Find(meter.Readings, meter.Shifts, options);
            meter.VoltageIssue = VoltageIssue.Find(meter.Readings, options);
        }
    }

    public void CorrelateElectricalSignals(IEnumerable<MeterAnalysis> meters)
    {
        foreach (var meter in meters)
        {
            meter.ElectricalChanges = meter.Shifts.ToDictionary(shift => shift, shift => ElectricalChange.Around(shift, meter.Readings));
            meter.Snapshot = ConsumptionSnapshotBuilder.Build(meter.Readings, meter.Shifts, ProfileOf(meter));
        }
    }

    public void MatchEvents(IEnumerable<MeterAnalysis> meters)
    {
        foreach (var meter in meters)
        {
            meter.ShiftEvents = meter.Shifts.ToDictionary(shift => shift, shift => EventMatcher.ForShift(shift, meter.Events, options));
            meter.DataQualityEvents = meter.DataQualityIssue is { } issue
                ? EventMatcher.ForDataQualityIssue(issue, meter.Events, options)
                : [];
        }
    }

    public IReadOnlyList<Finding> ComposeFindings(IEnumerable<MeterAnalysis> meters) =>
    [
        .. meters.SelectMany(meter => meter.Shifts
            .Select(shift => ShiftFindingComposer.Compose(meter, shift, options))
            .Concat(meter.DataQualityIssue is { } issue ? [DataQualityFindingComposer.Compose(meter, issue, options)] : [])
            .Concat(meter.VoltageIssue is { } voltage ? [VoltageFindingComposer.Compose(meter, voltage, options)] : []))
    ];

    public IReadOnlyList<Finding> Prioritize(IEnumerable<Finding> findings) =>
        [.. findings.OrderByDescending(finding => finding.Priority).ThenBy(finding => finding.MeterId)];

    public IReadOnlyList<Finding> Analyze(IReadOnlyList<MeterAnalysis> meters)
    {
        BuildBaselines(meters);
        DetectDeviations(meters);
        CorrelateElectricalSignals(meters);
        MatchEvents(meters);
        return Prioritize(ComposeFindings(meters));
    }

    private static HourlyProfile ProfileOf(MeterAnalysis meter) =>
        meter.DetectionProfile ?? throw new InvalidOperationException($"El medidor {meter.MeterId} todavía no tiene perfil horario.");
}
