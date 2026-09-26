using EnergyPlatform.Application.Anomalies;
using EnergyPlatform.Domain.Anomalies;
using EnergyPlatform.Domain.Meters;

namespace EnergyPlatform.Application.Meters;

public sealed record MeterListItemResponse(
    string MeterId,
    string Name,
    string Location,
    MeterStatus Status,
    double CurrentDailyKwh,
    double? BaselineDailyKwh,
    double? VariationPercent,
    AnomalyBadgeResponse? Anomaly,
    IReadOnlyList<double> DailyConsumption);

public sealed record AnomalyBadgeResponse(Guid Id, AnomalyType Type, Severity Severity, AnomalyStatus Status, double Priority);

public sealed record MeterDetailResponse(
    string MeterId,
    string Name,
    string Location,
    MeterStatus Status,
    double CurrentDailyKwh,
    double? BaselineDailyKwh,
    double? VariationPercent,
    IReadOnlyList<double> HourlyBaseline,
    int ReadingsCount,
    DateTimeOffset? FirstReadingAt,
    DateTimeOffset? LastReadingAt,
    IReadOnlyList<AnomalyListItemResponse> Anomalies,
    IReadOnlyList<EventResponse> Events);

public sealed record ReadingPointResponse(
    DateTimeOffset Timestamp,
    double ConsumptionKwh,
    double VoltageV,
    double CurrentA,
    double PowerFactor,
    double? ExpectedKwh,
    bool IsSuspect);

public sealed record EventResponse(string MeterId, DateTimeOffset Timestamp, string Type, string Description);
