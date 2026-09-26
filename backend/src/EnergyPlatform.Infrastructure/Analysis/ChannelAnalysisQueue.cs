using System.Threading.Channels;
using EnergyPlatform.Application.Analysis;

namespace EnergyPlatform.Infrastructure.Analysis;

public sealed class ChannelAnalysisQueue : IAnalysisQueue
{
    private readonly Channel<Guid> pendingRuns = Channel.CreateUnbounded<Guid>(new UnboundedChannelOptions { SingleReader = true });

    public ValueTask QueueAsync(Guid analysisRunId, CancellationToken cancellationToken) =>
        pendingRuns.Writer.WriteAsync(analysisRunId, cancellationToken);

    public IAsyncEnumerable<Guid> DequeueAllAsync(CancellationToken cancellationToken) =>
        pendingRuns.Reader.ReadAllAsync(cancellationToken);
}
