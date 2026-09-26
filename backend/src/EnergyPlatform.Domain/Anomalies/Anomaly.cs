using EnergyPlatform.Domain.Common;

namespace EnergyPlatform.Domain.Anomalies;

public sealed class Anomaly
{
    private readonly List<AnomalyStatusChange> statusChanges = [];

    private Anomaly()
    {
    }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Fingerprint { get; private set; } = string.Empty;
    public string MeterId { get; private set; } = string.Empty;
    public Guid AnalysisRunId { get; private set; }
    public DateTimeOffset DetectedAt { get; private set; }
    public DateTimeOffset LastAnalyzedAt { get; private set; }
    public AnomalyType Type { get; private set; }
    public Severity Severity { get; private set; }
    public double Confidence { get; private set; }
    public double Priority { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public string RecommendedAction { get; private set; } = string.Empty;
    public AnomalyStatus Status { get; private set; }
    public AnomalyEvidence Evidence { get; private set; } = null!;
    public string EvidenceHash { get; private set; } = string.Empty;
    public Explanation? Explanation { get; private set; }
    public ExplanationSource? ExplanationSource { get; private set; }
    public IReadOnlyList<AnomalyStatusChange> StatusChanges => statusChanges;

    public bool IsActive => Status is AnomalyStatus.Open or AnomalyStatus.Investigating;
    public bool NeedsExplanationFrom(ExplanationSource preferredSource) => Explanation is null || ExplanationSource != preferredSource;

    public static Anomaly Detect(Finding finding, string evidenceHash, Guid analysisRunId, DateTimeOffset detectedAt)
    {
        var anomaly = new Anomaly
        {
            Fingerprint = finding.Fingerprint,
            MeterId = finding.MeterId,
            DetectedAt = detectedAt,
            Status = finding.Type == AnomalyType.FalsePositive ? AnomalyStatus.Dismissed : AnomalyStatus.Open
        };

        anomaly.Refresh(finding, evidenceHash, analysisRunId, detectedAt);
        return anomaly;
    }

    public void Refresh(Finding finding, string evidenceHash, Guid analysisRunId, DateTimeOffset analyzedAt)
    {
        if (EvidenceHash != evidenceHash)
        {
            Explanation = null;
            ExplanationSource = null;
        }

        AnalysisRunId = analysisRunId;
        LastAnalyzedAt = analyzedAt;
        Type = finding.Type;
        Severity = finding.Severity;
        Confidence = finding.Confidence;
        Priority = finding.Priority;
        Reason = finding.Reason;
        RecommendedAction = finding.RecommendedAction;
        Evidence = finding.Evidence;
        EvidenceHash = evidenceHash;
    }

    public void AttachExplanation(Explanation explanation, ExplanationSource source)
    {
        Explanation = explanation;
        ExplanationSource = source;
    }

    public void ChangeStatus(AnomalyStatus newStatus, string? note, string changedBy, DateTimeOffset changedAt)
    {
        if (!AnomalyStatusTransitions.IsAllowed(Status, newStatus))
        {
            throw new DomainRuleException(
                $"No se puede pasar una anomalía de \"{AnomalyStatusTransitions.NameOf(Status)}\" a \"{AnomalyStatusTransitions.NameOf(newStatus)}\".");
        }

        statusChanges.Add(new AnomalyStatusChange(Id, Status, newStatus, note, changedBy, changedAt));
        Status = newStatus;
    }
}
