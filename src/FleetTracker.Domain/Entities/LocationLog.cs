namespace FleetTracker.Domain.Entities;

/// <summary>
/// یک رکورد موقعیت ثبت‌شده برای وسیله نقلیه.
/// این جدول به‌سرعت رشد می‌کند (هر چند ثانیه یک رکورد)؛
/// بنابراین ایندکس روی (VehicleId, RecordedAtUtc) حیاتی است.
/// </summary>
public class LocationLog
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }

    public double Latitude { get; set; }
    public double Longitude { get; set; }

    /// <summary>سرعت لحظه‌ای به کیلومتر بر ساعت.</summary>
    public double SpeedKmh { get; set; }

    /// <summary>زمان ثبت موقعیت بر اساس UTC.</summary>
    public DateTimeOffset RecordedAtUtc { get; set; }
}
