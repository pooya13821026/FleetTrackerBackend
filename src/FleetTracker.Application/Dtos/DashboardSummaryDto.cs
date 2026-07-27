namespace FleetTracker.Application.Dtos;

/// <summary>
/// خلاصه‌ی داشبورد برای نمایش وضعیت کلی ناوگان.
/// </summary>
/// <param name="TotalVehicles">تعداد کل وسایل نقلیه.</param>
/// <param name="ActiveVehicles">تعداد وسایل فعال.</param>
/// <param name="IdleVehicles">تعداد وسایل در حالت Idle.</param>
/// <param name="OfflineVehicles">تعداد وسایل آفلاین.</param>
/// <param name="OpenAlerts">تعداد هشدارهای باز (رفع‌نشده).</param>
public sealed record DashboardSummaryDto(
    int TotalVehicles,
    int ActiveVehicles,
    int IdleVehicles,
    int OfflineVehicles,
    int OpenAlerts);
