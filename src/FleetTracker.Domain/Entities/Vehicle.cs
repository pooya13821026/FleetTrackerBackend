using FleetTracker.Domain.Enums;

namespace FleetTracker.Domain.Entities;

/// <summary>
/// وسیله نقلیه‌ای که موقعیت آن ردیابی می‌شود.
/// </summary>
public class Vehicle
{
    public Guid Id { get; set; }

    /// <summary>شماره پلاک یکتای وسیله نقلیه.</summary>
    public string PlateNumber { get; set; } = default!;

    /// <summary>مدل/تیپ وسیله نقلیه.</summary>
    public string Model { get; set; } = default!;

    /// <summary>وضعیت عملیاتی؛ پیش‌فرض Offline تا زمانی که اولین موقعیت ثبت شود.</summary>
    public VehicleStatus Status { get; set; } = VehicleStatus.Offline;

    /// <summary>شناسه‌ی راننده‌ی اختصاص‌داده‌شده (اختیاری).</summary>
    public Guid? DriverId { get; set; }
    public Driver? Driver { get; set; }

    /// <summary>حوزه‌ی مجاز (Geofence) اختصاص‌داده‌شده به این وسیله (اختیاری).</summary>
    public Geofence? Geofence { get; set; }

    public ICollection<LocationLog> LocationLogs { get; set; } = new List<LocationLog>();
    public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
}
