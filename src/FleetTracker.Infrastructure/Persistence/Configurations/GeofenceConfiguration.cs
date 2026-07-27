using FleetTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FleetTracker.Infrastructure.Persistence.Configurations;

public class GeofenceConfiguration : IEntityTypeConfiguration<Geofence>
{
    public void Configure(EntityTypeBuilder<Geofence> builder)
    {
        builder.ToTable("Geofences");

        builder.HasKey(g => g.Id);

        // یک وسیله نقلیه نهایتاً یک Geofence دارد
        builder.HasIndex(g => g.VehicleId)
               .IsUnique();

        // رابطه‌ی یک‌به‌یک با Vehicle (navigation روی سمت Vehicle است)
        builder.HasOne<Vehicle>()
               .WithOne(v => v.Geofence)
               .HasForeignKey<Geofence>(g => g.VehicleId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
