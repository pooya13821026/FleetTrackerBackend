using FleetTracker.Application.Dtos;

namespace FleetTracker.Application.Interfaces;

/// <summary>
/// انتزاع اعلان لحظه‌ای به کلاینت‌ها.
/// لایه‌ی Application به این Interface وابسته است، نه به SignalR مستقیماً؛
/// بنابراین پیاده‌سازی (SignalR یا هر چیز دیگر) در لایه‌ی Infrastructure قرار می‌گیرد.
/// </summary>
public interface IRealtimeNotifier
{
    Task NotifyLocationUpdatedAsync(LastLocationDto location, CancellationToken ct = default);

    Task NotifyAlertCreatedAsync(AlertDto alert, CancellationToken ct = default);
}
