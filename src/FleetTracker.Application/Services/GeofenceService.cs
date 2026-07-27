using FleetTracker.Application.Interfaces;
using FleetTracker.Domain.Entities;

namespace FleetTracker.Application.Services;

/// <summary>
/// محاسبات مرتبط با حوزه‌ی مجاز (Geofence).
/// از فرمول Haversine برای محاسبه‌ی فاصله‌ی بین دو نقطه روی کره‌ی زمین استفاده می‌کند.
/// </summary>
public class GeofenceService : IGeofenceService
{
    private const double EarthRadiusMeters = 6_371_000;

    /// <inheritdoc />
    public double CalculateDistanceMeters(double lat1, double lon1, double lat2, double lon2)
    {
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return EarthRadiusMeters * c;
    }

    /// <inheritdoc />
    public bool IsOutside(double lat, double lng, Geofence fence)
    {
        var distance = CalculateDistanceMeters(lat, lng, fence.CenterLatitude, fence.CenterLongitude);
        return distance > fence.RadiusMeters;
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180.0;
}
