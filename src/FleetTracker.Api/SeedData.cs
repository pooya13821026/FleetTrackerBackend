using FleetTracker.Application.Dtos;
using FleetTracker.Application.Interfaces;
using FleetTracker.Domain.Entities;
using FleetTracker.Domain.Enums;
using FleetTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FleetTracker.Api;

/// <summary>
/// داده‌های اولیه نمونه (drivers, vehicles, initial locations) برای محیط Development.
/// </summary>
public static class SeedData
{
    public static async Task InitializeAsync(FleetTrackerDbContext db, ICacheService cache, ILogger logger)
    {
        db.Database.EnsureCreated();

        if (await db.Vehicles.AnyAsync())
            return;

        // --- Drivers ---
        var drivers = new Driver[]
        {
            new() { Id = Guid.NewGuid(), FullName = "حسین رضاخانی", NationalCode = "0012345678", PhoneNumber = "09121234567" },
            new() { Id = Guid.NewGuid(), FullName = "رضا احمدی",   NationalCode = "0023456789", PhoneNumber = "09129876543" },
            new() { Id = Guid.NewGuid(), FullName = "علی محمدی",   NationalCode = "0034567890", PhoneNumber = "09351112233" },
            new() { Id = Guid.NewGuid(), FullName = "محمد حسینی",  NationalCode = "0045678901", PhoneNumber = "09193334455" },
            new() { Id = Guid.NewGuid(), FullName = "امیر کاظمی",  NationalCode = "0056789012", PhoneNumber = "09367778899" },
            new() { Id = Guid.NewGuid(), FullName = "سعید نوری",   NationalCode = "0067890123", PhoneNumber = "09125556677" },
            new() { Id = Guid.NewGuid(), FullName = "مرتضی عباسی", NationalCode = "0078901234", PhoneNumber = "09214445566" },
            new() { Id = Guid.NewGuid(), FullName = "بهروز شریفی", NationalCode = "0089012345", PhoneNumber = "09132223344" },
        };
        db.Drivers.AddRange(drivers);

        // --- Vehicles ---
        var vehicles = new Vehicle[]
        {
            new() { Id = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"), PlateNumber = "21ب54367",   Model = "سمند EF7",      Status = VehicleStatus.Active, DriverId = drivers[0].Id, Driver = drivers[0] },
            new() { Id = Guid.Parse("11111111-2222-3333-4444-555555555555"), PlateNumber = "12الف32145", Model = "پراید 151",     Status = VehicleStatus.Active, DriverId = drivers[1].Id, Driver = drivers[1] },
            new() { Id = Guid.Parse("22222222-3333-4444-5555-666666666666"), PlateNumber = "33جی78912",  Model = "دنا پلاس",      Status = VehicleStatus.Idle,   DriverId = drivers[2].Id, Driver = drivers[2] },
            new() { Id = Guid.Parse("33333333-4444-5555-6666-777777777777"), PlateNumber = "44د23456",   Model = "تارا",          Status = VehicleStatus.Active, DriverId = drivers[3].Id, Driver = drivers[3] },
            new() { Id = Guid.Parse("44444444-5555-6666-7777-888888888888"), PlateNumber = "55س65432",   Model = "کی‌ام‌سی J5",    Status = VehicleStatus.Active, DriverId = drivers[4].Id, Driver = drivers[4] },
            new() { Id = Guid.Parse("55555555-6666-7777-8888-999999999999"), PlateNumber = "67ص87654",   Model = "رانا پلاس",     Status = VehicleStatus.Idle,   DriverId = drivers[5].Id, Driver = drivers[5] },
            new() { Id = Guid.Parse("66666666-7777-8888-9999-aaaaaaaaaaaa"), PlateNumber = "78ق34567",   Model = "ساینا S",       Status = VehicleStatus.Offline, DriverId = drivers[6].Id, Driver = drivers[6] },
            new() { Id = Guid.Parse("77777777-8888-9999-aaaa-bbbbbbbbbbbb"), PlateNumber = "89ل98765",   Model = "کوییک R",      Status = VehicleStatus.Active, DriverId = drivers[7].Id, Driver = drivers[7] },
            new() { Id = Guid.Parse("88888888-9999-aaaa-bbbb-cccccccccccc"), PlateNumber = "11م11223",   Model = "هایما S7",      Status = VehicleStatus.Active },
            new() { Id = Guid.Parse("99999999-aaaa-bbbb-cccc-dddddddddddd"), PlateNumber = "22ن44556",   Model = "چری آریزو 5",   Status = VehicleStatus.Idle },
            new() { Id = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-111111111111"), PlateNumber = "33و77889",   Model = "MVM 315",       Status = VehicleStatus.Offline },
            new() { Id = Guid.Parse("bbbbbbbb-cccc-dddd-eeee-222222222222"), PlateNumber = "44ه33445",   Model = "لیفان X60",     Status = VehicleStatus.Active },
        };
        db.Vehicles.AddRange(vehicles);
        await db.SaveChangesAsync();
        logger.LogInformation("دوازده وسیله نقلیه و هشت راننده نمونه به دیتابیس اضافه شد.");

        // --- Initial Locations ---
        var now = DateTimeOffset.UtcNow;
        var initialLocations = new (Guid VehicleId, double Lat, double Lng)[]
        {
            // Active
            (Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"), 35.7219, 51.3347),
            (Guid.Parse("11111111-2222-3333-4444-555555555555"), 35.7500, 51.3800),
            (Guid.Parse("33333333-4444-5555-6666-777777777777"), 35.7300, 51.4200),
            (Guid.Parse("44444444-5555-6666-7777-888888888888"), 35.7200, 51.2800),
            (Guid.Parse("77777777-8888-9999-aaaa-bbbbbbbbbbbb"), 35.7450, 51.4050),
            (Guid.Parse("88888888-9999-aaaa-bbbb-cccccccccccc"), 35.6900, 51.3700),
            (Guid.Parse("99999999-aaaa-bbbb-cccc-dddddddddddd"), 35.7400, 51.4400),
            (Guid.Parse("bbbbbbbb-cccc-dddd-eeee-222222222222"), 35.6700, 51.3500),
            // Idle
            (Guid.Parse("22222222-3333-4444-5555-666666666666"), 35.6850, 51.3250),
            (Guid.Parse("55555555-6666-7777-8888-999999999999"), 35.7100, 51.2700),
            // Offline
            (Guid.Parse("66666666-7777-8888-9999-aaaaaaaaaaaa"), 35.7050, 51.2950),
            (Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-111111111111"), 35.7550, 51.3750),
        };

        foreach (var (vehicleId, lat, lng) in initialLocations)
        {
            db.LocationLogs.Add(new LocationLog
            {
                VehicleId = vehicleId,
                Latitude = lat,
                Longitude = lng,
                SpeedKmh = 0,
                RecordedAtUtc = now
            });
        }
        await db.SaveChangesAsync();
        logger.LogInformation("موقعیت اولیه ۱۲ خودرو ذخیره شد.");

        // Redis
        foreach (var (vehicleId, lat, lng) in initialLocations)
        {
            await cache.SetLastLocationAsync(vehicleId, new LastLocationDto(vehicleId, lat, lng, 0, now));
        }
        logger.LogInformation("موقعیت اولیه در Redis کش شد.");
    }
}
