using FleetTracker.Domain.Enums;

namespace FleetTracker.Domain.Entities;

/// <summary>
/// هشدار تولیدشده برای یک وسیله نقلیه (مثلاً خروج از حوزه‌ی مجاز یا آفلاین شدن).
/// </summary>
public class Alert
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }

    public AlertType Type { get; set; }

    public string Message { get; set; } = default!;

    public DateTimeOffset CreatedAtUtc { get; set; }

    /// <summary>آیا هشدار رفع/بسته شده است؟</summary>
    public bool IsResolved { get; set; }
}
