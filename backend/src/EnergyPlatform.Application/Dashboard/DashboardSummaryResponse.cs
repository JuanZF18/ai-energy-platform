using EnergyPlatform.Application.Meters;
using EnergyPlatform.Domain.AnalysisRuns;

namespace EnergyPlatform.Application.Dashboard;

public sealed record DashboardSummaryResponse(
    MeterCountsResponse Meters,
    ConsumptionTotalsResponse Consumption,
    AnomalyCountsResponse Anomalies,
    double? AggregateConfidence,
    LastAnalysisResponse? LastAnalysis,
    IReadOnlyList<DailyConsumptionResponse> DailyConsumption,
    IReadOnlyList<EventResponse> Events);

public sealed record MeterCountsResponse(int Total, int Ok, int Alert, int Critical, int NotAnalyzed);

public sealed record ConsumptionTotalsResponse(
    double PeriodKwh,
    double LastDayKwh,
    DateTimeOffset? PeriodStart,
    DateTimeOffset? PeriodEnd);

public sealed record AnomalyCountsResponse(int Cases, int Anomalies, int Dismissed, int HighPriority);

public sealed record LastAnalysisResponse(
    Guid Id,
    AnalysisRunStatus Status,
    DateTimeOffset RequestedAt,
    DateTimeOffset? FinishedAt,
    double? DurationSeconds);

public sealed record DailyConsumptionResponse(DateOnly Day, double ConsumptionKwh);
