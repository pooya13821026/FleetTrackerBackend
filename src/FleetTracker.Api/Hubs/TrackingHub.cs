using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace FleetTracker.Api.Hubs;

/// <summary>
/// Hub سیگنال‌آر برای دریافت موقعیت و هشدارهای لحظه‌ای.
/// کلاینت‌ها می‌توانند به گروهِ یک وسیله‌ی نقلیه بپیوندند تا فقط رویدادهای همان وسیله را دریافت کنند.
/// </summary>
[Authorize]
public class TrackingHub : Hub
{
    /// <summary>عضویت در گروهِ یک وسیله‌ی نقلیه.</summary>
    public Task JoinVehicleGroup(Guid vehicleId)
        => Groups.AddToGroupAsync(Context.ConnectionId, vehicleId.ToString());

    /// <summary>ترک گروهِ یک وسیله‌ی نقلیه.</summary>
    public Task LeaveVehicleGroup(Guid vehicleId)
        => Groups.RemoveFromGroupAsync(Context.ConnectionId, vehicleId.ToString());
}
