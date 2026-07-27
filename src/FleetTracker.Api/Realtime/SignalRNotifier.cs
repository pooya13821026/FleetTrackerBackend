using FleetTracker.Application.Dtos;
using FleetTracker.Application.Interfaces;
using FleetTracker.Api.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace FleetTracker.Api.Realtime;

/// <summary>
/// پیاده‌سازی <see cref="IRealtimeNotifier"/> با SignalR.
/// رویدادها به گروهِ وسیله‌ی نقلیه ارسال می‌شوند (مثلاً همه‌ی کلاینت‌هایی که آن وسیله را تماشا می‌کنند).
/// </summary>
public class SignalRNotifier(IHubContext<TrackingHub> hub) : IRealtimeNotifier
{
    public Task NotifyLocationUpdatedAsync(LastLocationDto location, CancellationToken ct = default)
        => hub.Clients.Group(location.VehicleId.ToString())
                       .SendAsync("locationUpdated", location, ct);

    public Task NotifyAlertCreatedAsync(AlertDto alert, CancellationToken ct = default)
        => hub.Clients.Group(alert.VehicleId.ToString())
                       .SendAsync("alertCreated", alert, ct);
}
