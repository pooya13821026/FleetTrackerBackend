namespace FleetTracker.Domain.Enums;

/// <summary>
/// وضعیت عملیاتی وسیله نقلیه.
/// نکته: اولین مقدار صراحتاً به یک حالت معنادار (Offline) نسبت داده شده
/// تا default بودن ناخواسته‌ی یک مقدار enum باعث رفتار غلط نشود.
/// </summary>
public enum VehicleStatus
{
    Active = 0,
    Idle = 1,
    Offline = 2
}
