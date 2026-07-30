using FleetTracker.Domain.Enums;

namespace FleetTracker.Application.Dtos;

/// <summary>
/// نمای خلاصه‌ی یک وسیله نقلیه با اطلاعات راننده.
/// </summary>
/// <param name="Id">شناسه‌ی یکتای وسیله.</param>
/// <param name="PlateNumber">شماره پلاک.</param>
/// <param name="Model">مدل وسیله.</param>
/// <param name="Status">وضعیت عملیاتی.</param>
/// <param name="DriverId">شناسه‌ی راننده (در صورت وجود).</param>
/// <param name="DriverName">نام کامل راننده (در صورت وجود).</param>
/// <param name="DriverNationalCode">کد ملی راننده (در صورت وجود).</param>
/// <param name="DriverPhoneNumber">شماره تماس راننده (در صورت وجود).</param>
public sealed record VehicleDto(
    Guid Id,
    string PlateNumber,
    string Model,
    VehicleStatus Status,
    Guid? DriverId,
    string? DriverName = null,
    string? DriverNationalCode = null,
    string? DriverPhoneNumber = null);
