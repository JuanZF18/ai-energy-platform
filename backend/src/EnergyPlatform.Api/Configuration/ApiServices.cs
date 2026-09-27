using System.Text.Json;
using System.Text.Json.Serialization;
using EnergyPlatform.Api.Authentication;
using EnergyPlatform.Api.Middleware;
using EnergyPlatform.Api.Workers;

namespace EnergyPlatform.Api.Configuration;

public static class ApiServices
{
    public const string FrontendCorsPolicy = "Frontend";

    public static IServiceCollection AddApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseUpper));
            options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
        });

        services.AddOpenApi();
        services.AddValidation();
        services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
        {
            if (context.ProblemDetails.Status == StatusCodes.Status400BadRequest && context.ProblemDetails.Title?.StartsWith("One or more") == true)
            {
                context.ProblemDetails.Title = "La petición tiene datos inválidos";
            }
        });
        services.AddExceptionHandler<ApiExceptionHandler>();
        services.AddHostedService<AnalysisWorker>();
        services.AddApiAuthentication(configuration);

        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        services.AddCors(options => options.AddPolicy(FrontendCorsPolicy, policy =>
            policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

        return services;
    }
}
