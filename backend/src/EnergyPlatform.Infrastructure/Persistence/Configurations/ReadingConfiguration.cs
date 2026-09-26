using EnergyPlatform.Domain.Meters;
using EnergyPlatform.Domain.Readings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnergyPlatform.Infrastructure.Persistence.Configurations;

public sealed class ReadingConfiguration : IEntityTypeConfiguration<Reading>
{
    public void Configure(EntityTypeBuilder<Reading> builder)
    {
        builder.HasKey(reading => reading.Id);
        builder.Property(reading => reading.MeterId).HasMaxLength(20);
        builder.Property(reading => reading.Timestamp).HasColumnType("timestamp without time zone");
        builder.HasIndex(reading => new { reading.MeterId, reading.Timestamp }).IsUnique();
        builder.HasOne<Meter>().WithMany().HasForeignKey(reading => reading.MeterId).HasPrincipalKey(meter => meter.MeterId);
        builder.Ignore(reading => reading.HourOfDay);
        builder.Ignore(reading => reading.ElectricalRatio);
    }
}
