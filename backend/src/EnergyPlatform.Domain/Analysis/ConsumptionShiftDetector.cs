using EnergyPlatform.Domain.Readings;

namespace EnergyPlatform.Domain.Analysis;

public static class ConsumptionShiftDetector
{
    public static IReadOnlyList<ConsumptionShift> Detect(IReadOnlyList<Reading> readings, HourlyProfile profile, AnalysisOptions options)
    {
        var shifts = new List<ConsumptionShift>();
        var run = new List<DeviatingReading>();

        foreach (var reading in readings)
        {
            var deviation = profile.DeviationOf(reading);
            var isOutsideExpected = Math.Abs(deviation) > options.DeviationThreshold;
            var continuesRun = run.Count > 0 && isOutsideExpected && Math.Sign(deviation) == Math.Sign(run[0].Deviation);

            if (!continuesRun)
            {
                AddShiftIfLongEnough(run, readings, options, shifts);
                run.Clear();
            }

            if (isOutsideExpected)
            {
                run.Add(new DeviatingReading(reading, deviation));
            }
        }

        AddShiftIfLongEnough(run, readings, options, shifts);
        return shifts;
    }

    private static void AddShiftIfLongEnough(
        List<DeviatingReading> run,
        IReadOnlyList<Reading> readings,
        AnalysisOptions options,
        List<ConsumptionShift> shifts)
    {
        if (run.Count < options.MinimumShiftHours)
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
