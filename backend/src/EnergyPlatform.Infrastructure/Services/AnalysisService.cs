using EnergyPlatform.Application.Analysis;
using EnergyPlatform.Domain.AnalysisRuns;
using EnergyPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnergyPlatform.Infrastructure.Services;

public sealed class AnalysisService(EnergyDbContext db, IAnalysisQueue queue, TimeProvider clock) : IAnalysisService
{
    public async Task<AnalysisRequestResult> RequestAsync(CancellationToken cancellationToken)
    {
        var runInProgress = await db.AnalysisRuns.AsNoTracking()
            .Where(run => run.Status == AnalysisRunStatus.Queued || run.Status == AnalysisRunStatus.Running)
            .OrderByDescending(run => run.RequestedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (runInProgress is not null)
        {
            return new AnalysisRequestResult(runInProgress.ToResponse(), true);
        }

        var newRun = AnalysisRun.Request(clock.GetUtcNow());
        db.AnalysisRuns.Add(newRun);
        await db.SaveChangesAsync(cancellationToken);
        await queue.QueueAsync(newRun.Id, cancellationToken);

        return new AnalysisRequestResult(newRun.ToResponse(), false);
    }

    public async Task<AnalysisRunResponse?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var run = await db.AnalysisRuns.AsNoTracking().SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        return run?.ToResponse();
    }
}
