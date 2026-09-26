using EnergyPlatform.Domain.Readings;

namespace EnergyPlatform.Domain.Analysis;

public static class ElectricalQualityDetector
{
    public static IReadOnlyList<SuspectReading> FindSuspectReadings(IReadOnlyList<Reading> readings, AnalysisOptions options)
    {
        var powerFactorScores = RobustStatistics.RobustZScores([.. readings.Select(reading => reading.PowerFactor)]);
        var ratioScores = RobustStatistics.RobustZScores([.. readings.Select(reading => reading.ElectricalRatio)]);
        var suspects = new List<SuspectReading>();

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

            if (reasons != SuspicionReason.None)
            {
                suspects.Add(new SuspectReading(reading, reasons));
            }
        }

        return suspects;
    }
}
