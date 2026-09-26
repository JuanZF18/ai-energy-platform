using EnergyPlatform.Domain.AnalysisRuns;

namespace EnergyPlatform.Application.Analysis;

public static class AnalysisRunMappings
{
    private static readonly Dictionary<AnalysisStage, string> Labels = new()
    {
        [AnalysisStage.Readings] = "Lecturas",
        [AnalysisStage.Baseline] = "Consumo esperado",
        [AnalysisStage.Detection] = "Detección",
        [AnalysisStage.Correlation] = "Correlación",
        [AnalysisStage.Events] = "Eventos",
        [AnalysisStage.Explanation] = "Explicación",
        [AnalysisStage.Recommendation] = "Recomendación"
    };

    public static AnalysisRunResponse ToResponse(this AnalysisRun run) => new(
        run.Id,
        run.Status,
        run.CurrentStage,
        [.. Enum.GetValues<AnalysisStage>().Select(stage => ProgressOf(run, stage))],
        run.RequestedAt,
        run.StartedAt,
        run.FinishedAt,
        run.Summary,
        run.Summary is { } summary ? HeadlineFor(summary) : null,
        run.Error);

    public static string HeadlineFor(AnalysisSummary summary) =>
        $"{summary.Cases} anomalías detectadas · {summary.PriorityCases} requieren atención prioritaria";

    private static StageProgressResponse ProgressOf(AnalysisRun run, AnalysisStage stage)
    {
        var progress = run.Stages.FirstOrDefault(item => item.Stage == stage);
        var state = progress switch
        {
            null => StageState.Pending,
            { FinishedAt: null } => StageState.Running,
            _ => StageState.Done
        };

        return new StageProgressResponse(stage, Labels[stage], state, (progress?.FinishedAt - progress?.StartedAt)?.TotalSeconds);
    }
}
