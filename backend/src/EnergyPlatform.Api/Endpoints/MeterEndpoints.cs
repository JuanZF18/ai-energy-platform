using EnergyPlatform.Application.Meters;
using Microsoft.AspNetCore.Http.HttpResults;

namespace EnergyPlatform.Api.Endpoints;

public static class MeterEndpoints
{
    public static void MapMeterEndpoints(this IEndpointRouteBuilder app)
    {
        var meters = app.MapGroup("/api/meters").WithTags("Medidores");

        meters.MapGet("/", ListMetersAsync)
            .WithName("ListMeters")
            .WithSummary("Lista de medidores con consumo, variación, estado y anomalía")
            .WithDescription("Filtros: status = all | normal | alert | critical. Búsqueda por meter_id con search. Orden: sortBy = severity | consumption | variation | meterId y direction = descending | ascending.");

        meters.MapGet("/{meterId}", GetMeterAsync)
            .WithName("GetMeter")
            .WithSummary("Detalle de un medidor: consumo actual, baseline, variación, estado, anomalías y eventos");

        meters.MapGet("/{meterId}/readings", GetReadingsAsync)
            .WithName("GetMeterReadings")
            .WithSummary("Serie de lecturas con consumo, voltaje, corriente, factor de potencia y baseline esperado")
            .WithDescription("granularity = hour | day. from y to son opcionales y usan fecha con zona horaria.");
    }

    private static async Task<Results<Ok<IReadOnlyList<MeterListItemResponse>>, ValidationProblem>> ListMetersAsync(
        IMeterService meterService,
        CancellationToken cancellationToken,
        string? status = null,
        string? search = null,
        string? sortBy = null,
        string? direction = null)
    {
        if (!QueryValue.TryParse(status, MeterStatusFilter.All, out var statusFilter))
        {
            return TypedResults.ValidationProblem(QueryValue.InvalidValue<MeterStatusFilter>(nameof(status)));
        }

        if (!QueryValue.TryParse(sortBy, MeterSortField.Severity, out var sortField))
        {
            return TypedResults.ValidationProblem(QueryValue.InvalidValue<MeterSortField>(nameof(sortBy)));
        }

        if (!QueryValue.TryParse(direction, SortDirection.Descending, out var sortDirection))
        {
            return TypedResults.ValidationProblem(QueryValue.InvalidValue<SortDirection>(nameof(direction)));
        }

        var query = new MeterListQuery(statusFilter, search, sortField, sortDirection);
        return TypedResults.Ok(await meterService.ListAsync(query, cancellationToken));
    }

    private static async Task<Results<Ok<MeterDetailResponse>, NotFound>> GetMeterAsync(
        string meterId,
        IMeterService meterService,
        CancellationToken cancellationToken) =>
        await meterService.GetAsync(meterId, cancellationToken) is { } meter
            ? TypedResults.Ok(meter)
            : TypedResults.NotFound();

    private static async Task<Results<Ok<IReadOnlyList<ReadingPointResponse>>, NotFound, ValidationProblem>> GetReadingsAsync(
        string meterId,
        IMeterService meterService,
        CancellationToken cancellationToken,
        DateTimeOffset? from = null,
        DateTimeOffset? to = null,
        string? granularity = null)
    {
        if (!QueryValue.TryParse(granularity, ReadingGranularity.Hour, out var readingGranularity))
        {
            return TypedResults.ValidationProblem(QueryValue.InvalidValue<ReadingGranularity>(nameof(granularity)));
        }

        var readings = await meterService.GetReadingsAsync(meterId, new ReadingsQuery(from, to, readingGranularity), cancellationToken);
        return readings is null ? TypedResults.NotFound() : TypedResults.Ok(readings);
    }
}
