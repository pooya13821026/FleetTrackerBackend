using FleetTracker.Application.Dtos;
using FleetTracker.Application.Interfaces;
using FleetTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FleetTracker.Application.Services;

public class DriverService : IDriverService
{
    private readonly IFleetTrackerDbContext _db;

    public DriverService(IFleetTrackerDbContext db) => _db = db;

    /// <inheritdoc />
    public async Task<List<DriverDto>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.Drivers
            .AsNoTracking()
            .Select(d => new DriverDto(d.Id, d.FullName, d.NationalCode, d.PhoneNumber))
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<DriverDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.Drivers
            .AsNoTracking()
            .Where(d => d.Id == id)
            .Select(d => new DriverDto(d.Id, d.FullName, d.NationalCode, d.PhoneNumber))
            .FirstOrDefaultAsync(ct);
    }

    /// <inheritdoc />
    public async Task<DriverDto> CreateAsync(string fullName, string? nationalCode, string phoneNumber, CancellationToken ct = default)
    {
        var driver = new Driver
        {
            Id = Guid.NewGuid(),
            FullName = fullName,
            NationalCode = nationalCode,
            PhoneNumber = phoneNumber
        };

        _db.Drivers.Add(driver);
        await _db.SaveChangesAsync(ct);

        return new DriverDto(driver.Id, driver.FullName, driver.NationalCode, driver.PhoneNumber);
    }

    /// <inheritdoc />
    public async Task<DriverDto?> UpdateAsync(Guid id, string fullName, string? nationalCode, string phoneNumber, CancellationToken ct = default)
    {
        var driver = await _db.Drivers.FindAsync(id);
        if (driver is null)
            return null;

        driver.FullName = fullName;
        driver.NationalCode = nationalCode;
        driver.PhoneNumber = phoneNumber;

        await _db.SaveChangesAsync(ct);

        return new DriverDto(driver.Id, driver.FullName, driver.NationalCode, driver.PhoneNumber);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var driver = await _db.Drivers.FindAsync(id);
        if (driver is null)
            return false;

        // بررسی آیا راننده به وسیله‌ای اختصاص دارد
        var hasVehicle = await _db.Vehicles.AnyAsync(v => v.DriverId == id, ct);
        if (hasVehicle)
            throw new InvalidOperationException("این راننده به یک وسیله نقلیه اختصاص دارد. ابتدا وسیله را ویرایش کنید.");

        _db.Drivers.Remove(driver);
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
