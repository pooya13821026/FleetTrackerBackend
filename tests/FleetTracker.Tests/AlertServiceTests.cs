using FleetTracker.Application.Dtos;
using FleetTracker.Application.Interfaces;
using FleetTracker.Application.Services;
using FleetTracker.Domain.Entities;
using FleetTracker.Domain.Enums;
using FleetTracker.Infrastructure.Caching;
using FleetTracker.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FleetTracker.Tests;

public class AlertServiceTests
{
    private static FleetTrackerDbContext CreateInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<FleetTrackerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new FleetTrackerDbContext(options);
    }

    [Fact]
    public async Task CreateGeofenceExitAlert_CreatesAlertWithCorrectType()
    {
        // Arrange
        await using var db = CreateInMemoryDb();
        var vehicle = new Vehicle { Id = Guid.NewGuid(), PlateNumber = "11-الف-123", Model = "سواری" };
        db.Vehicles.Add(vehicle);
        await db.SaveChangesAsync();

        var sut = new AlertService(db);

        // Act
        var result = await sut.CreateGeofenceExitAlertAsync(vehicle.Id);

        // Assert
        result.VehicleId.Should().Be(vehicle.Id);
        result.Type.Should().Be(AlertType.GeofenceExit);
        result.IsResolved.Should().BeFalse();
        result.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ResolveAsync_WhenAlertExists_UpdatesIsResolved()
    {
        // Arrange
        await using var db = CreateInMemoryDb();
        var vehicle = new Vehicle { Id = Guid.NewGuid(), PlateNumber = "11-ب-456", Model = "سواری" };
        db.Vehicles.Add(vehicle);
        await db.SaveChangesAsync();

        var sut = new AlertService(db);
        var alert = await sut.CreateGeofenceExitAlertAsync(vehicle.Id);

        // Act
        var ok = await sut.ResolveAsync(alert.Id);

        // Assert
        ok.Should().BeTrue();
        var resolvedAlert = await sut.GetOpenAlertsAsync();
        resolvedAlert.Should().BeEmpty();
    }

    [Fact]
    public async Task ResolveAsync_WhenAlertNotFound_ReturnsFalse()
    {
        // Arrange
        await using var db = CreateInMemoryDb();
        var sut = new AlertService(db);

        // Act
        var ok = await sut.ResolveAsync(Guid.NewGuid());

        // Assert
        ok.Should().BeFalse();
    }

    [Fact]
    public async Task GetByVehicle_OnlyReturnsAlertsForThatVehicle()
    {
        // Arrange
        await using var db = CreateInMemoryDb();
        var v1 = new Vehicle { Id = Guid.NewGuid(), PlateNumber = "11-ج-111", Model = "سواری" };
        var v2 = new Vehicle { Id = Guid.NewGuid(), PlateNumber = "11-ج-222", Model = "سواری" };
        db.Vehicles.AddRange(v1, v2);
        await db.SaveChangesAsync();

        var sut = new AlertService(db);
        await sut.CreateGeofenceExitAlertAsync(v1.Id);
        await sut.CreateGeofenceExitAlertAsync(v1.Id);
        await sut.CreateGeofenceExitAlertAsync(v2.Id);

        // Act
        var v1Alerts = await sut.GetByVehicleAsync(v1.Id);

        // Assert
        v1Alerts.Should().HaveCount(2);
        v1Alerts.All(a => a.VehicleId == v1.Id).Should().BeTrue();
    }

    [Fact]
    public async Task NullCacheService_SetAndGet_WorksCorrectly()
    {
        // Arrange
        var cache = new NullCacheService();
        var location = new LastLocationDto(Guid.NewGuid(), 35.7, 51.4, 45, DateTimeOffset.UtcNow);

        // Act
        await cache.SetLastLocationAsync(location.VehicleId, location);
        var result = await cache.GetLastLocationAsync(location.VehicleId);

        // Assert
        result.Should().NotBeNull();
        result!.VehicleId.Should().Be(location.VehicleId);
        result.Latitude.Should().BeApproximately(location.Latitude, 0.001);
    }

    [Fact]
    public async Task NullCacheService_GetNonExistent_ReturnsNull()
    {
        var cache = new NullCacheService();
        var result = await cache.GetLastLocationAsync(Guid.NewGuid());
        result.Should().BeNull();
    }

    [Fact]
    public async Task NullCacheService_GetAllLastLocations_ReturnsAllStored()
    {
        var cache = new NullCacheService();
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var loc1 = new LastLocationDto(id1, 35.70, 51.40, 50, DateTimeOffset.UtcNow);
        var loc2 = new LastLocationDto(id2, 35.80, 51.50, 60, DateTimeOffset.UtcNow);

        await cache.SetLastLocationAsync(id1, loc1);
        await cache.SetLastLocationAsync(id2, loc2);

        var result = await cache.GetAllLastLocationsAsync();

        result.Should().HaveCount(2);
        result[id1].Latitude.Should().BeApproximately(35.70, 0.001);
        result[id2].Latitude.Should().BeApproximately(35.80, 0.001);
    }

    [Fact]
    public async Task NullCacheService_GetAllLastLocations_EmptyStore_ReturnsEmpty()
    {
        var cache = new NullCacheService();
        var result = await cache.GetAllLastLocationsAsync();
        result.Should().BeEmpty();
    }
}