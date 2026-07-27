using FleetTracker.Application.Dtos;
using FleetTracker.Application.Interfaces;
using FleetTracker.Domain.Entities;
using FleetTracker.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FleetTracker.Application.Services;

/// <summary>
/// هماهنگ‌کننده‌ی اصلی سیستم ردیابی ناوگان (قلب برنامه).
/// وظایف این سرویس به‌ازای هر موقعیت دریافت‌شده:
///   ۱) ذخیره‌ی رکورد در SQL (cold-data / تاریخچه)
///   ۲) به‌روزرسانی آخرین موقعیت در Redis (hot-data)
///   ۳) اعلان لحظه‌ای به کلاینت‌ها از طریق SignalR
///   ۴) بررسی خروج/بازگشت از حوزه‌ی مجاز و تولید هشدار فقط هنگام تغییر وضعیت
/// </summary>
public class LocationService : ILocationService
{
    private readonly IFleetTrackerDbContext _db;
    private readonly ICacheService _cache;
    private readonly IGeofenceService _geofence;
    private readonly IAlertService _alertService;
    private readonly IRealtimeNotifier _notifier;
    private readonly ILogger<LocationService> _logger;

    public LocationService(
        IFleetTrackerDbContext db,
        ICacheService cache,
        IGeofenceService geofence,
        IAlertService alertService,
        IRealtimeNotifier notifier,
        ILogger<LocationService> logger)
    {
        _db = db;
        _cache = cache;
        _geofence = geofence;
        _alertService = alertService;
        _notifier = notifier;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task IngestAsync(LocationIngestRequest request, CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;

        // ۱) ذخیره در SQL به‌عنوان تاریخچه
        var log = new LocationLog
        {
            VehicleId = request.VehicleId,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            SpeedKmh = request.SpeedKmh,
            RecordedAtUtc = now
        };
        _db.LocationLogs.Add(log);
        await _db.SaveChangesAsync(ct);

        // ۲) به‌روزرسانی آخرین موقعیت در Redis (overwrite، نه append)
        var lastLocation = new LastLocationDto(
            request.VehicleId,
            request.Latitude,
            request.Longitude,
            request.SpeedKmh,
            now);
        await _cache.SetLastLocationAsync(request.VehicleId, lastLocation, ct);

        // ۳) اعلان لحظه‌ای به گروهِ وسیله‌ی نقلیه
        await _notifier.NotifyLocationUpdatedAsync(lastLocation, ct);

        // ۴) بررسی حوزه‌ی مجاز — فقط هنگام تغییر state هشدار می‌سازد (نه هر بار)
        await EvaluateGeofenceAsync(request.VehicleId, request.Latitude, request.Longitude, ct);
    }

    /// <summary>
    /// منطق تشخیص گذار وضعیت داخل/خارج حوزه‌ی مجاز.
    /// state قبلی صریحاً روی Geofence نگه‌داری می‌شود تا هشدار تکراری تولید نشود.
    /// </summary>
    private async Task EvaluateGeofenceAsync(Guid vehicleId, double latitude, double longitude, CancellationToken ct)
    {
        var fence = await _db.Geofences.FirstOrDefaultAsync(f => f.VehicleId == vehicleId, ct);
        if (fence is null)
            return;

        var isOutsideNow = _geofence.IsOutside(latitude, longitude, fence);

        // گذار از «داخل» به «خارج» → تولید هشدار
        if (isOutsideNow && fence.IsCurrentlyInside)
        {
            fence.IsCurrentlyInside = false;

            // ابتدا state عوض شود، سپس notify (جلوگیری از race condition / اعلان دوبار)
            await _db.SaveChangesAsync(ct);

            var alert = await _alertService.CreateGeofenceExitAlertAsync(vehicleId, ct);
            await _notifier.NotifyAlertCreatedAsync(alert, ct);

            _logger.LogWarning("وسیله {VehicleId} از حوزه‌ی مجاز خارج شد", vehicleId);
        }
        // گذار از «خارج» به «داخل» → فقط به‌روزرسانی state (بدون هشدار)
        else if (!isOutsideNow && !fence.IsCurrentlyInside)
        {
            fence.IsCurrentlyInside = true;
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("وسیله {VehicleId} به حوزه‌ی مجاز بازگشت", vehicleId);
        }
    }
}
