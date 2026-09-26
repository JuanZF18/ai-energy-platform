using EnergyPlatform.Domain.AnalysisRuns;

namespace EnergyPlatform.Application.Analysis;

public enum StageState
{
    Pending,
    Running,
    Done
}

public sealed record AnalysisRequestResult(AnalysisRunResponse Run, bool WasAlreadyInProgress);

public sealed record AnalysisRunResponse(
    Guid Id,
    AnalysisRunStatus Status,
    AnalysisStage? CurrentStage,
    IReadOnlyList<StageProgressResponse> Stages,
    DateTimeOffset RequestedAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? FinishedAt,
    AnalysisSummary? Summary,
    string? Headline,
    string? Error);

public sealed record StageProgressResponse(AnalysisStage Stage, string Label, StageState State, double? DurationSeconds);
