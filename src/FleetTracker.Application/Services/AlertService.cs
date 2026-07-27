using FleetTracker.Application.Dtos;
using FleetTracker.Application.Interfaces;
using FleetTracker.Domain.Entities;
using FleetTracker.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FleetTracker.Application.Services;

public class AlertService(IFleetTrackerDbContext db) : IAlertService
{
    /// <inheritdoc />
    public async Task<List<AlertDto>> GetByVehicleAsync(Guid vehicleId, CancellationToken ct = default)
    {
        return await db.Alerts
            .AsNoTracking()
            .Where(a => a.VehicleId == vehicleId)
            .OrderByDescending(a => a.CreatedAtUtc)
            .Select(a => new AlertDto(
                a.Id,
                a.VehicleId,
                a.Type,
                a.Message,
                a.CreatedAtUtc,
                a.IsResolved))
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<List<AlertDto>> GetOpenAlertsAsync(CancellationToken ct = default)
    {
        return await db.Alerts
            .AsNoTracking()
            .Where(a => !a.IsResolved)
            .OrderByDescending(a => a.CreatedAtUtc)
            .Select(a => new AlertDto(
                a.Id,
                a.VehicleId,
                a.Type,
                a.Message,
                a.CreatedAtUtc,
                a.IsResolved))
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<AlertDto> CreateGeofenceExitAlertAsync(Guid vehicleId, CancellationToken ct = default)
    {
        var alert = new Alert
        {
            Id = Guid.NewGuid(),
            VehicleId = vehicleId,
            Type = AlertType.GeofenceExit,
            Message = "وسیله نقلیه از حوزه‌ی مجاز خارج شد.",
            CreatedAtUtc = DateTimeOffset.UtcNow,
            IsResolved = false
        };

        db.Alerts.Add(alert);
        await db.SaveChangesAsync(ct);

        return new AlertDto(
            alert.Id,
            alert.VehicleId,
            alert.Type,
            alert.Message,
            alert.CreatedAtUtc,
            alert.IsResolved);
    }

    /// <inheritdoc />
    public async Task<bool> ResolveAsync(Guid alertId, CancellationToken ct = default)
    {
        var alert = await db.Alerts.FirstOrDefaultAsync(a => a.Id == alertId, ct);
        if (alert is null)
            return false;

        alert.IsResolved = true;
        await db.SaveChangesAsync(ct);
        return true;
    }
}
