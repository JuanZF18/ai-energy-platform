using EnergyPlatform.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http.HttpResults;

namespace EnergyPlatform.Api.Endpoints;

public static class HealthEndpoints
{
    public sealed record HealthResponse(string Status, bool DatabaseReachable);

    public static void MapHealthEndpoints(this WebApplication app)
    {
        app.MapGet("/health", async Task<Results<Ok<HealthResponse>, JsonHttpResult<HealthResponse>>> (EnergyDbContext db, CancellationToken cancellationToken) =>
            await db.Database.CanConnectAsync(cancellationToken)
                ? TypedResults.Ok(new HealthResponse("healthy", true))
                : TypedResults.Json(new HealthResponse("unhealthy", false), statusCode: StatusCodes.Status503ServiceUnavailable))
            .WithName("GetHealth")
            .WithTags("Salud")
            .WithSummary("Estado del servicio y de la base de datos");
    }
}
