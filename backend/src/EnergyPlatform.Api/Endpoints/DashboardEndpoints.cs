using EnergyPlatform.Application.Dashboard;

namespace EnergyPlatform.Api.Endpoints;

public static class DashboardEndpoints
{
    public static void MapDashboardEndpoints(this WebApplication app)
    {
        app.MapGet("/api/dashboard/summary", async (IDashboardService dashboard, CancellationToken cancellationToken) =>
                TypedResults.Ok(await dashboard.GetSummaryAsync(cancellationToken)))
            .WithName("GetDashboardSummary")
            .WithTags("Dashboard")
            .WithSummary("KPIs del dashboard")
            .WithDescription("Medidores por estado, consumo del período, anomalías, alta prioridad, confianza agregada, último análisis, consumo diario y eventos.");
    }
}
