using EnergyPlatform.Api.Endpoints;
using EnergyPlatform.Infrastructure.Persistence;
using Scalar.AspNetCore;

namespace EnergyPlatform.Api.Configuration;

public static class ApiPipeline
{
    public static WebApplication UseApiPipeline(this WebApplication app)
    {
        app.UseExceptionHandler();
        app.UseStatusCodePages();
        app.UseCors(ApiServices.FrontendCorsPolicy);
        app.UseAuthentication();
        app.UseAuthorization();
        return app;
    }

    public static WebApplication MapApiEndpoints(this WebApplication app)
    {
        app.MapOpenApi();
        app.MapScalarApiReference("/docs", options => options.WithTitle("Vatio · AI Energy Management API"));

        app.MapHealthEndpoints();
        app.MapAuthEndpoints();

        var secured = app.MapGroup(string.Empty).RequireAuthorization();
        secured.MapDashboardEndpoints();
        secured.MapMeterEndpoints();
        secured.MapAnomalyEndpoints();
        secured.MapAnalysisEndpoints();
        return app;
    }

    public static async Task PrepareDatabaseAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<DatabaseInitializer>().InitializeAsync(app.Lifetime.ApplicationStopping);
    }
}
