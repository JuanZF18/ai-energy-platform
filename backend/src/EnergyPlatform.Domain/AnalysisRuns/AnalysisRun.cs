namespace EnergyPlatform.Domain.AnalysisRuns;

public sealed class AnalysisRun
{
    private AnalysisRun()
    {
    }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public AnalysisRunStatus Status { get; private set; }
    public AnalysisStage? CurrentStage { get; private set; }
    public DateTimeOffset RequestedAt { get; private set; }
    public DateTimeOffset? StartedAt { get; private set; }
    public DateTimeOffset? FinishedAt { get; private set; }
    public StageProgress[] Stages { get; private set; } = [];
    public AnalysisSummary? Summary { get; private set; }
    public string? Error { get; private set; }

    public bool IsInProgress => Status is AnalysisRunStatus.Queued or AnalysisRunStatus.Running;

    public static AnalysisRun Request(DateTimeOffset requestedAt) => new()
    {
        Status = AnalysisRunStatus.Queued,
        RequestedAt = requestedAt
    };

    public void Start(DateTimeOffset startedAt)
    {
        Status = AnalysisRunStatus.Running;
        StartedAt = startedAt;
    }

    public void BeginStage(AnalysisStage stage, DateTimeOffset startedAt)
    {
        CurrentStage = stage;
        Stages = [.. Stages, new StageProgress(stage, startedAt, null)];
    }

    public void FinishStage(AnalysisStage stage, DateTimeOffset finishedAt) =>
        Stages = [.. Stages.Select(progress => progress.Stage == stage ? progress with { FinishedAt = finishedAt } : progress)];

    public void Complete(AnalysisSummary summary, DateTimeOffset finishedAt)
    {
        Status = AnalysisRunStatus.Completed;
        CurrentStage = null;
        Summary = summary;
        FinishedAt = finishedAt;
    }

    public void Fail(string error, DateTimeOffset finishedAt)
    {
        Status = AnalysisRunStatus.Failed;
        Error = error;
        FinishedAt = finishedAt;
    }
}
