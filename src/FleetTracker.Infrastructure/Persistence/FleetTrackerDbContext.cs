using FleetTracker.Application.Interfaces;
using FleetTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FleetTracker.Infrastructure.Persistence;

/// <summary>
/// DbContext اصلی برنامه که قرارداد <see cref="IFleetTrackerDbContext"/> را پیاده‌سازی می‌کند.
/// این پیاده‌سازی در لایه‌ی Infrastructure قرار دارد تا Application به آن مستقیم وابسته نباشد.
/// </summary>
public class FleetTrackerDbContext : DbContext, IFleetTrackerDbContext
{
    public FleetTrackerDbContext(DbContextOptions<FleetTrackerDbContext> options) : base(options) { }

    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Driver> Drivers => Set<Driver>();
    public DbSet<LocationLog> LocationLogs => Set<LocationLog>();
    public DbSet<Geofence> Geofences => Set<Geofence>();
    public DbSet<Alert> Alerts => Set<Alert>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FleetTrackerDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
