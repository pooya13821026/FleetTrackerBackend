using FleetTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FleetTracker.Infrastructure.Persistence.Configurations;

public class AlertConfiguration : IEntityTypeConfiguration<Alert>
{
    public void Configure(EntityTypeBuilder<Alert> builder)
    {
        builder.ToTable("Alerts");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Type).HasConversion<int>();

        builder.Property(a => a.Message)
               .HasMaxLength(500)
               .IsRequired();

        builder.Property(a => a.CreatedAtUtc).IsRequired();

        // ایندکس برای کوئری «هشدارهای بازِ یک وسیله»
        builder.HasIndex(a => new { a.VehicleId, a.IsResolved })
               .HasDatabaseName("IX_Alerts_Vehicle_Resolved");

        builder.HasOne<Vehicle>()
               .WithMany(v => v.Alerts)
               .HasForeignKey(a => a.VehicleId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
