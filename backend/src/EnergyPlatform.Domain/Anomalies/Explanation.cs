namespace EnergyPlatform.Domain.Anomalies;

public sealed record Explanation(
    string Summary,
    IReadOnlyList<string> WhatChanged,
    IReadOnlyList<string> PossibleCauses,
    IReadOnlyList<string> NextSteps);
