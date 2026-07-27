using FleetTracker.Domain.Entities;

namespace FleetTracker.Application.Interfaces;

public interface IGeofenceService
{
    /// <summary>محاسبه‌ی فاصله‌ی دو نقطه‌ی جغرافیایی بر حسب متر (فرمول Haversine).</summary>
    double CalculateDistanceMeters(double lat1, double lon1, double lat2, double lon2);

    /// <summary>آیا نقطه‌ی داده‌شده خارج از حوزه‌ی مجاز است؟</summary>
    bool IsOutside(double lat, double lng, Geofence fence);
}
