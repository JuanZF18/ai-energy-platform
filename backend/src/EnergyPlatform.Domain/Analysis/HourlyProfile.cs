using EnergyPlatform.Domain.Readings;

namespace EnergyPlatform.Domain.Analysis;

public sealed class HourlyProfile
{
    private readonly double[] expectedByHour;

    private HourlyProfile(double[] expectedByHour) => this.expectedByHour = expectedByHour;

    public IReadOnlyList<double> ExpectedByHour => expectedByHour;

    public double DailyTotal => expectedByHour.Sum();

    public static HourlyProfile From(IEnumerable<Reading> readings)
    {
        var medianByHour = readings
            .GroupBy(reading => reading.HourOfDay)
            .ToDictionary(hour => hour.Key, hour => RobustStatistics.Median(hour.Select(reading => reading.ConsumptionKwh)));

        return new HourlyProfile([.. Enumerable.Range(0, 24).Select(hour => medianByHour.GetValueOrDefault(hour))]);
    }

    public double ExpectedAt(DateTime timestamp) => expectedByHour[timestamp.Hour];

    public double DeviationOf(Reading reading)
    {
        var expected = ExpectedAt(reading.Timestamp);
        return expected > 0 ? reading.ConsumptionKwh / expected - 1 : 0;
    }
}
