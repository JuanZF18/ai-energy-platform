using EnergyPlatform.Domain.Analysis;
using EnergyPlatform.Domain.Meters;
using EnergyPlatform.Domain.Readings;
using EnergyPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EnergyPlatform.Infrastructure.Seeding;

public sealed class SeedOptions
{
    public const string SectionName = "Seed";

    public string DataDirectory { get; set; } = "data";
}

public sealed class ChallengeDataSeeder(
    EnergyDbContext db,
    IOptions<SeedOptions> options,
    IHostEnvironment environment,
    TimeProvider clock,
    ILogger<ChallengeDataSeeder> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        if (await db.Meters.AnyAsync(cancellationToken))
        {
            logger.LogInformation("Los datos del reto ya estaban cargados.");
            return;
        }

        var reader = new ChallengeDataReader(Path.GetFullPath(options.Value.DataDirectory, environment.ContentRootPath));
        var readings = reader.ReadReadings();
        var events = reader.ReadEvents();
        var catalog = reader.ReadMeterCatalog();
        ReportRejectedLines("readings.csv", readings.RejectedLines);
        ReportRejectedLines("events.csv", events.RejectedLines);

        db.Meters.AddRange(BuildMeters(readings.Rows, events.Rows.Select(meterEvent => meterEvent.MeterId), catalog.Rows));
        db.Events.AddRange(events.Rows);
        await db.SaveChangesAsync(cancellationToken);
        await ReadingsBulkCopy.WriteAsync(db, readings.Rows, cancellationToken);

        logger.LogInformation(
            "Carga inicial completa: {Meters} medidores, {Readings} lecturas y {Events} eventos.",
            db.Meters.Local.Count, readings.Rows.Count, events.Rows.Count);
    }

    private IEnumerable<Meter> BuildMeters(
        IReadOnlyList<Reading> readings,
        IEnumerable<string> meterIdsFromEvents,
        IReadOnlyList<MeterCatalogEntry> catalog)
    {
        var readingsByMeter = readings.GroupBy(reading => reading.MeterId).ToDictionary(group => group.Key, group => group.OrderBy(r => r.Timestamp).ToList());
        var catalogById = catalog.ToDictionary(entry => entry.MeterId);

        foreach (var meterId in readingsByMeter.Keys.Union(meterIdsFromEvents).Order())
        {
            var entry = catalogById.GetValueOrDefault(meterId) ?? new MeterCatalogEntry(meterId, meterId, "Sin ubicación registrada");
            var meter = new Meter(meterId, entry.Name, entry.Location, clock.GetUtcNow());
            meter.RecordCurrentConsumption(ConsumptionSnapshotBuilder.CurrentDailyConsumption(readingsByMeter.GetValueOrDefault(meterId) ?? []));
            yield return meter;
        }
    }

    private void ReportRejectedLines(string fileName, IReadOnlyList<int> rejectedLines)
    {
        if (rejectedLines.Count > 0)
        {
            logger.LogWarning("Se descartaron {Count} filas inválidas de {File}: líneas {Lines}.", rejectedLines.Count, fileName, string.Join(", ", rejectedLines));
        }
    }
}
