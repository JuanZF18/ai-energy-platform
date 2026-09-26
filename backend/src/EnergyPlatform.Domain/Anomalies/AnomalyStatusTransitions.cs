namespace EnergyPlatform.Domain.Anomalies;

public static class AnomalyStatusTransitions
{
    private static readonly Dictionary<AnomalyStatus, AnomalyStatus[]> Allowed = new()
    {
        [AnomalyStatus.Open] = [AnomalyStatus.Investigating, AnomalyStatus.Resolved, AnomalyStatus.Dismissed],
        [AnomalyStatus.Investigating] = [AnomalyStatus.Open, AnomalyStatus.Resolved, AnomalyStatus.Dismissed],
        [AnomalyStatus.Resolved] = [AnomalyStatus.Open],
        [AnomalyStatus.Dismissed] = [AnomalyStatus.Open]
    };

    private static readonly Dictionary<AnomalyStatus, string> Names = new()
    {
        [AnomalyStatus.Open] = "Abierta",
        [AnomalyStatus.Investigating] = "En investigación",
        [AnomalyStatus.Resolved] = "Resuelta",
        [AnomalyStatus.Dismissed] = "Descartada"
    };

    public static bool IsAllowed(AnomalyStatus from, AnomalyStatus to) => Allowed[from].Contains(to);

    public static string NameOf(AnomalyStatus status) => Names[status];
}
