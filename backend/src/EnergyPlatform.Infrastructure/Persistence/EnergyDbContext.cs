using EnergyPlatform.Domain.AnalysisRuns;
using EnergyPlatform.Domain.Anomalies;
using EnergyPlatform.Domain.Events;
using EnergyPlatform.Domain.Meters;
using EnergyPlatform.Domain.Readings;
using Microsoft.EntityFrameworkCore;

namespace EnergyPlatform.Infrastructure.Persistence;

public sealed class EnergyDbContext(DbContextOptions<EnergyDbContext> options) : DbContext(options)
{
    public DbSet<Meter> Meters => Set<Meter>();
    public DbSet<Reading> Readings => Set<Reading>();
    public DbSet<MeterEvent> Events => Set<MeterEvent>();
    public DbSet<Anomaly> Anomalies => Set<Anomaly>();
    public DbSet<AnomalyStatusChange> AnomalyStatusChanges => Set<AnomalyStatusChange>();
    public DbSet<AnalysisRun> AnalysisRuns => Set<AnalysisRun>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EnergyDbContext).Assembly);

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.Properties<Enum>().HaveConversion<string>().HaveMaxLength(40);
}
