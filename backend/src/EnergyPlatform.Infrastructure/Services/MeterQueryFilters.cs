using EnergyPlatform.Application.Meters;
using EnergyPlatform.Domain.Meters;
using Microsoft.EntityFrameworkCore;

namespace EnergyPlatform.Infrastructure.Services;

public static class MeterQueryFilters
{
    public static IQueryable<Meter> WithStatus(this IQueryable<Meter> meters, MeterStatusFilter filter) => filter switch
    {
        MeterStatusFilter.Normal => meters.Where(meter => meter.Status == MeterStatus.Ok),
        MeterStatusFilter.Alert => meters.Where(meter => meter.Status == MeterStatus.Alert),
        MeterStatusFilter.Critical => meters.Where(meter => meter.Status == MeterStatus.Critical),
        _ => meters
    };

    public static IQueryable<Meter> MatchingId(this IQueryable<Meter> meters, string? search) =>
        string.IsNullOrWhiteSpace(search)
            ? meters
            : meters.Where(meter => EF.Functions.ILike(meter.MeterId, $"%{search.Trim()}%"));

    public static IQueryable<Meter> SortedBy(this IQueryable<Meter> meters, MeterSortField field, SortDirection direction)
    {
        var ascending = direction == SortDirection.Ascending;

        return field switch
        {
            MeterSortField.Consumption => ascending
                ? meters.OrderBy(meter => meter.CurrentDailyKwh)
                : meters.OrderByDescending(meter => meter.CurrentDailyKwh),
            MeterSortField.Variation => ascending
                ? meters.OrderBy(meter => meter.VariationPercent)
                : meters.OrderByDescending(meter => meter.VariationPercent),
            MeterSortField.MeterId => ascending
                ? meters.OrderBy(meter => meter.MeterId)
                : meters.OrderByDescending(meter => meter.MeterId),
            _ => ascending
                ? meters.OrderBy(SeverityRank).ThenBy(meter => meter.MeterId)
                : meters.OrderByDescending(SeverityRank).ThenBy(meter => meter.MeterId)
        };
    }

    public static int SeverityRankOf(MeterStatus status) => status switch
    {
        MeterStatus.Critical => 3,
        MeterStatus.Alert => 2,
        MeterStatus.Ok => 1,
        _ => 0
    };

    private static readonly System.Linq.Expressions.Expression<Func<Meter, int>> SeverityRank = meter =>
        meter.Status == MeterStatus.Critical ? 3
        : meter.Status == MeterStatus.Alert ? 2
        : meter.Status == MeterStatus.Ok ? 1
        : 0;
}
