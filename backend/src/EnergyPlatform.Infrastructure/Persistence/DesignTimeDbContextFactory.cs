using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EnergyPlatform.Infrastructure.Persistence;

public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<EnergyDbContext>
{
    public EnergyDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<EnergyDbContext>()
            .UseNpgsql("Host=localhost;Database=energy_platform_design")
            .UseSnakeCaseNamingConvention()
            .Options;

        return new EnergyDbContext(options);
    }
}
