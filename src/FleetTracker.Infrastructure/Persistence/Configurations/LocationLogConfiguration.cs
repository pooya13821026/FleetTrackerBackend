using FleetTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FleetTracker.Infrastructure.Persistence.Configurations;

/// <summary>
/// پیکربندی جدول LocationLog — مهم‌ترین ایندکس پروژه.
/// بدون این ایندکس، کوئری «تاریخچه‌ی موقعیت یک وسیله در بازه‌ی زمانی» روی جدولی
/// که به‌سرعت رشد می‌کند (هر چند ثانیه یک رکورد) به‌شدت کند می‌شود.
/// </summary>
public class LocationLogConfiguration : IEntityTypeConfiguration<LocationLog>
{
    public void Configure(EntityTypeBuilder<LocationLog> builder)
    {
        builder.ToTable("LocationLogs");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id).ValueGeneratedOnAdd();

        builder.HasIndex(l => new { l.VehicleId, l.RecordedAtUtc })
               .HasDatabaseName("IX_LocationLog_Vehicle_RecordedAt");

        builder.Property(l => l.RecordedAtUtc).IsRequired();
        builder.Property(l => l.Latitude).IsRequired();
        builder.Property(l => l.Longitude).IsRequired();

        builder.HasOne<Vehicle>()
               .WithMany(v => v.LocationLogs)
               .HasForeignKey(l => l.VehicleId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
