using EnergyPlatform.Domain.Anomalies;

namespace EnergyPlatform.Application.Explanations;

public interface IExplanationWriter
{
    ExplanationSource PreferredSource { get; }

    Task<WrittenExplanation> WriteAsync(Finding finding, CancellationToken cancellationToken);
}

public sealed record WrittenExplanation(Explanation Explanation, ExplanationSource Source);
