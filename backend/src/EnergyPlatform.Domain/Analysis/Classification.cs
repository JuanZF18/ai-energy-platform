using EnergyPlatform.Domain.Anomalies;

namespace EnergyPlatform.Domain.Analysis;

public sealed record Classification(AnomalyType Type, Severity Severity, IReadOnlyList<bool> SupportingChecks)
{
    public double Corroboration => SupportingChecks.Count == 0
        ? 0
        : SupportingChecks.Count(check => check) / (double)SupportingChecks.Count;
}
