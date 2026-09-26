using EnergyPlatform.Domain.Events;
using EnergyPlatform.Domain.Meters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnergyPlatform.Infrastructure.Persistence.Configurations;

public sealed class MeterEventConfiguration : IEntityTypeConfiguration<MeterEvent>
{
    public void Configure(EntityTypeBuilder<MeterEvent> builder)
    {
        builder.ToTable("events");
        builder.HasKey(meterEvent => meterEvent.Id);
        builder.Property(meterEvent => meterEvent.Id).ValueGeneratedNever();
        builder.Property(meterEvent => meterEvent.MeterId).HasMaxLength(20);
        builder.Property(meterEvent => meterEvent.Timestamp).HasColumnType("timestamp without time zone");
        builder.Property(meterEvent => meterEvent.Description).HasMaxLength(500);
        builder.HasIndex(meterEvent => new { meterEvent.MeterId, meterEvent.Timestamp });
        builder.HasOne<Meter>().WithMany().HasForeignKey(meterEvent => meterEvent.MeterId).HasPrincipalKey(meter => meter.MeterId);
        builder.Ignore(meterEvent => meterEvent.DeclaredDuration);
    }
}
