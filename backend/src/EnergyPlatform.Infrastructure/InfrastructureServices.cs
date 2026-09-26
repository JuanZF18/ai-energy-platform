using Anthropic;
using EnergyPlatform.Application.Analysis;
using EnergyPlatform.Application.Anomalies;
using EnergyPlatform.Application.Dashboard;
using EnergyPlatform.Application.Explanations;
using EnergyPlatform.Application.Meters;
using EnergyPlatform.Domain.Analysis;
using EnergyPlatform.Infrastructure.Analysis;
using EnergyPlatform.Infrastructure.Explanations;
using EnergyPlatform.Infrastructure.Persistence;
using EnergyPlatform.Infrastructure.Seeding;
using EnergyPlatform.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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
        AddExplanationWriter(services, configuration);

        services.AddScoped<ChallengeDataSeeder>();
        services.AddScoped<DatabaseInitializer>();
        return services;
    }

    private static void AddExplanationWriter(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<TemplateExplanationWriter>();
        var anthropic = configuration.GetSection(AnthropicOptions.SectionName).Get<AnthropicOptions>() ?? new AnthropicOptions();
        if (!anthropic.IsConfigured)
        {
            services.AddScoped<IExplanationWriter>(provider => provider.GetRequiredService<TemplateExplanationWriter>());
            return;
        }

        services.AddOptions<AnthropicOptions>().BindConfiguration(AnthropicOptions.SectionName);
        services.AddSingleton(provider =>
        {
            var options = provider.GetRequiredService<IOptions<AnthropicOptions>>().Value;
            return new AnthropicClient { ApiKey = options.ApiKey, Timeout = options.Timeout, MaxRetries = 1 };
        });
        services.AddScoped<IExplanationWriter, ClaudeExplanationWriter>();
    }
}
