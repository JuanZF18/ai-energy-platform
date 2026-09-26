using EnergyPlatform.Domain.Anomalies;

namespace EnergyPlatform.Application.Anomalies;

public sealed record AnomalyListItemResponse(
    Guid Id,
    int Rank,
    string MeterId,
    string MeterName,
    bool Anomaly,
    AnomalyType Type,
    Severity Severity,
    double Confidence,
    string ConfidenceLabel,
    double Priority,
    string Reason,
    string RecommendedAction,
    AnomalyStatus Status,
    DateTimeOffset DetectedAt);

public sealed record AnomalyDetailResponse(
    AnomalyListItemResponse Summary,
    string MeterLocation,
    EvidenceResponse Evidence,
    ExplanationResponse? Explanation,
    IReadOnlyList<StatusChangeResponse> History);

public sealed record EvidenceResponse(
    EvidenceWindowResponse Window,
    ConsumptionEvidence Consumption,
    IReadOnlyList<VariableChange> ChangedVariables,
    IReadOnlyList<EventEvidenceResponse> Events,
    DataQualityEvidence? DataQuality,
    ConfidenceBreakdown Confidence,
    IReadOnlyList<string> Facts);

public sealed record EvidenceWindowResponse(DateTimeOffset Start, DateTimeOffset End, int Hours, bool IsOngoing);

public sealed record EventEvidenceResponse(string Type, DateTimeOffset Timestamp, string Description, bool ExplainsChange);

public sealed record ExplanationResponse(
    string Summary,
    IReadOnlyList<string> WhatChanged,
    IReadOnlyList<string> PossibleCauses,
    IReadOnlyList<string> NextSteps,
    ExplanationSource Source);

public sealed record StatusChangeResponse(
    AnomalyStatus From,
    AnomalyStatus To,
    string? Note,
    string ChangedBy,
    DateTimeOffset ChangedAt);
