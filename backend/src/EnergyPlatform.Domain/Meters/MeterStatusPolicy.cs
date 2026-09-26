using EnergyPlatform.Domain.Anomalies;

namespace EnergyPlatform.Domain.Meters;

public static class MeterStatusPolicy
{
    public static MeterStatus From(IEnumerable<Anomaly> anomalies)
    {
        var active = anomalies.Where(anomaly => anomaly.IsActive).ToList();

        if (active.Any(anomaly => anomaly.Type == AnomalyType.RealAnomaly && anomaly.Severity == Severity.High))
        {
            return MeterStatus.Critical;
        }

        return active.Any(anomaly => anomaly.Type != AnomalyType.FalsePositive)
            ? MeterStatus.Alert
            : MeterStatus.Ok;
    }
}
