using EnergyPlatform.Domain.Readings;
using EnergyPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;

namespace EnergyPlatform.Infrastructure.Seeding;

public static class ReadingsBulkCopy
{
    private const string CopyCommand =
        "COPY readings (meter_id, timestamp, consumption_kwh, voltage_v, current_a, power_factor, status) FROM STDIN (FORMAT BINARY)";

    public static async Task WriteAsync(EnergyDbContext db, IReadOnlyList<Reading> readings, CancellationToken cancellationToken)
    {
        var connection = (NpgsqlConnection)db.Database.GetDbConnection();
        await db.Database.OpenConnectionAsync(cancellationToken);

        try
        {
            await using var importer = await connection.BeginBinaryImportAsync(CopyCommand, cancellationToken);
            foreach (var reading in readings)
            {
                await importer.StartRowAsync(cancellationToken);
                await importer.WriteAsync(reading.MeterId, NpgsqlDbType.Varchar, cancellationToken);
                await importer.WriteAsync(reading.Timestamp, NpgsqlDbType.Timestamp, cancellationToken);
                await importer.WriteAsync(reading.ConsumptionKwh, NpgsqlDbType.Double, cancellationToken);
                await importer.WriteAsync(reading.VoltageV, NpgsqlDbType.Double, cancellationToken);
                await importer.WriteAsync(reading.CurrentA, NpgsqlDbType.Double, cancellationToken);
                await importer.WriteAsync(reading.PowerFactor, NpgsqlDbType.Double, cancellationToken);
                await importer.WriteAsync(reading.Status.ToString(), NpgsqlDbType.Varchar, cancellationToken);
            }

            await importer.CompleteAsync(cancellationToken);
        }
        finally
        {
            await db.Database.CloseConnectionAsync();
        }
    }
}
