using EnergyPlatform.Domain.Readings;

namespace EnergyPlatform.Domain.Analysis;

public static class ElectricalQualityDetector
{
    public static IReadOnlyList<SuspectReading> FindSuspectReadings(IReadOnlyList<Reading> readings, AnalysisOptions options) =>
        [.. Assess(readings, options).Where(suspect => suspect.Reasons != SuspicionReason.None && !IsCoherentVoltageDeviation(suspect))];

    public static IReadOnlyList<Reading> FindVoltageOutsideStandard(IReadOnlyList<Reading> readings, AnalysisOptions options) =>
        [.. Assess(readings, options).Where(IsCoherentVoltageDeviation).Select(suspect => suspect.Reading)];

    private static bool IsCoherentVoltageDeviation(SuspectReading suspect) => suspect.Reasons == SuspicionReason.VoltageOutOfRange;

    private static IEnumerable<SuspectReading> Assess(IReadOnlyList<Reading> readings, AnalysisOptions options)
    {
        var powerFactorScores = RobustStatistics.RobustZScores([.. readings.Select(reading => reading.PowerFactor)]);
        var ratioScores = RobustStatistics.RobustZScores([.. readings.Select(reading => reading.ElectricalRatio)]);

        for (var index = 0; index < readings.Count; index++)
        {
            var reading = readings[index];
            var reasons = SuspicionReason.None;

            if (reading.VoltageV < options.MinimumVoltage || reading.VoltageV > options.MaximumVoltage)
            {
                reasons |= SuspicionReason.VoltageOutOfRange;
            }

            if (Math.Abs(powerFactorScores[index]) > options.RobustZThreshold)
            {
                reasons |= SuspicionReason.AtypicalPowerFactor;
            }

            if (Math.Abs(ratioScores[index]) > options.RobustZThreshold)
            {
                reasons |= SuspicionReason.InconsistentElectricalRatio;
            }

            yield return new SuspectReading(reading, reasons);
        }
    }
}
