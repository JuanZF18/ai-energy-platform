using EnergyPlatform.Domain.Readings;

namespace EnergyPlatform.Domain.Analysis;

public sealed record VoltageIssue(IReadOnlyList<Reading> Readings, bool IsOngoing)
{
    public DateTime FirstSeen => Readings[0].Timestamp;
    public DateTime LastSeen => Readings[^1].Timestamp;
    public int SpanHours => (int)(LastSeen - FirstSeen).TotalHours + 1;
    public double HighestVoltage => Readings.Max(reading => reading.VoltageV);
    public double LowestVoltage => Readings.Min(reading => reading.VoltageV);

    public bool IsOvervoltage(AnalysisOptions options) => Readings.Any(reading => reading.VoltageV > options.MaximumVoltage);
    public bool IsUndervoltage(AnalysisOptions options) => Readings.Any(reading => reading.VoltageV < options.MinimumVoltage);

    public double LargestExcessPercent(AnalysisOptions options) => Readings.Max(reading =>
        Math.Max(reading.VoltageV - options.MaximumVoltage, options.MinimumVoltage - reading.VoltageV) / options.NominalVoltage * 100);

    public static VoltageIssue? Find(IReadOnlyList<Reading> readings, AnalysisOptions options)
    {
        var outsideStandard = ElectricalQualityDetector.FindVoltageOutsideStandard(readings, options);
        return outsideStandard.Count == 0
            ? null
            : new VoltageIssue(outsideStandard, outsideStandard[^1].Timestamp >= readings[^1].Timestamp.AddHours(-24));
    }
}
