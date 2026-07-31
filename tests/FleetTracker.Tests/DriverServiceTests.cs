using FleetTracker.Application.Services;
using FleetTracker.Domain.Entities;
using FleetTracker.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FleetTracker.Tests;

public class DriverServiceTests
{
    private static FleetTrackerDbContext CreateInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<FleetTrackerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new FleetTrackerDbContext(options);
    }

    [Fact]
    public async Task CreateAsync_AddsDriverAndReturnsDto()
    {
        await using var db = CreateInMemoryDb();
        var sut = new DriverService(db);

        var result = await sut.CreateAsync("حسین رضاخانی", "0012345678", "09121234567");

        result.Id.Should().NotBeEmpty();
        result.FullName.Should().Be("حسین رضاخانی");
        result.NationalCode.Should().Be("0012345678");
        result.PhoneNumber.Should().Be("09121234567");
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllDrivers()
    {
        await using var db = CreateInMemoryDb();
        db.Drivers.AddRange(
            new Driver { Id = Guid.NewGuid(), FullName = "علی", NationalCode = "111", PhoneNumber = "0911" },
            new Driver { Id = Guid.NewGuid(), FullName = "رضا", NationalCode = "222", PhoneNumber = "0922" }
        );
        await db.SaveChangesAsync();

        var sut = new DriverService(db);
        var result = await sut.GetAllAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingDriver_ReturnsDriver()
    {
        await using var db = CreateInMemoryDb();
        var id = Guid.NewGuid();
        db.Drivers.Add(new Driver { Id = id, FullName = "محمد", NationalCode = "333", PhoneNumber = "0933" });
        await db.SaveChangesAsync();

        var sut = new DriverService(db);
        var result = await sut.GetByIdAsync(id);

        result.Should().NotBeNull();
        result!.FullName.Should().Be("محمد");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistent_ReturnsNull()
    {
        await using var db = CreateInMemoryDb();
        var sut = new DriverService(db);

        var result = await sut.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ExistingDriver_UpdatesAndReturns()
    {
        await using var db = CreateInMemoryDb();
        var id = Guid.NewGuid();
        db.Drivers.Add(new Driver { Id = id, FullName = "قبلی", NationalCode = "111", PhoneNumber = "0911" });
        await db.SaveChangesAsync();

        var sut = new DriverService(db);
        var result = await sut.UpdateAsync(id, "جدید", "999", "0999");

        result.Should().NotBeNull();
        result!.FullName.Should().Be("جدید");
        result.NationalCode.Should().Be("999");
    }

    [Fact]
    public async Task UpdateAsync_NonExistent_ReturnsNull()
    {
        await using var db = CreateInMemoryDb();
        var sut = new DriverService(db);

        var result = await sut.UpdateAsync(Guid.NewGuid(), "test", null, "0900");

        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_UnassignedDriver_ReturnsTrue()
    {
        await using var db = CreateInMemoryDb();
        var id = Guid.NewGuid();
        db.Drivers.Add(new Driver { Id = id, FullName = "حذف شونده", NationalCode = "111", PhoneNumber = "0911" });
        await db.SaveChangesAsync();

        var sut = new DriverService(db);
        var result = await sut.DeleteAsync(id);

        result.Should().BeTrue();
        (await db.Drivers.FindAsync(id)).Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_AssignedDriver_ThrowsException()
    {
        await using var db = CreateInMemoryDb();
        var driverId = Guid.NewGuid();
        db.Drivers.Add(new Driver { Id = driverId, FullName = "راننده فعال", NationalCode = "222", PhoneNumber = "0922" });
        db.Vehicles.Add(new Vehicle { Id = Guid.NewGuid(), PlateNumber = "11الف11111", Model = "پراید", DriverId = driverId });
        await db.SaveChangesAsync();

        var sut = new DriverService(db);

        var act = () => sut.DeleteAsync(driverId);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task DeleteAsync_NonExistent_ReturnsFalse()
    {
        await using var db = CreateInMemoryDb();
        var sut = new DriverService(db);

        var result = await sut.DeleteAsync(Guid.NewGuid());

        result.Should().BeFalse();
    }
}
