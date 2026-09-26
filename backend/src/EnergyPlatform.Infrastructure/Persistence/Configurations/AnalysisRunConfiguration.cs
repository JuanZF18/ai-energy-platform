using EnergyPlatform.Domain.AnalysisRuns;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnergyPlatform.Infrastructure.Persistence.Configurations;

public sealed class AnalysisRunConfiguration : IEntityTypeConfiguration<AnalysisRun>
{
    public void Configure(EntityTypeBuilder<AnalysisRun> builder)
    {
        builder.HasKey(run => run.Id);
        builder.Property(run => run.Id).ValueGeneratedNever();
        builder.Property(run => run.Stages).StoredAsJson();
        builder.Property(run => run.Summary).StoredAsJson();
        builder.Property(run => run.Error).HasMaxLength(500);
        builder.HasIndex(run => new { run.Status, run.RequestedAt });
        builder.Ignore(run => run.IsInProgress);
    }
}
