namespace FleetTracker.Domain.Entities;

/// <summary>
/// حوزه‌ی مجاز دایره‌ای شکل حول یک مرکز جغرافیایی.
/// آخرین وضعیت شناخته‌شده (داخل/خارج) به‌صورت صریح نگه‌داری می‌شود
/// تا هشدار تکراری در خروج از حوزه تولید نشود (تشخیص گذار state).
/// </summary>
public class Geofence
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }

    public double CenterLatitude { get; set; }
    public double CenterLongitude { get; set; }
    public double RadiusMeters { get; set; }

    /// <summary>
    /// آخرین وضعیت شناخته‌شده: true یعنی وسیله داخل حوزه است.
    /// برای جلوگیری از Alert تکراری هنگام خروج/بازگشت.
    /// </summary>
    public bool IsCurrentlyInside { get; set; } = true;
}
