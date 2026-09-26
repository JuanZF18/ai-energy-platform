using EnergyPlatform.Domain.Meters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnergyPlatform.Infrastructure.Persistence.Configurations;

public sealed class MeterConfiguration : IEntityTypeConfiguration<Meter>
{
    public void Configure(EntityTypeBuilder<Meter> builder)
    {
        builder.HasKey(meter => meter.Id);
        builder.Property(meter => meter.Id).ValueGeneratedNever();
        builder.HasAlternateKey(meter => meter.MeterId);
        builder.Property(meter => meter.MeterId).HasMaxLength(20);
        builder.Property(meter => meter.Name).HasMaxLength(120);
        builder.Property(meter => meter.Location).HasMaxLength(120);
        builder.HasIndex(meter => meter.Status);
    }
}
