namespace EnergyPlatform.Application.Analysis;

public interface IAnalysisService
{
    Task<AnalysisRequestResult> RequestAsync(CancellationToken cancellationToken);

    Task<AnalysisRunResponse?> GetAsync(Guid id, CancellationToken cancellationToken);
}

public interface IAnalysisQueue
{
    ValueTask QueueAsync(Guid analysisRunId, CancellationToken cancellationToken);

    IAsyncEnumerable<Guid> DequeueAllAsync(CancellationToken cancellationToken);
}

public interface IAnalysisRunner
{
    Task RunAsync(Guid analysisRunId, CancellationToken cancellationToken);
}
