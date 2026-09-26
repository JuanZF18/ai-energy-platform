using EnergyPlatform.Domain.AnalysisRuns;
using EnergyPlatform.Domain.Anomalies;
using Microsoft.EntityFrameworkCore;

namespace EnergyPlatform.Infrastructure.Persistence;

public static class CurrentAnalysisQueries
{
    public static IQueryable<Guid> LatestCompletedRunId(this EnergyDbContext db) =>
        db.AnalysisRuns
            .Where(run => run.Status == AnalysisRunStatus.Completed)
            .OrderByDescending(run => run.FinishedAt)
            .Select(run => run.Id)
            .Take(1);

    public static IQueryable<Anomaly> CurrentAnomalies(this EnergyDbContext db) =>
        db.Anomalies.Where(anomaly => db.LatestCompletedRunId().Contains(anomaly.AnalysisRunId));

    public static async Task<Dictionary<Guid, int>> CurrentRanksAsync(this EnergyDbContext db, CancellationToken cancellationToken)
    {
        var orderedIds = await db.CurrentAnomalies()
            .OrderByDescending(anomaly => anomaly.Priority)
            .ThenBy(anomaly => anomaly.MeterId)
            .Select(anomaly => anomaly.Id)
            .ToListAsync(cancellationToken);

        return orderedIds.Select((id, index) => (id, rank: index + 1)).ToDictionary(item => item.id, item => item.rank);
    }
}
