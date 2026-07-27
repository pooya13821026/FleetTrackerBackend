using FleetTracker.Application.Services;
using FleetTracker.Domain.Entities;
using FluentAssertions;

namespace FleetTracker.Tests;

public class GeofenceServiceTests
{
    private readonly GeofenceService _sut = new();

    [Fact]
    public void IsOutside_WhenWithinRadius_ReturnsFalse()
    {
        var fence = new Geofence { CenterLatitude = 35.70, CenterLongitude = 51.40, RadiusMeters = 500 };
        var result = _sut.IsOutside(35.7001, 51.4001, fence);
        result.Should().BeFalse();
    }

    [Fact]
    public void IsOutside_WhenFarAway_ReturnsTrue()
    {
        var fence = new Geofence { CenterLatitude = 35.70, CenterLongitude = 51.40, RadiusMeters = 500 };
        var result = _sut.IsOutside(35.90, 51.60, fence);
        result.Should().BeTrue();
    }

    /// <summary>
    /// مقدار مرزی — یکی از مهم‌ترین تست‌هایی که نشان می‌دهد به edge caseها فکر شده.
    /// یک نقطه دقیقاً روی شعاع حوزه (از طریق تبدیل تقریبی متر به درجه latitude) قرار دارد.
    /// </summary>
    [Fact]
    public void IsOutside_ExactlyOnBoundary_ReturnsFalse()
    {
        var fence = new Geofence { CenterLatitude = 35.70, CenterLongitude = 51.40, RadiusMeters = 1000 };
        // تقریب: ۱ درجه latitude ≈ 111,320 متر
        var edgeLat = 35.70 + (1000.0 / 111_320.0);
        var result = _sut.IsOutside(edgeLat, 51.40, fence);
        result.Should().BeFalse("نقطه دقیقاً روی مرز است و باید داخل حساب شود");
    }

    [Fact]
    public void CalculateDistanceMeters_SamePoint_ReturnsZero()
    {
        var distance = _sut.CalculateDistanceMeters(35.70, 51.40, 35.70, 51.40);
        distance.Should().BeApproximately(0, 0.01);
    }

    [Fact]
    public void CalculateDistanceMeters_KnownDistance_IsReasonable()
    {
        // فاصله تقریبی تهران-اصفهان ≈ 340 km
        var distance = _sut.CalculateDistanceMeters(35.69, 51.39, 32.65, 51.67);
        distance.Should().BeGreaterThan(300_000).And.BeLessThan(400_000);
    }
}