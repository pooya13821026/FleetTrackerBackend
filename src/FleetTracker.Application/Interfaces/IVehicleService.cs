using FleetTracker.Application.Dtos;
using FleetTracker.Domain.Entities;

namespace FleetTracker.Application.Interfaces;

public interface IVehicleService
{
    Task<List<VehicleDto>> GetAllAsync(CancellationToken ct = default);

    Task<VehicleDto?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<VehicleDto> CreateAsync(Vehicle vehicle, CancellationToken ct = default);

    /// <summary>اختصاص یا به‌روزرسانی حوزه‌ی مجاز (Geofence) یک وسیله.</summary>
    Task SetGeofenceAsync(Guid vehicleId, double centerLatitude, double centerLongitude, double radiusMeters, CancellationToken ct = default);
}
