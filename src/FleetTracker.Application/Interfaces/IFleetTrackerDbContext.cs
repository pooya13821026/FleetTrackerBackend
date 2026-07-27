using FleetTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FleetTracker.Application.Interfaces;

/// <summary>
/// انتزاع DbContext برای رفع وابستگی مستقیم لایه‌ی Application به EF Core/SQL.
/// پیاده‌سازی واقعی (<see cref="Persistence.FleetTrackerDbContext"/>) در لایه‌ی Infrastructure قرار دارد.
/// این قرارداد به Application اجازه می‌دهد بدون ارجاع دایره‌ای به Infrastructure، به داده‌ها دسترسی داشته باشد.
/// </summary>
public interface IFleetTrackerDbContext
{
    DbSet<Vehicle> Vehicles { get; }
    DbSet<Driver> Drivers { get; }
    DbSet<LocationLog> LocationLogs { get; }
    DbSet<Geofence> Geofences { get; }
    DbSet<Alert> Alerts { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
