using EnergyPlatform.Domain.Events;
using EnergyPlatform.Infrastructure.Seeding;

namespace EnergyPlatform.Domain.Tests;

public sealed class CsvReadingTests : IDisposable
{
    private readonly string directory = Directory.CreateTempSubdirectory("vatio-csv-").FullName;

    [Fact]
    public void Keeps_valid_readings_and_reports_the_broken_lines()
    {
        Write("readings.csv",
            "meter_id,timestamp,consumption_kwh,voltage_v,current_a,power_factor,status",
            "M-1,2026-09-01 00:00:00,10.5,220,50,0.95,OK",
            "M-1,2026-09-01 01:00,11,221,51,0.94,OK",
            "M-1,no-es-fecha,10,220,50,0.95,OK",
            "M-1,2026-09-01 03:00:00,abc,220,50,0.95,OK",
            "M-1,2026-09-01 04:00:00,NaN,220,50,0.95,OK",
            "M-1,2026-09-01 05:00:00,10",
            "",
            "M-1,2026-09-01 06:00:00,12,219,52,0.93,OK");

        var result = new ChallengeDataReader(directory).ReadReadings();

        Assert.Equal(3, result.Rows.Count);
        Assert.Equal([4, 5, 6, 7], result.RejectedLines);
    }

    [Fact]
    public void Treats_an_unknown_event_type_as_unclassified_so_it_explains_nothing()
    {
        Write("events.csv",
            "meter_id,event_timestamp,event_type,description",
            "M-1,2026-09-01 00:00,MAINTENANCE_WINDOW,Something new",
            "M-1,2026-09-02 00:00,OPERATIONAL_CHANGE,Line started, with a comma");

        var events = new ChallengeDataReader(directory).ReadEvents().Rows;

        Assert.Equal(MeterEventType.Unknown, events[0].Type);
        Assert.Equal(MeterEventType.OperationalChange, events[1].Type);
        Assert.Equal("Line started, with a comma", events[1].Description);
    }

    public void Dispose() => Directory.Delete(directory, recursive: true);

    private void Write(string fileName, params string[] lines) => File.WriteAllLines(Path.Combine(directory, fileName), lines);
}
