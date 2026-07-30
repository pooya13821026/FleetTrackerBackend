using FleetTracker.Application.Dtos;

namespace FleetTracker.Application.Interfaces;

public interface ICacheService
{
    /// <summary>ذخیره‌ی آخرین موقعیت شناخته‌شده‌ی وسیله (hot-data).</summary>
    Task SetLastLocationAsync(Guid vehicleId, LastLocationDto location, CancellationToken ct = default);

    /// <summary>بازخوانی آخرین موقعیت وسیله از کش (بدون رفتن به SQL).</summary>
    Task<LastLocationDto?> GetLastLocationAsync(Guid vehicleId, CancellationToken ct = default);

    /// <summary>بازخوانی آخرین موقعیت همه‌ی وسایل از کش.</summary>
    Task<Dictionary<Guid, LastLocationDto>> GetAllLastLocationsAsync(CancellationToken ct = default);
}
