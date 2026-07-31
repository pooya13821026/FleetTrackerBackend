using FleetTracker.Application.Dtos;
using FleetTracker.Application.Services;
using FleetTracker.Domain.Entities;
using FleetTracker.Domain.Enums;
using FleetTracker.Infrastructure.Caching;
using FleetTracker.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FleetTracker.Tests;

public class VehicleServiceTests
{
    private static FleetTrackerDbContext CreateInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<FleetTrackerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new FleetTrackerDbContext(options);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllVehicles()
    {
        await using var db = CreateInMemoryDb();
        db.Vehicles.AddRange(
            new Vehicle { Id = Guid.NewGuid(), PlateNumber = "11الف11111", Model = "پراید", Status = VehicleStatus.Active },
            new Vehicle { Id = Guid.NewGuid(), PlateNumber = "22ب22222", Model = "سمند", Status = VehicleStatus.Idle }
        );
        await db.SaveChangesAsync();

        var sut = new VehicleService(db);
        var result = await sut.GetAllAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_IncludesLocationData()
    {
        await using var db = CreateInMemoryDb();
        var vehicleId = Guid.NewGuid();
        db.Vehicles.Add(new Vehicle { Id = vehicleId, PlateNumber = "33ج33333", Model = "تارا", Status = VehicleStatus.Active });
        db.LocationLogs.Add(new LocationLog
        {
            VehicleId = vehicleId,
            Latitude = 35.7219,
            Longitude = 51.3347,
            SpeedKmh = 60,
            RecordedAtUtc = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();

        var sut = new VehicleService(db);
        var result = await sut.GetAllAsync();

        var vehicle = result.Should().ContainSingle(v => v.Id == vehicleId).Which;
        vehicle.LastLatitude.Should().BeApproximately(35.7219, 0.0001);
        vehicle.LastLongitude.Should().BeApproximately(51.3347, 0.0001);
        vehicle.LastSpeedKmh.Should().BeApproximately(60, 0.01);
        vehicle.LastRecordedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAllAsync_VehicleWithNoLocation_ReturnsNullLocationFields()
    {
        await using var db = CreateInMemoryDb();
        var vehicleId = Guid.NewGuid();
        db.Vehicles.Add(new Vehicle { Id = vehicleId, PlateNumber = "44د44444", Model = "کی‌ام‌سی", Status = VehicleStatus.Offline });
        await db.SaveChangesAsync();

        var sut = new VehicleService(db);
        var result = await sut.GetAllAsync();

        var vehicle = result.Should().ContainSingle(v => v.Id == vehicleId).Which;
        vehicle.LastLatitude.Should().BeNull();
        vehicle.LastLongitude.Should().BeNull();
        vehicle.LastSpeedKmh.Should().BeNull();
        vehicle.LastRecordedAtUtc.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsLatestLocationOnly()
    {
        await using var db = CreateInMemoryDb();
        var vehicleId = Guid.NewGuid();
        db.Vehicles.Add(new Vehicle { Id = vehicleId, PlateNumber = "55س55555", Model = "دنا", Status = VehicleStatus.Active });

        var oldTime = DateTimeOffset.UtcNow.AddMinutes(-10);
        var newTime = DateTimeOffset.UtcNow;
        db.LocationLogs.AddRange(
            new LocationLog { VehicleId = vehicleId, Latitude = 35.68, Longitude = 51.32, SpeedKmh = 30, RecordedAtUtc = oldTime },
            new LocationLog { VehicleId = vehicleId, Latitude = 35.72, Longitude = 51.38, SpeedKmh = 50, RecordedAtUtc = newTime }
        );
        await db.SaveChangesAsync();

        var sut = new VehicleService(db);
        var result = await sut.GetAllAsync();

        var vehicle = result.Should().ContainSingle(v => v.Id == vehicleId).Which;
        vehicle.LastLatitude.Should().BeApproximately(35.72, 0.01);
        vehicle.LastLongitude.Should().BeApproximately(51.38, 0.01);
    }

    [Fact]
    public async Task GetByIdAsync_WithLocation_ReturnsCorrectData()
    {
        await using var db = CreateInMemoryDb();
        var vehicleId = Guid.NewGuid();
        var driverId = Guid.NewGuid();
        db.Drivers.Add(new Driver { Id = driverId, FullName = "علی", NationalCode = "1234567890", PhoneNumber = "09121112233" });
        db.Vehicles.Add(new Vehicle
        {
            Id = vehicleId,
            PlateNumber = "66ص66666",
            Model = "رانا",
            Status = VehicleStatus.Active,
            DriverId = driverId
        });
        db.LocationLogs.Add(new LocationLog
        {
            VehicleId = vehicleId,
            Latitude = 35.74,
            Longitude = 51.40,
            SpeedKmh = 70,
            RecordedAtUtc = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();

        var sut = new VehicleService(db);
        var result = await sut.GetByIdAsync(vehicleId);

        result.Should().NotBeNull();
        result!.DriverName.Should().Be("علی");
        result.LastLatitude.Should().BeApproximately(35.74, 0.01);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistent_ReturnsNull()
    {
        await using var db = CreateInMemoryDb();
        var sut = new VehicleService(db);

        var result = await sut.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ReturnsVehicleWithId()
    {
        await using var db = CreateInMemoryDb();
        var sut = new VehicleService(db);

        var result = await sut.CreateAsync(new Vehicle
        {
            PlateNumber = "77ق77777",
            Model = "ساینا",
            Status = VehicleStatus.Active
        });

        result.Id.Should().NotBeEmpty();
        result.PlateNumber.Should().Be("77ق77777");
    }
}
