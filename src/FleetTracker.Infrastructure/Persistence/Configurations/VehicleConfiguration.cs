using FleetTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FleetTracker.Infrastructure.Persistence.Configurations;

public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("Vehicles");

        builder.HasKey(v => v.Id);

        // شماره پلاک یکتا است تا دو وسیله با پلاک تکراری ثبت نشوند
        builder.HasIndex(v => v.PlateNumber)
               .IsUnique();

        builder.Property(v => v.PlateNumber)
               .HasMaxLength(20)
               .IsRequired();

        builder.Property(v => v.Model)
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(v => v.Status)
               .HasConversion<int>();

        // رابطه با راننده (اختیاری)
        builder.HasOne(v => v.Driver)
               .WithMany()
               .HasForeignKey(v => v.DriverId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
