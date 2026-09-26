using EnergyPlatform.Application.Common;
using EnergyPlatform.Application.Meters;
using EnergyPlatform.Domain.Meters;
using EnergyPlatform.Domain.Readings;
using EnergyPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnergyPlatform.Infrastructure.Services;

public sealed class ReadingSeries(EnergyDbContext db, PlantTime plantTime)
{
    public async Task<IReadOnlyList<ReadingPointResponse>> ForAsync(Meter meter, ReadingsQuery query, CancellationToken cancellationToken)
    {
        var readings = db.Readings.AsNoTracking().Where(reading => reading.MeterId == meter.MeterId);

        if (query.From is { } from)
        {
            var start = plantTime.ToPlantLocal(from);
            readings = readings.Where(reading => reading.Timestamp >= start);
        }

        if (query.To is { } to)
        {
            var end = plantTime.ToPlantLocal(to);
            readings = readings.Where(reading => reading.Timestamp < end);
        }

        return query.Granularity == ReadingGranularity.Day
            ? await DailyAsync(meter, readings, cancellationToken)
            : await HourlyAsync(meter, readings, cancellationToken);
    }

    private async Task<IReadOnlyList<ReadingPointResponse>> HourlyAsync(Meter meter, IQueryable<Reading> readings, CancellationToken cancellationToken)
    {
        var rows = await readings.OrderBy(reading => reading.Timestamp).ToListAsync(cancellationToken);
        var hasBaseline = meter.HourlyBaseline.Length == 24;

        return [.. rows.Select(reading => new ReadingPointResponse(
            plantTime.ToOffset(reading.Timestamp),
            reading.ConsumptionKwh,
            reading.VoltageV,
            reading.CurrentA,
            reading.PowerFactor,
            hasBaseline ? Math.Round(meter.HourlyBaseline[reading.HourOfDay], 2) : null,
            reading.Status == ReadingStatus.Suspect))];
    }

    private async Task<IReadOnlyList<ReadingPointResponse>> DailyAsync(Meter meter, IQueryable<Reading> readings, CancellationToken cancellationToken)
    {
        var days = await readings
            .GroupBy(reading => reading.Timestamp.Date)
            .Select(day => new
            {
                Day = day.Key,
                Consumption = day.Sum(reading => reading.ConsumptionKwh),
                Voltage = day.Average(reading => reading.VoltageV),
                Current = day.Average(reading => reading.CurrentA),
                PowerFactor = day.Average(reading => reading.PowerFactor),
                HasSuspects = day.Any(reading => reading.Status == ReadingStatus.Suspect)
            })
            .OrderBy(day => day.Day)
            .ToListAsync(cancellationToken);

        return [.. days.Select(day => new ReadingPointResponse(
            plantTime.ToOffset(day.Day),
            Math.Round(day.Consumption, 2),
            Math.Round(day.Voltage, 2),
            Math.Round(day.Current, 2),
            Math.Round(day.PowerFactor, 3),
            meter.BaselineDailyKwh is { } baseline ? Math.Round(baseline, 2) : null,
            day.HasSuspects))];
    }
}
