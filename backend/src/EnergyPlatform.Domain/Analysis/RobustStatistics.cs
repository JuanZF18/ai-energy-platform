namespace EnergyPlatform.Domain.Analysis;

public static class RobustStatistics
{
    private const double NormalConsistencyFactor = 0.6745;
    private const double SmallestSpread = 1e-9;

    public static double Median(IEnumerable<double> values)
    {
        var sorted = values.Where(value => !double.IsNaN(value)).Order().ToArray();
        if (sorted.Length == 0)
        {
            return 0;
        }

        var middle = sorted.Length / 2;
        return sorted.Length % 2 == 1 ? sorted[middle] : (sorted[middle - 1] + sorted[middle]) / 2;
    }

    public static double[] RobustZScores(IReadOnlyList<double> values)
    {
        var median = Median(values);
        var spread = Math.Max(Median(values.Select(value => Math.Abs(value - median))), SmallestSpread);
        return [.. values.Select(value => double.IsNaN(value) ? double.PositiveInfinity : NormalConsistencyFactor * (value - median) / spread)];
    }
}
