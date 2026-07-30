using FleetTracker.Domain.Entities;

namespace FleetTracker.Application.Dtos;

/// <summary>
/// نمای اطلاعات راننده.
/// </summary>
/// <param name="Id">شناسه‌ی یکتای راننده.</param>
/// <param name="FullName">نام کامل.</param>
/// <param name="NationalCode">کد ملی.</param>
/// <param name="PhoneNumber">شماره تماس.</param>
public sealed record DriverDto(
    Guid Id,
    string FullName,
    string? NationalCode,
    string PhoneNumber);
