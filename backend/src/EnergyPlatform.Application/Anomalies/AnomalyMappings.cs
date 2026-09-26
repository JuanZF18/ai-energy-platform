using EnergyPlatform.Application.Common;
using EnergyPlatform.Domain.Anomalies;

namespace EnergyPlatform.Application.Anomalies;

public static class AnomalyMappings
{
    public static string ConfidenceLabel(double confidence) => confidence switch
    {
        >= 0.85 => "Alta",
        >= 0.6 => "Media",
        _ => "Baja"
    };

    public static AnomalyListItemResponse ToListItem(this Anomaly anomaly, int rank, string meterName) => new(
        anomaly.Id,
        rank,
        anomaly.MeterId,
        meterName,
        anomaly.Type != AnomalyType.FalsePositive,
        anomaly.Type,
        anomaly.Severity,
        anomaly.Confidence,
        ConfidenceLabel(anomaly.Confidence),
        anomaly.Priority,
        anomaly.Reason,
        anomaly.RecommendedAction,
        anomaly.Status,
        anomaly.DetectedAt);

    public static AnomalyDetailResponse ToDetail(this Anomaly anomaly, int rank, string meterName, string meterLocation, PlantTime plantTime) => new(
        anomaly.ToListItem(rank, meterName),
        meterLocation,
        anomaly.Evidence.ToResponse(plantTime),
        anomaly.Explanation is { } explanation
            ? new ExplanationResponse(explanation.Summary, explanation.WhatChanged, explanation.PossibleCauses, explanation.NextSteps, anomaly.ExplanationSource ?? ExplanationSource.Template)
            : null,
        [.. anomaly.StatusChanges
            .OrderBy(change => change.ChangedAt)
            .Select(change => new StatusChangeResponse(change.FromStatus, change.ToStatus, change.Note, change.ChangedBy, change.ChangedAt))]);

    private static EvidenceResponse ToResponse(this AnomalyEvidence evidence, PlantTime plantTime) => new(
        new EvidenceWindowResponse(
            plantTime.ToOffset(evidence.Window.Start),
            plantTime.ToOffset(evidence.Window.End),
            evidence.Window.Hours,
            evidence.Window.IsOngoing),
        evidence.Consumption,
        evidence.ChangedVariables,
        [.. evidence.Events.Select(item => new EventEvidenceResponse(item.Type, plantTime.ToOffset(item.Timestamp), item.Description, item.ExplainsChange))],
        evidence.DataQuality,
        evidence.Confidence,
        evidence.Facts);
}
