using EnergyPlatform.Domain.Anomalies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnergyPlatform.Infrastructure.Persistence.Configurations;

public sealed class AnomalyStatusChangeConfiguration : IEntityTypeConfiguration<AnomalyStatusChange>
{
    public void Configure(EntityTypeBuilder<AnomalyStatusChange> builder)
    {
        builder.HasKey(change => change.Id);
        builder.Property(change => change.Id).ValueGeneratedNever();
        builder.Property(change => change.Note).HasMaxLength(500);
        builder.Property(change => change.ChangedBy).HasMaxLength(120);
    }
}
