using EnergyPlatform.Application.Common;
using Microsoft.Extensions.DependencyInjection;

namespace EnergyPlatform.Application;

public static class ApplicationServices
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddOptions<PlantOptions>().BindConfiguration(PlantOptions.SectionName);
        services.AddSingleton<PlantTime>();
        return services;
    }
}
