using EnergyPlatform.Application.Common;
using EnergyPlatform.Application.Dashboard;
using EnergyPlatform.Application.Meters;
using EnergyPlatform.Domain.Anomalies;
using EnergyPlatform.Domain.Events;
using EnergyPlatform.Domain.Meters;
using EnergyPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnergyPlatform.Infrastructure.Services;

public sealed class DashboardService(EnergyDbContext db, PlantTime plantTime) : IDashboardService
{
    public async Task<DashboardSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken)
    {
        var statuses = await db.Meters.Select(meter => meter.Status).ToListAsync(cancellationToken);
        var anomalies = await db.CurrentAnomalies().AsNoTracking().ToListAsync(cancellationToken);
        var realAnomalies = anomalies.Where(anomaly => anomaly.Type != AnomalyType.FalsePositive).ToList();

        return new DashboardSummaryResponse(
            new MeterCountsResponse(
                statuses.Count,
                statuses.Count(status => status == MeterStatus.Ok),
                statuses.Count(status => status == MeterStatus.Alert),
                statuses.Count(status => status == MeterStatus.Critical),
                statuses.Count(status => status == MeterStatus.NotAnalyzed)),
            await ConsumptionTotalsAsync(cancellationToken),
            new AnomalyCountsResponse(
                anomalies.Count,
                realAnomalies.Count,
                anomalies.Count - realAnomalies.Count,
                anomalies.Count(anomaly => anomaly.Severity == Severity.High && anomaly.Type is AnomalyType.RealAnomaly or AnomalyType.DataQuality)),
            realAnomalies.Count > 0 ? Math.Round(realAnomalies.Average(anomaly => anomaly.Confidence), 2) : null,
            await LastAnalysisAsync(cancellationToken),
            await DailyConsumptionAsync(cancellationToken),
            await EventsAsync(cancellationToken));
    }

    private async Task<ConsumptionTotalsResponse> ConsumptionTotalsAsync(CancellationToken cancellationToken)
    {
        var period = await db.Readings
            .GroupBy(_ => 1)
            .Select(all => new { Total = all.Sum(r => r.ConsumptionKwh), Start = all.Min(r => r.Timestamp), End = all.Max(r => r.Timestamp) })
            .SingleOrDefaultAsync(cancellationToken);

        var lastDay = await db.Meters.SumAsync(meter => meter.CurrentDailyKwh, cancellationToken);

        return new ConsumptionTotalsResponse(
            Math.Round(period?.Total ?? 0, 1),
            Math.Round(lastDay, 1),
            period is null ? null : plantTime.ToOffset(period.Start),
            period is null ? null : plantTime.ToOffset(period.End));
    }

    private async Task<LastAnalysisResponse?> LastAnalysisAsync(CancellationToken cancellationToken)
    {
        var run = await db.AnalysisRuns.AsNoTracking().OrderByDescending(item => item.RequestedAt).FirstOrDefaultAsync(cancellationToken);

        return run is null
            ? null
            : new LastAnalysisResponse(run.Id, run.Status, run.RequestedAt, run.FinishedAt, (run.FinishedAt - run.StartedAt)?.TotalSeconds);
    }

    private async Task<IReadOnlyList<DailyConsumptionResponse>> DailyConsumptionAsync(CancellationToken cancellationToken)
    {
        var days = await db.Readings
            .GroupBy(reading => reading.Timestamp.Date)
            .Select(day => new { Day = day.Key, Total = day.Sum(reading => reading.ConsumptionKwh) })
            .OrderBy(day => day.Day)
            .ToListAsync(cancellationToken);

        return [.. days.Select(day => new DailyConsumptionResponse(DateOnly.FromDateTime(day.Day), Math.Round(day.Total, 1)))];
    }

    private async Task<IReadOnlyList<EventResponse>> EventsAsync(CancellationToken cancellationToken)
    {
        var events = await db.Events.AsNoTracking().OrderBy(meterEvent => meterEvent.Timestamp).ToListAsync(cancellationToken);
        return [.. events.Select(meterEvent => new EventResponse(meterEvent.MeterId, plantTime.ToOffset(meterEvent.Timestamp), meterEvent.Type.ToCode(), meterEvent.Description))];
    }
}
