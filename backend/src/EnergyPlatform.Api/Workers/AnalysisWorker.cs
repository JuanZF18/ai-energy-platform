using EnergyPlatform.Application.Analysis;

namespace EnergyPlatform.Api.Workers;

public sealed class AnalysisWorker(IAnalysisQueue queue, IServiceScopeFactory scopeFactory, ILogger<AnalysisWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var analysisRunId in queue.DequeueAllAsync(stoppingToken))
        {
            logger.LogInformation("Empieza el análisis {AnalysisRunId}.", analysisRunId);

            await using var scope = scopeFactory.CreateAsyncScope();
            await scope.ServiceProvider.GetRequiredService<IAnalysisRunner>().RunAsync(analysisRunId, stoppingToken);
        }
    }
}
