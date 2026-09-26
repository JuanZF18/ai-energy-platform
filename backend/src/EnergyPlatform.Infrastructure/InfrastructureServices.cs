using EnergyPlatform.Application.Analysis;
using EnergyPlatform.Application.Anomalies;
using EnergyPlatform.Application.Dashboard;
using EnergyPlatform.Application.Explanations;
using EnergyPlatform.Application.Meters;
using EnergyPlatform.Domain.Analysis;
using EnergyPlatform.Infrastructure.Analysis;
using EnergyPlatform.Infrastructure.Persistence;
using EnergyPlatform.Infrastructure.Seeding;
using EnergyPlatform.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnergyPlatform.Infrastructure;

public static class InfrastructureServices
{
    public const string ConnectionStringName = "EnergyDatabase";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Falta la cadena de conexión 'ConnectionStrings:{ConnectionStringName}'. Configúrala con dotnet user-secrets o una variable de entorno.");
        }

        services.AddDbContext<EnergyDbContext>(options => options
            .UseNpgsql(PostgresConnectionString.Normalize(connectionString))
            .UseSnakeCaseNamingConvention());

        services.AddOptions<SeedOptions>().BindConfiguration(SeedOptions.SectionName);
        services.AddOptions<DatabaseOptions>().BindConfiguration(DatabaseOptions.SectionName);
        services.AddOptions<AnalysisPacingOptions>().BindConfiguration(AnalysisPacingOptions.SectionName);
        services.AddSingleton(configuration.GetSection("Analysis:Thresholds").Get<AnalysisOptions>() ?? AnalysisOptions.Default);
        services.AddSingleton(TimeProvider.System);

        services.AddScoped<IMeterService, MeterService>();
        services.AddScoped<ReadingSeries>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IAnomalyService, AnomalyService>();
        services.AddScoped<IAnalysisService, AnalysisService>();

        services.AddSingleton<IAnalysisQueue, ChannelAnalysisQueue>();
        services.AddScoped<IAnalysisRunner, AnalysisRunner>();
        services.AddScoped<AnalysisInputLoader>();
        services.AddScoped<AnalysisResultWriter>();
        services.AddScoped<IExplanationWriter, TemplateExplanationWriter>();

        services.AddScoped<ChallengeDataSeeder>();
        services.AddScoped<DatabaseInitializer>();
        return services;
    }
}
