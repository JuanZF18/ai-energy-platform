namespace EnergyPlatform.Infrastructure.Seeding;

public sealed record CsvLoadResult<T>(IReadOnlyList<T> Rows, IReadOnlyList<int> RejectedLines);

public sealed record MeterCatalogEntry(string MeterId, string Name, string Location);
