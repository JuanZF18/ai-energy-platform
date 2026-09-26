using EnergyPlatform.Domain.Readings;

namespace EnergyPlatform.Domain.Analysis;

public sealed record DataQualityIssue(IReadOnlyList<SuspectReading> Readings, bool IsOngoing)
{
    public DateTime FirstSeen => Readings[0].Reading.Timestamp;
    public DateTime LastSeen => Readings[^1].Reading.Timestamp;
    public int SpanHours => (int)(LastSeen - FirstSeen).TotalHours + 1;

    public int? RepeatsEveryHours
    {
        get
        {
            var gaps = Readings
                .Zip(Readings.Skip(1), (previous, next) => (int)(next.Reading.Timestamp - previous.Reading.Timestamp).TotalHours)
                .Distinct()
                .ToList();

            return gaps.Count == 1 ? gaps[0] : null;
        }
    }

    public bool Has(SuspicionReason reason) => Readings.Any(suspect => suspect.Reasons.HasFlag(reason));

    public static DataQualityIssue? Find(
        IReadOnlyList<Reading> readings,
        IReadOnlyList<ConsumptionShift> shifts,
        AnalysisOptions options)
    {
        var suspectsWithNormalConsumption = ElectricalQualityDetector
            .FindSuspectReadings(readings, options)
            .Where(suspect => !shifts.Any(shift => shift.Contains(suspect.Reading.Timestamp)))
            .ToList();

        if (suspectsWithNormalConsumption.Count < options.MinimumSuspectReadings)
        {
            return null;
        }

        var lastSuspect = suspectsWithNormalConsumption[^1].Reading.Timestamp;
        return new DataQualityIssue(suspectsWithNormalConsumption, lastSuspect >= readings[^1].Timestamp.AddHours(-24));
    }
}
