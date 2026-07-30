using System.Text.Json;
using FleetTracker.Application.Dtos;
using FleetTracker.Application.Interfaces;
using StackExchange.Redis;

namespace FleetTracker.Infrastructure.Caching;

/// <summary>
/// پیاده‌سازی کش مبتنی بر Redis برای آخرین موقعیت وسایل نقلیه.
/// فقط آخرین مقدار برای هر وسیله مهم است؛ بنابراین StringSet ساده (overwrite) کافی است.
/// </summary>
public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public RedisCacheService(IConnectionMultiplexer redis) => _redis = redis;

    /// <inheritdoc />
    public async Task SetLastLocationAsync(Guid vehicleId, LastLocationDto location, CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        var json = JsonSerializer.Serialize(location, JsonOptions);
        await db.StringSetAsync(Key(vehicleId), json);
    }

    /// <inheritdoc />
    public async Task<LastLocationDto?> GetLastLocationAsync(Guid vehicleId, CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        var json = await db.StringGetAsync(Key(vehicleId));
        return json.IsNullOrEmpty ? null : JsonSerializer.Deserialize<LastLocationDto>(json!, JsonOptions);
    }

    /// <inheritdoc />
    public async Task<Dictionary<Guid, LastLocationDto>> GetAllLastLocationsAsync(CancellationToken ct = default)
    {
        var server = _redis.GetServer(_redis.GetEndPoints().First());
        var db = _redis.GetDatabase();
        var result = new Dictionary<Guid, LastLocationDto>();

        var keys = server.Keys(pattern: "vehicle:*:last-location");
        foreach (var key in keys)
        {
            var json = await db.StringGetAsync(key);
            if (!json.IsNullOrEmpty)
            {
                var location = JsonSerializer.Deserialize<LastLocationDto>(json!, JsonOptions);
                if (location != null)
                    result[location.VehicleId] = location;
            }
        }

        return result;
    }

    private static string Key(Guid vehicleId) => $"vehicle:{vehicleId}:last-location";
}
