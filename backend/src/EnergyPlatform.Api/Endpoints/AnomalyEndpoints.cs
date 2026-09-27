using System.Security.Claims;
using EnergyPlatform.Application.Anomalies;
using EnergyPlatform.Domain.Anomalies;
using Microsoft.AspNetCore.Http.HttpResults;

namespace EnergyPlatform.Api.Endpoints;

public static class AnomalyEndpoints
{
    private const string DefaultOperator = "operador-demo";

    public static void MapAnomalyEndpoints(this IEndpointRouteBuilder app)
    {
        var anomalies = app.MapGroup("/api/anomalies").WithTags("Anomalías IA");

        anomalies.MapGet("/", ListAnomaliesAsync)
            .WithName("ListAnomalies")
            .WithSummary("Anomalías del último análisis, ordenadas por prioridad")
            .WithDescription("Filtros opcionales: type = REAL_ANOMALY | DATA_QUALITY | EXPLAINABLE_ANOMALY | FALSE_POSITIVE, status = OPEN | INVESTIGATING | RESOLVED | DISMISSED y limit.");

        anomalies.MapGet("/{id:guid}", GetAnomalyAsync)
            .WithName("GetAnomaly")
            .WithSummary("Investigación: qué encontró la IA, variables que cambiaron, eventos, confianza, acción y evidencia");

        anomalies.MapPatch("/{id:guid}", ChangeStatusAsync)
            .WithName("ChangeAnomalyStatus")
            .WithSummary("Registra la acción del operador cambiando el estado de la anomalía")
            .WithDescription("Transiciones permitidas: OPEN → INVESTIGATING | RESOLVED | DISMISSED, INVESTIGATING → OPEN | RESOLVED | DISMISSED, RESOLVED → OPEN y DISMISSED → OPEN.");
    }

    private static async Task<Results<Ok<IReadOnlyList<AnomalyListItemResponse>>, ValidationProblem>> ListAnomaliesAsync(
        IAnomalyService anomalyService,
        CancellationToken cancellationToken,
        string? type = null,
        string? status = null,
        int? limit = null)
    {
        if (!QueryValue.TryParseOptional<AnomalyType>(type, out var typeFilter))
        {
            return TypedResults.ValidationProblem(QueryValue.InvalidValue<AnomalyType>(nameof(type)));
        }

        if (!QueryValue.TryParseOptional<AnomalyStatus>(status, out var statusFilter))
        {
            return TypedResults.ValidationProblem(QueryValue.InvalidValue<AnomalyStatus>(nameof(status)));
        }

        if (limit is < 1 or > 100)
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]> { [nameof(limit)] = ["Debe estar entre 1 y 100."] });
        }

        return TypedResults.Ok(await anomalyService.ListAsync(new AnomalyListQuery(typeFilter, statusFilter, limit), cancellationToken));
    }

    private static async Task<Results<Ok<AnomalyDetailResponse>, NotFound>> GetAnomalyAsync(
        Guid id,
        IAnomalyService anomalyService,
        CancellationToken cancellationToken) =>
        await anomalyService.GetAsync(id, cancellationToken) is { } anomaly
            ? TypedResults.Ok(anomaly)
            : TypedResults.NotFound();

    private static async Task<Results<Ok<AnomalyDetailResponse>, NotFound>> ChangeStatusAsync(
        Guid id,
        UpdateAnomalyStatusRequest request,
        IAnomalyService anomalyService,
        ClaimsPrincipal user,
        CancellationToken cancellationToken) =>
        await anomalyService.ChangeStatusAsync(id, request, user.Identity?.Name ?? DefaultOperator, cancellationToken) is { } anomaly
            ? TypedResults.Ok(anomaly)
            : TypedResults.NotFound();
}
