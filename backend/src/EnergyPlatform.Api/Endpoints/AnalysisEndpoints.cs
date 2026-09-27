using EnergyPlatform.Application.Analysis;
using Microsoft.AspNetCore.Http.HttpResults;

namespace EnergyPlatform.Api.Endpoints;

public static class AnalysisEndpoints
{
    public static void MapAnalysisEndpoints(this IEndpointRouteBuilder app)
    {
        var analysis = app.MapGroup("/api/ai").WithTags("Análisis IA");

        analysis.MapPost("/analyze", RequestAnalysisAsync)
            .WithName("RunAiAnalysis")
            .WithSummary("Run AI Analysis: inicia el análisis en segundo plano")
            .WithDescription("Responde 202 con el id del análisis. Si ya hay uno en curso, devuelve ese mismo.");

        analysis.MapGet("/analysis/{id:guid}", GetAnalysisAsync)
            .WithName("GetAiAnalysis")
            .WithSummary("Estado del análisis: etapa actual, progreso de las 7 etapas y resumen final");
    }

    private static async Task<Accepted<AnalysisRunResponse>> RequestAnalysisAsync(
        IAnalysisService analysisService,
        CancellationToken cancellationToken)
    {
        var result = await analysisService.RequestAsync(cancellationToken);
        return TypedResults.Accepted($"/api/ai/analysis/{result.Run.Id}", result.Run);
    }

    private static async Task<Results<Ok<AnalysisRunResponse>, NotFound>> GetAnalysisAsync(
        Guid id,
        IAnalysisService analysisService,
        CancellationToken cancellationToken) =>
        await analysisService.GetAsync(id, cancellationToken) is { } run
            ? TypedResults.Ok(run)
            : TypedResults.NotFound();
}
