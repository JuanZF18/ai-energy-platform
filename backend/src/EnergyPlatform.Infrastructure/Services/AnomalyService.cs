using EnergyPlatform.Application.Anomalies;
using EnergyPlatform.Application.Common;
using EnergyPlatform.Domain.Meters;
using EnergyPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnergyPlatform.Infrastructure.Services;

public sealed class AnomalyService(EnergyDbContext db, PlantTime plantTime, TimeProvider clock) : IAnomalyService
{
    public async Task<IReadOnlyList<AnomalyListItemResponse>> ListAsync(AnomalyListQuery query, CancellationToken cancellationToken)
    {
        var ranks = await db.CurrentRanksAsync(cancellationToken);
        var meterNames = await db.Meters.ToDictionaryAsync(meter => meter.MeterId, meter => meter.Name, cancellationToken);
        var anomalies = db.CurrentAnomalies().AsNoTracking();

        if (query.Type is { } type)
        {
            anomalies = anomalies.Where(anomaly => anomaly.Type == type);
        }

        if (query.Status is { } status)
        {
            anomalies = anomalies.Where(anomaly => anomaly.Status == status);
        }

        var ordered = await anomalies
            .OrderByDescending(anomaly => anomaly.Priority)
            .ThenBy(anomaly => anomaly.MeterId)
            .Take(query.Limit ?? 100)
            .ToListAsync(cancellationToken);

        return [.. ordered.Select(anomaly => anomaly.ToListItem(ranks.GetValueOrDefault(anomaly.Id), meterNames.GetValueOrDefault(anomaly.MeterId, anomaly.MeterId)))];
    }

    public async Task<AnomalyDetailResponse?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var anomaly = await db.Anomalies.AsNoTracking()
            .Include(item => item.StatusChanges)
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (anomaly is null)
        {
            return null;
        }

        var meter = await db.Meters.AsNoTracking().SingleAsync(item => item.MeterId == anomaly.MeterId, cancellationToken);
        var ranks = await db.CurrentRanksAsync(cancellationToken);
        return anomaly.ToDetail(ranks.GetValueOrDefault(anomaly.Id), meter.Name, meter.Location, plantTime);
    }

    public async Task<AnomalyDetailResponse?> ChangeStatusAsync(Guid id, UpdateAnomalyStatusRequest request, string changedBy, CancellationToken cancellationToken)
    {
        var anomaly = await db.Anomalies.Include(item => item.StatusChanges).SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (anomaly is null)
        {
            return null;
        }

        anomaly.ChangeStatus(request.Status, request.Note?.Trim(), changedBy, clock.GetUtcNow());
        await RefreshMeterStatusAsync(anomaly.MeterId, anomaly.AnalysisRunId, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return await GetAsync(id, cancellationToken);
    }

    private async Task RefreshMeterStatusAsync(string meterId, Guid analysisRunId, CancellationToken cancellationToken)
    {
        var isCurrentRun = await db.LatestCompletedRunId().ContainsAsync(analysisRunId, cancellationToken);
        if (!isCurrentRun)
        {
            return;
        }

        var meterAnomalies = await db.Anomalies
            .Where(anomaly => anomaly.MeterId == meterId && anomaly.AnalysisRunId == analysisRunId)
            .ToListAsync(cancellationToken);

        var meter = await db.Meters.SingleAsync(item => item.MeterId == meterId, cancellationToken);
        meter.ChangeStatus(MeterStatusPolicy.From(meterAnomalies));
    }
}
