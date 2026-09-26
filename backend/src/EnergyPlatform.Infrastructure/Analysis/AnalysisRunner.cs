using System.Diagnostics;
using EnergyPlatform.Application.Analysis;
using EnergyPlatform.Domain.Analysis;
using EnergyPlatform.Domain.AnalysisRuns;
using EnergyPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EnergyPlatform.Infrastructure.Analysis;

public sealed class AnalysisPacingOptions
{
    public const string SectionName = "Analysis:Pacing";

    public TimeSpan MinimumStageDuration { get; set; } = TimeSpan.FromMilliseconds(350);
}

public sealed class AnalysisRunner(
    EnergyDbContext db,
    AnalysisInputLoader inputLoader,
    AnalysisResultWriter resultWriter,
    AnalysisOptions analysisOptions,
    IOptions<AnalysisPacingOptions> pacing,
    TimeProvider clock,
    ILogger<AnalysisRunner> logger) : IAnalysisRunner
{
    public async Task RunAsync(Guid analysisRunId, CancellationToken cancellationToken)
    {
        var run = await db.AnalysisRuns.SingleOrDefaultAsync(item => item.Id == analysisRunId, cancellationToken);
        if (run is null || !run.IsInProgress)
        {
            return;
        }

        run.Start(clock.GetUtcNow());
        await db.SaveChangesAsync(cancellationToken);

        try
        {
            await ExecuteStagesAsync(run, cancellationToken);
            logger.LogInformation("El análisis {AnalysisRunId} terminó: {Summary}.", run.Id, run.Summary);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "El análisis {AnalysisRunId} falló.", analysisRunId);
            await MarkAsFailedAsync(analysisRunId);
        }
    }

    private async Task ExecuteStagesAsync(AnalysisRun run, CancellationToken cancellationToken)
    {
        var engine = new AnomalyEngine(analysisOptions);

        var meters = await StageAsync(run, AnalysisStage.Readings, () => inputLoader.LoadAsync(cancellationToken), cancellationToken);
        await StageAsync(run, AnalysisStage.Baseline, () => engine.BuildBaselines(meters), cancellationToken);
        await StageAsync(run, AnalysisStage.Detection, () => engine.DetectDeviations(meters), cancellationToken);
        await StageAsync(run, AnalysisStage.Correlation, () => engine.CorrelateElectricalSignals(meters), cancellationToken);
        await StageAsync(run, AnalysisStage.Events, () => engine.MatchEvents(meters), cancellationToken);

        var findings = engine.Prioritize(engine.ComposeFindings(meters));
        var anomalies = await StageAsync(run, AnalysisStage.Explanation, () => resultWriter.RecordAnomaliesAsync(run.Id, findings, cancellationToken), cancellationToken);
        await StageAsync(run, AnalysisStage.Recommendation, () => resultWriter.ApplyToMetersAsync(meters, anomalies, cancellationToken), cancellationToken);

        run.Complete(AnalysisSummary.From(findings, meters.Count, meters.Sum(meter => meter.Readings.Count)), clock.GetUtcNow());
        await db.SaveChangesAsync(cancellationToken);
    }

    private Task StageAsync(AnalysisRun run, AnalysisStage stage, Action work, CancellationToken cancellationToken) =>
        StageAsync(run, stage, () =>
        {
            work();
            return Task.FromResult(true);
        }, cancellationToken);

    private Task StageAsync(AnalysisRun run, AnalysisStage stage, Func<Task> work, CancellationToken cancellationToken) =>
        StageAsync(run, stage, async () =>
        {
            await work();
            return true;
        }, cancellationToken);

    private async Task<T> StageAsync<T>(AnalysisRun run, AnalysisStage stage, Func<Task<T>> work, CancellationToken cancellationToken)
    {
        run.BeginStage(stage, clock.GetUtcNow());
        await db.SaveChangesAsync(cancellationToken);

        var stopwatch = Stopwatch.StartNew();
        var result = await work();
        var remaining = pacing.Value.MinimumStageDuration - stopwatch.Elapsed;
        if (remaining > TimeSpan.Zero)
        {
            await Task.Delay(remaining, cancellationToken);
        }

        run.FinishStage(stage, clock.GetUtcNow());
        await db.SaveChangesAsync(cancellationToken);
        return result;
    }

    private async Task MarkAsFailedAsync(Guid analysisRunId)
    {
        db.ChangeTracker.Clear();
        var run = await db.AnalysisRuns.SingleAsync(item => item.Id == analysisRunId);
        run.Fail("El análisis no pudo completarse. Revisa los registros del servidor.", clock.GetUtcNow());
        await db.SaveChangesAsync();
    }
}
