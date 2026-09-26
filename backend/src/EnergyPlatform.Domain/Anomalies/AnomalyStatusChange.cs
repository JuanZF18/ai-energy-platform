namespace EnergyPlatform.Domain.Anomalies;

public sealed class AnomalyStatusChange
{
    private AnomalyStatusChange()
    {
    }

    public AnomalyStatusChange(Guid anomalyId, AnomalyStatus fromStatus, AnomalyStatus toStatus, string? note, string changedBy, DateTimeOffset changedAt)
    {
        AnomalyId = anomalyId;
        FromStatus = fromStatus;
        ToStatus = toStatus;
        Note = note;
        ChangedBy = changedBy;
        ChangedAt = changedAt;
    }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid AnomalyId { get; private set; }
    public AnomalyStatus FromStatus { get; private set; }
    public AnomalyStatus ToStatus { get; private set; }
    public string? Note { get; private set; }
    public string ChangedBy { get; private set; } = string.Empty;
    public DateTimeOffset ChangedAt { get; private set; }
}
