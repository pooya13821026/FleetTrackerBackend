namespace FleetTracker.Application.Dtos;

/// <summary>
/// آخرین موقعیت شناخته‌شده‌ی یک وسیله نقلیه (hot-data؛ از Redis خوانده می‌شود).
/// </summary>
/// <param name="VehicleId">شناسه‌ی وسیله.</param>
/// <param name="Latitude">عرض جغرافیایی.</param>
/// <param name="Longitude">طول جغرافیایی.</param>
/// <param name="SpeedKmh">سرعت لحظه‌ای (km/h).</param>
/// <param name="RecordedAtUtc">زمان ثبت موقعیت (UTC).</param>
public sealed record LastLocationDto(
    Guid VehicleId,
    double Latitude,
    double Longitude,
    double SpeedKmh,
    DateTimeOffset RecordedAtUtc);
