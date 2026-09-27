using EnergyPlatform.Domain.Readings;

namespace EnergyPlatform.Domain.Analysis;

public static class ConsumptionShiftDetector
{
    public static IReadOnlyList<ConsumptionShift> Detect(IReadOnlyList<Reading> readings, HourlyProfile profile, AnalysisOptions options)
    {
        var shifts = new List<ConsumptionShift>();
        var run = new List<DeviatingReading>();
        var gap = new List<DeviatingReading>();

        foreach (var reading in readings)
        {
            var deviating = new DeviatingReading(reading, profile.DeviationOf(reading));
            var isOutsideExpected = Math.Abs(deviating.Deviation) > options.DeviationThreshold;

            if (run.Count == 0)
            {
                if (isOutsideExpected)
                {
                    run.Add(deviating);
                }

                continue;
            }

            if (isOutsideExpected && Math.Sign(deviating.Deviation) == Math.Sign(run[0].Deviation))
            {
                run.AddRange(gap);
                gap.Clear();
                run.Add(deviating);
                continue;
            }

            if (!isOutsideExpected && gap.Count < options.MaximumShiftGapHours)
            {
                gap.Add(deviating);
                continue;
            }

            AddShiftIfSignificant(run, readings, options, shifts);
            run.Clear();
            gap.Clear();
            if (isOutsideExpected)
            {
                run.Add(deviating);
            }
        }

        AddShiftIfSignificant(run, readings, options, shifts);
        return shifts;
    }

    private static void AddShiftIfSignificant(
        List<DeviatingReading> run,
        IReadOnlyList<Reading> readings,
        AnalysisOptions options,
        List<ConsumptionShift> shifts)
    {
        var isLongEnough = run.Count >= options.MinimumShiftHours;
        var isSevereEnough = run.Any(item => Math.Abs(item.Deviation) >= options.HighSeverityDeviation);
        if (!isLongEnough && !isSevereEnough)
        {
            return;
        }

        var end = run[^1].Reading.Timestamp;
        shifts.Add(new ConsumptionShift(
            run[0].Reading.Timestamp,
            end,
            run.Count,
            run[0].Deviation > 0 ? ShiftDirection.Up : ShiftDirection.Down,
            run.Average(item => item.Deviation),
            end == readings[^1].Timestamp));
    }

    private sealed record DeviatingReading(Reading Reading, double Deviation);
}
