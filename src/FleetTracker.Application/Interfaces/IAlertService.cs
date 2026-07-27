using FleetTracker.Application.Dtos;

namespace FleetTracker.Application.Interfaces;

public interface IAlertService
{
    Task<List<AlertDto>> GetByVehicleAsync(Guid vehicleId, CancellationToken ct = default);

    Task<List<AlertDto>> GetOpenAlertsAsync(CancellationToken ct = default);

    /// <summary>ایجاد هشدار خروج از حوزه‌ی مجاز برای وسیله و بازگرداندن DTO آن.</summary>
    Task<AlertDto> CreateGeofenceExitAlertAsync(Guid vehicleId, CancellationToken ct = default);

    /// <summary>رفع/بستن یک هشدار.</summary>
    Task<bool> ResolveAsync(Guid alertId, CancellationToken ct = default);
}
