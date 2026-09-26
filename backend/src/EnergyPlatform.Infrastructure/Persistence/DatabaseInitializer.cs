using EnergyPlatform.Domain.AnalysisRuns;
using EnergyPlatform.Infrastructure.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EnergyPlatform.Infrastructure.Persistence;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    public bool ApplyMigrationsOnStartup { get; set; } = true;
    public bool SeedOnStartup { get; set; } = true;
}

public sealed class DatabaseInitializer(
    EnergyDbContext db,
    ChallengeDataSeeder seeder,
    IOptions<DatabaseOptions> options,
    TimeProvider clock,
    ILogger<DatabaseInitializer> logger)
{
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        if (options.Value.ApplyMigrationsOnStartup)
        {
            await db.Database.MigrateAsync(cancellationToken);
        }

        if (options.Value.SeedOnStartup)
        {
            await seeder.SeedAsync(cancellationToken);
        }

        await CloseInterruptedRunsAsync(cancellationToken);
    }

    private async Task CloseInterruptedRunsAsync(CancellationToken cancellationToken)
    {
        var interrupted = await db.AnalysisRuns
            .Where(run => run.Status == AnalysisRunStatus.Queued || run.Status == AnalysisRunStatus.Running)
            .ToListAsync(cancellationToken);

        foreach (var run in interrupted)
        {
            run.Fail("El análisis se interrumpió porque el servidor se reinició.", clock.GetUtcNow());
        }

        if (interrupted.Count > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
            logger.LogWarning("Se cerraron {Count} análisis que quedaron interrumpidos.", interrupted.Count);
        }
    }
}
