using EnergyPlatform.Domain.AnalysisRuns;
using EnergyPlatform.Domain.Anomalies;
using EnergyPlatform.Domain.Meters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnergyPlatform.Infrastructure.Persistence.Configurations;

public sealed class AnomalyConfiguration : IEntityTypeConfiguration<Anomaly>
{
    public void Configure(EntityTypeBuilder<Anomaly> builder)
    {
        builder.HasKey(anomaly => anomaly.Id);
        builder.Property(anomaly => anomaly.Id).ValueGeneratedNever();
        builder.Property(anomaly => anomaly.Fingerprint).HasMaxLength(80);
        builder.Property(anomaly => anomaly.MeterId).HasMaxLength(20);
        builder.Property(anomaly => anomaly.Reason).HasMaxLength(500);
        builder.Property(anomaly => anomaly.RecommendedAction).HasMaxLength(300);
        builder.Property(anomaly => anomaly.EvidenceHash).HasMaxLength(64);
        builder.Property(anomaly => anomaly.Evidence).StoredAsJson();
        builder.Property(anomaly => anomaly.Explanation).StoredAsJson();
        builder.HasIndex(anomaly => anomaly.Fingerprint).IsUnique();
        builder.HasIndex(anomaly => new { anomaly.AnalysisRunId, anomaly.Priority });
        builder.HasOne<Meter>().WithMany().HasForeignKey(anomaly => anomaly.MeterId).HasPrincipalKey(meter => meter.MeterId);
        builder.HasOne<AnalysisRun>().WithMany().HasForeignKey(anomaly => anomaly.AnalysisRunId);
        builder.HasMany(anomaly => anomaly.StatusChanges).WithOne().HasForeignKey(change => change.AnomalyId);
        builder.Navigation(anomaly => anomaly.StatusChanges).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Ignore(anomaly => anomaly.IsActive);
    }
}
