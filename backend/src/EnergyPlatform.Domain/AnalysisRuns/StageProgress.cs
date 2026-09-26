namespace EnergyPlatform.Domain.AnalysisRuns;

public sealed record StageProgress(AnalysisStage Stage, DateTimeOffset StartedAt, DateTimeOffset? FinishedAt);
