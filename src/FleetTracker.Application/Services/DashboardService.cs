using FleetTracker.Application.Dtos;
using FleetTracker.Application.Interfaces;
using FleetTracker.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FleetTracker.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IFleetTrackerDbContext _db;

    public DashboardService(IFleetTrackerDbContext db) => _db = db;

    /// <inheritdoc />
    public async Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken ct = default)
    {
        var total = await _db.Vehicles.CountAsync(ct);
        var active = await _db.Vehicles.CountAsync(v => v.Status == VehicleStatus.Active, ct);
        var idle = await _db.Vehicles.CountAsync(v => v.Status == VehicleStatus.Idle, ct);
        var offline = await _db.Vehicles.CountAsync(v => v.Status == VehicleStatus.Offline, ct);
        var openAlerts = await _db.Alerts.CountAsync(a => !a.IsResolved, ct);

        return new DashboardSummaryDto(total, active, idle, offline, openAlerts);
    }
}
