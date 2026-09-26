namespace EnergyPlatform.Domain.Anomalies;

public sealed record Finding(
    string MeterId,
    string Fingerprint,
    AnomalyType Type,
    Severity Severity,
    double Confidence,
    double Priority,
    string Reason,
    string RecommendedAction,
    AnomalyEvidence Evidence)
{
    public bool IsPriority => Severity == Severity.High && Type is AnomalyType.RealAnomaly or AnomalyType.DataQuality;
}
