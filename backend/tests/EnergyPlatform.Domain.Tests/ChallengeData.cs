using EnergyPlatform.Domain.Analysis;
using EnergyPlatform.Infrastructure.Seeding;

namespace EnergyPlatform.Domain.Tests;

public static class ChallengeData
{
    private static readonly Lazy<ChallengeDataReader> Reader = new(() => new ChallengeDataReader(FindDataDirectory()));

    public static ChallengeDataReader Files => Reader.Value;

    public static IReadOnlyList<MeterAnalysis> MetersToAnalyze()
    {
        var events = Files.ReadEvents().Rows;
        return
        [
            .. Files.ReadReadings().Rows
                .GroupBy(reading => reading.MeterId)
                .Select(meter => new MeterAnalysis(meter.Key, meter, events.Where(meterEvent => meterEvent.MeterId == meter.Key)))
        ];
    }

    private static string FindDataDirectory()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "data", "readings.csv")))
        {
            directory = directory.Parent;
        }

        return directory is null
            ? throw new DirectoryNotFoundException("No se encontró la carpeta data con readings.csv.")
            : Path.Combine(directory.FullName, "data");
    }
}
