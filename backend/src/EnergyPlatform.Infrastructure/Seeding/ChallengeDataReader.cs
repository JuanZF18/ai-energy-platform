using System.Globalization;
using EnergyPlatform.Domain.Events;
using EnergyPlatform.Domain.Readings;

namespace EnergyPlatform.Infrastructure.Seeding;

public sealed class ChallengeDataReader(string dataDirectory)
{
    private static readonly string[] TimestampFormats = ["yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd HH:mm"];

    public CsvLoadResult<Reading> ReadReadings() => Load("readings.csv", TryReadReading);

    public CsvLoadResult<MeterEvent> ReadEvents() => Load("events.csv", TryReadEvent);

    public CsvLoadResult<MeterCatalogEntry> ReadMeterCatalog() =>
        File.Exists(PathOf("meters.csv")) ? Load("meters.csv", TryReadCatalogEntry) : new CsvLoadResult<MeterCatalogEntry>([], []);

    public static DateTime ParseTimestamp(string value) =>
        DateTime.ParseExact(value.Trim(), TimestampFormats, CultureInfo.InvariantCulture, DateTimeStyles.None);

    private CsvLoadResult<T> Load<T>(string fileName, Func<string[], T?> tryRead) where T : class
    {
        var rows = new List<T>();
        var rejectedLines = new List<int>();
        var lineNumber = 1;

        foreach (var line in File.ReadLines(PathOf(fileName)).Skip(1))
        {
            lineNumber++;
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if (tryRead(line.Split(',')) is { } row)
            {
                rows.Add(row);
            }
            else
            {
                rejectedLines.Add(lineNumber);
            }
        }

        return new CsvLoadResult<T>(rows, rejectedLines);
    }

    private string PathOf(string fileName) => Path.Combine(dataDirectory, fileName);

    private static Reading? TryReadReading(string[] columns)
    {
        if (columns.Length < 6 || !TryParseTimestamp(columns[1], out var timestamp))
        {
            return null;
        }

        return TryParseNumber(columns[2], out var consumption)
            && TryParseNumber(columns[3], out var voltage)
            && TryParseNumber(columns[4], out var current)
            && TryParseNumber(columns[5], out var powerFactor)
            ? new Reading(columns[0].Trim(), timestamp, consumption, voltage, current, powerFactor)
            : null;
    }

    private static MeterEvent? TryReadEvent(string[] columns) =>
        columns.Length >= 4 && TryParseTimestamp(columns[1], out var timestamp)
            ? new MeterEvent(columns[0].Trim(), timestamp, MeterEventTypeCode.FromCode(columns[2]), string.Join(',', columns[3..]).Trim())
            : null;

    private static MeterCatalogEntry? TryReadCatalogEntry(string[] columns) =>
        columns.Length >= 3 ? new MeterCatalogEntry(columns[0].Trim(), columns[1].Trim(), columns[2].Trim()) : null;

    private static bool TryParseTimestamp(string value, out DateTime timestamp) =>
        DateTime.TryParseExact(value.Trim(), TimestampFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out timestamp);

    private static bool TryParseNumber(string value, out double number) =>
        double.TryParse(value.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out number);
}
