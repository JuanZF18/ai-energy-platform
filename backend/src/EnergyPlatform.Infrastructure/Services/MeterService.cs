using EnergyPlatform.Application.Anomalies;
using EnergyPlatform.Application.Common;
using EnergyPlatform.Application.Meters;
using EnergyPlatform.Domain.Events;
using EnergyPlatform.Domain.Meters;
using EnergyPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnergyPlatform.Infrastructure.Services;

public sealed class MeterService(EnergyDbContext db, ReadingSeries readingSeries, PlantTime plantTime) : IMeterService
{
    public async Task<IReadOnlyList<MeterListItemResponse>> ListAsync(MeterListQuery query, CancellationToken cancellationToken)
    {
        var meters = await db.Meters.AsNoTracking()
            .WithStatus(query.Status)
            .MatchingId(query.Search)
            .SortedBy(query.SortBy, query.Direction)
            .ToListAsync(cancellationToken);

        var badges = await CurrentBadgesAsync(cancellationToken);
        var dailyConsumption = await DailyConsumptionByMeterAsync(cancellationToken);

        var items = meters.Select(meter => new MeterListItemResponse(
            meter.MeterId,
            meter.Name,
            meter.Location,
            meter.Status,
            Math.Round(meter.CurrentDailyKwh, 1),
            RoundOrNull(meter.BaselineDailyKwh),
            RoundOrNull(meter.VariationPercent),
            badges.GetValueOrDefault(meter.MeterId),
            dailyConsumption.GetValueOrDefault(meter.MeterId) ?? []));

        return query.SortBy == MeterSortField.Severity ? [.. BySeverity(items, query.Direction)] : [.. items];
    }

    private static IEnumerable<MeterListItemResponse> BySeverity(IEnumerable<MeterListItemResponse> items, SortDirection direction)
    {
        var mostSevereFirst = items
            .OrderByDescending(item => MeterQueryFilters.SeverityRankOf(item.Status))
            .ThenByDescending(item => item.Anomaly?.Priority ?? -1)
            .ThenBy(item => item.MeterId);

        return direction == SortDirection.Ascending ? mostSevereFirst.Reverse() : mostSevereFirst;
    }

    public async Task<MeterDetailResponse?> GetAsync(string meterId, CancellationToken cancellationToken)
    {
        var meter = await FindAsync(meterId, cancellationToken);
        if (meter is null)
        {
            return null;
        }

        var readingRange = await db.Readings.Where(reading => reading.MeterId == meter.MeterId)
            .GroupBy(reading => reading.MeterId)
            .Select(group => new { Count = group.Count(), First = group.Min(r => r.Timestamp), Last = group.Max(r => r.Timestamp) })
            .SingleOrDefaultAsync(cancellationToken);

        var ranks = await db.CurrentRanksAsync(cancellationToken);
        var anomalies = await db.CurrentAnomalies().AsNoTracking()
            .Where(anomaly => anomaly.MeterId == meter.MeterId)
            .OrderByDescending(anomaly => anomaly.Priority)
            .ToListAsync(cancellationToken);

        var events = await db.Events.AsNoTracking()
            .Where(meterEvent => meterEvent.MeterId == meter.MeterId)
            .OrderBy(meterEvent => meterEvent.Timestamp)
            .ToListAsync(cancellationToken);

        return new MeterDetailResponse(
            meter.MeterId,
            meter.Name,
            meter.Location,
            meter.Status,
            Math.Round(meter.CurrentDailyKwh, 1),
            RoundOrNull(meter.BaselineDailyKwh),
            RoundOrNull(meter.VariationPercent),
            [.. meter.HourlyBaseline.Select(value => Math.Round(value, 2))],
            readingRange?.Count ?? 0,
            readingRange is null ? null : plantTime.ToOffset(readingRange.First),
            readingRange is null ? null : plantTime.ToOffset(readingRange.Last),
            [.. anomalies.Select(anomaly => anomaly.ToListItem(ranks.GetValueOrDefault(anomaly.Id), meter.Name))],
            [.. events.Select(meterEvent => new EventResponse(meterEvent.MeterId, plantTime.ToOffset(meterEvent.Timestamp), meterEvent.Type.ToCode(), meterEvent.Description))]);
    }

    public async Task<IReadOnlyList<ReadingPointResponse>?> GetReadingsAsync(string meterId, ReadingsQuery query, CancellationToken cancellationToken)
    {
        var meter = await FindAsync(meterId, cancellationToken);
        return meter is null ? null : await readingSeries.ForAsync(meter, query, cancellationToken);
    }

    private Task<Meter?> FindAsync(string meterId, CancellationToken cancellationToken) =>
        db.Meters.AsNoTracking().SingleOrDefaultAsync(meter => meter.MeterId == meterId.Trim().ToUpperInvariant(), cancellationToken);

    private async Task<Dictionary<string, AnomalyBadgeResponse>> CurrentBadgesAsync(CancellationToken cancellationToken)
    {
        var anomalies = await db.CurrentAnomalies().AsNoTracking().ToListAsync(cancellationToken);

        return anomalies
            .GroupBy(anomaly => anomaly.MeterId)
            .Select(group => group.MaxBy(anomaly => anomaly.Priority)!)
            .ToDictionary(anomaly => anomaly.MeterId, anomaly => new AnomalyBadgeResponse(anomaly.Id, anomaly.Type, anomaly.Severity, anomaly.Status, anomaly.Priority));
    }

    private async Task<Dictionary<string, IReadOnlyList<double>>> DailyConsumptionByMeterAsync(CancellationToken cancellationToken)
    {
        var rows = await db.Readings
            .GroupBy(reading => new { reading.MeterId, Day = reading.Timestamp.Date })
            .Select(group => new { group.Key.MeterId, group.Key.Day, Total = group.Sum(reading => reading.ConsumptionKwh) })
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(row => row.MeterId)
            .ToDictionary(
                meter => meter.Key,
                meter => (IReadOnlyList<double>)[.. meter.OrderBy(row => row.Day).Select(row => Math.Round(row.Total, 1))]);
    }

    private static double? RoundOrNull(double? value) => value is { } number ? Math.Round(number, 1) : null;
}
