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
            .Include(v => v.Driver)
            .Select(v => new VehicleDto
            {
                Id = v.Id,
                PlateNumber = v.PlateNumber,
                Model = v.Model,
                Status = v.Status,
                DriverId = v.DriverId,
                DriverName = v.Driver != null ? v.Driver.FullName : null,
                DriverNationalCode = v.Driver != null ? v.Driver.NationalCode : null,
                DriverPhoneNumber = v.Driver != null ? v.Driver.PhoneNumber : null,
                LastLatitude = v.LocationLogs
                    .OrderByDescending(l => l.RecordedAtUtc)
                    .Select(l => (double?)l.Latitude)
                    .FirstOrDefault(),
                LastLongitude = v.LocationLogs
                    .OrderByDescending(l => l.RecordedAtUtc)
                    .Select(l => (double?)l.Longitude)
                    .FirstOrDefault(),
                LastSpeedKmh = v.LocationLogs
                    .OrderByDescending(l => l.RecordedAtUtc)
                    .Select(l => (double?)l.SpeedKmh)
                    .FirstOrDefault(),
                LastRecordedAtUtc = v.LocationLogs
                    .OrderByDescending(l => l.RecordedAtUtc)
                    .Select(l => (DateTimeOffset?)l.RecordedAtUtc)
                    .FirstOrDefault(),
            })
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<VehicleDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.Vehicles
            .AsNoTracking()
            .Include(v => v.Driver)
            .Where(v => v.Id == id)
            .Select(v => new VehicleDto
            {
                Id = v.Id,
                PlateNumber = v.PlateNumber,
                Model = v.Model,
                Status = v.Status,
                DriverId = v.DriverId,
                DriverName = v.Driver != null ? v.Driver.FullName : null,
                DriverNationalCode = v.Driver != null ? v.Driver.NationalCode : null,
                DriverPhoneNumber = v.Driver != null ? v.Driver.PhoneNumber : null,
                LastLatitude = v.LocationLogs
                    .OrderByDescending(l => l.RecordedAtUtc)
                    .Select(l => (double?)l.Latitude)
                    .FirstOrDefault(),
                LastLongitude = v.LocationLogs
                    .OrderByDescending(l => l.RecordedAtUtc)
                    .Select(l => (double?)l.Longitude)
                    .FirstOrDefault(),
                LastSpeedKmh = v.LocationLogs
                    .OrderByDescending(l => l.RecordedAtUtc)
                    .Select(l => (double?)l.SpeedKmh)
                    .FirstOrDefault(),
                LastRecordedAtUtc = v.LocationLogs
                    .OrderByDescending(l => l.RecordedAtUtc)
                    .Select(l => (DateTimeOffset?)l.RecordedAtUtc)
                    .FirstOrDefault(),
            })
            .FirstOrDefaultAsync(ct);
    }

    /// <inheritdoc />
    public async Task<VehicleDto> CreateAsync(Vehicle vehicle, CancellationToken ct = default)
    {
        if (vehicle.Id == Guid.Empty)
            vehicle.Id = Guid.NewGuid();

        _db.Vehicles.Add(vehicle);
        await _db.SaveChangesAsync(ct);

        // اگر راننده دارد، اطلاعاتش را برگردان
        string? driverName = null;
        string? driverNationalCode = null;
        string? driverPhoneNumber = null;

        if (vehicle.DriverId.HasValue)
        {
            var driver = await _db.Drivers.FindAsync(vehicle.DriverId.Value);
            if (driver != null)
            {
                driverName = driver.FullName;
                driverNationalCode = driver.NationalCode;
                driverPhoneNumber = driver.PhoneNumber;
            }
        }

        return new VehicleDto
        {
            Id = vehicle.Id,
            PlateNumber = vehicle.PlateNumber,
            Model = vehicle.Model,
            Status = vehicle.Status,
            DriverId = vehicle.DriverId,
            DriverName = driverName,
            DriverNationalCode = driverNationalCode,
            DriverPhoneNumber = driverPhoneNumber,
        };
    }

    /// <inheritdoc />
    public async Task SetGeofenceAsync(Guid vehicleId, double centerLatitude, double centerLongitude, double radiusMeters, CancellationToken ct = default)
    {
        var fence = await _db.Geofences.FirstOrDefaultAsync(g => g.VehicleId == vehicleId, ct);
        if (fence is null)
        {
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
