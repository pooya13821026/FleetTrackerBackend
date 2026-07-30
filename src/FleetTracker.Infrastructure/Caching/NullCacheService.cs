using FleetTracker.Application.Dtos;
using FleetTracker.Application.Interfaces;

namespace FleetTracker.Infrastructure.Caching;

/// <summary>
/// پیاده‌سازی in-memory ساده‌ی کش برای زمانی که Redis در دسترس نیست
/// (مثلاً هنگام اجرای تست‌ها یا توسعه‌ی محلی بدون Docker).
/// </summary>
public class NullCacheService : ICacheService
{
    private readonly Dictionary<Guid, LastLocationDto> _store = new();

    public Task SetLastLocationAsync(Guid vehicleId, LastLocationDto location, CancellationToken ct = default)
    {
        _store[vehicleId] = location;
        return Task.CompletedTask;
    }

    public Task<LastLocationDto?> GetLastLocationAsync(Guid vehicleId, CancellationToken ct = default)
    {
        _store.TryGetValue(vehicleId, out var location);
        return Task.FromResult(location);
    }

    public Task<Dictionary<Guid, LastLocationDto>> GetAllLastLocationsAsync(CancellationToken ct = default)
    {
        return Task.FromResult(new Dictionary<Guid, LastLocationDto>(_store));
    }
}
