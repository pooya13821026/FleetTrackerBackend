using FleetTracker.Application.Dtos;
using FleetTracker.Application.Interfaces;
using FleetTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FleetTracker.Application.Services;

public class VehicleService : IVehicleService
{
    private readonly IFleetTrackerDbContext _db;

    public VehicleService(IFleetTrackerDbContext db) => _db = db;

    /// <inheritdoc />
    public async Task<List<VehicleDto>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.Vehicles
            .AsNoTracking()
            .Select(v => new VehicleDto(
                v.Id,
                v.PlateNumber,
                v.Model,
                v.Status,
                v.DriverId))
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<VehicleDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.Vehicles
            .AsNoTracking()
            .Where(v => v.Id == id)
            .Select(v => new VehicleDto(
                v.Id,
                v.PlateNumber,
                v.Model,
                v.Status,
                v.DriverId))
            .FirstOrDefaultAsync(ct);
    }

    /// <inheritdoc />
    public async Task<VehicleDto> CreateAsync(Vehicle vehicle, CancellationToken ct = default)
    {
        if (vehicle.Id == Guid.Empty)
            vehicle.Id = Guid.NewGuid();

        _db.Vehicles.Add(vehicle);
        await _db.SaveChangesAsync(ct);

        return new VehicleDto(
            vehicle.Id,
            vehicle.PlateNumber,
            vehicle.Model,
            vehicle.Status,
            vehicle.DriverId);
    }

    /// <inheritdoc />
    public async Task SetGeofenceAsync(Guid vehicleId, double centerLatitude, double centerLongitude, double radiusMeters, CancellationToken ct = default)
    {
        var fence = await _db.Geofences.FirstOrDefaultAsync(g => g.VehicleId == vehicleId, ct);
        if (fence is null)
        {
            // اطمینان از وجود وسیله نقلیه پیش از ساخت Geofence
            var vehicleExists = await _db.Vehicles.AnyAsync(v => v.Id == vehicleId, ct);
            if (!vehicleExists)
                throw new InvalidOperationException($"وسیله نقلیه با شناسه {vehicleId} یافت نشد.");

            fence = new Geofence
            {
                VehicleId = vehicleId,
                CenterLatitude = centerLatitude,
                CenterLongitude = centerLongitude,
                RadiusMeters = radiusMeters,
                IsCurrentlyInside = true
            };
            _db.Geofences.Add(fence);
        }
        else
        {
            fence.CenterLatitude = centerLatitude;
            fence.CenterLongitude = centerLongitude;
            fence.RadiusMeters = radiusMeters;
        }

        await _db.SaveChangesAsync(ct);
    }
}
