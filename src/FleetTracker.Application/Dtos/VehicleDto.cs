using FleetTracker.Domain.Enums;

namespace FleetTracker.Application.Dtos;

/// <summary>
/// نمای خلاصه‌ی یک وسیله نقلیه با اطلاعات راننده و آخرین موقعیت.
/// </summary>
public sealed record VehicleDto
{
    public Guid Id { get; init; }
    public string PlateNumber { get; init; } = "";
    public string Model { get; init; } = "";
    public VehicleStatus Status { get; init; }
    public Guid? DriverId { get; init; }
    public string? DriverName { get; init; }
    public string? DriverNationalCode { get; init; }
    public string? DriverPhoneNumber { get; init; }

    /// <summary>آخرین عرض جغرافیایی (null اگر موقعیتی ثبت نشده).</summary>
    public double? LastLatitude { get; init; }
    /// <summary>آخرین طول جغرافیایی.</summary>
    public double? LastLongitude { get; init; }
    /// <summary>آخرین سرعت (km/h).</summary>
    public double? LastSpeedKmh { get; init; }
    /// <summary>زمان آخرین موقعیت.</summary>
    public DateTimeOffset? LastRecordedAtUtc { get; init; }
}
