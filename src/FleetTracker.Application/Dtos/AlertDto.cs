using FleetTracker.Domain.Enums;

namespace FleetTracker.Application.Dtos;

/// <summary>
/// نمای یک هشدار برای ارسال به کلاینت (REST یا SignalR).
/// </summary>
/// <param name="Id">شناسه‌ی هشدار.</param>
/// <param name="VehicleId">شناسه‌ی وسیله نقلیه.</param>
/// <param name="Type">نوع هشدار.</param>
/// <param name="Message">متن هشدار.</param>
/// <param name="CreatedAtUtc">زمان ایجاد (UTC).</param>
/// <param name="IsResolved">آیا رفع شده؟</param>
public sealed record AlertDto(
    Guid Id,
    Guid VehicleId,
    AlertType Type,
    string Message,
    DateTimeOffset CreatedAtUtc,
    bool IsResolved);
