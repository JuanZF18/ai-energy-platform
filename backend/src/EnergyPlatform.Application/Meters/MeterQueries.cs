namespace EnergyPlatform.Application.Meters;

public enum MeterStatusFilter
{
    All,
    Normal,
    Alert,
    Critical
}

public enum MeterSortField
{
    Severity,
    Consumption,
    Variation,
    MeterId
}

public enum SortDirection
{
    Descending,
    Ascending
}

public enum ReadingGranularity
{
    Hour,
    Day
}

public sealed record MeterListQuery(MeterStatusFilter Status, string? Search, MeterSortField SortBy, SortDirection Direction);

public sealed record ReadingsQuery(DateTimeOffset? From, DateTimeOffset? To, ReadingGranularity Granularity);
