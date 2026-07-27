using FleetTracker.Api.DTO;
using FleetTracker.Application.Dtos;
using FleetTracker.Application.Interfaces;
using FleetTracker.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FleetTracker.Api.Controllers;

[ApiController]
[Route("api/vehicles")]
[Authorize]
public class VehiclesController(IVehicleService vehicles, ICacheService cache) : ControllerBase
{
    /// <summary>لیست همه‌ی وسایل نقلیه.</summary>
    [HttpGet]
    public async Task<ActionResult<List<VehicleDto>>> GetAll(CancellationToken ct)
        => Ok(await vehicles.GetAllAsync(ct));

    /// <summary>دریافت یک وسیله نقلیه با شناسه.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<VehicleDto>> GetById(Guid id, CancellationToken ct)
    {
        var vehicle = await vehicles.GetByIdAsync(id, ct);
        return vehicle is null ? NotFound() : Ok(vehicle);
    }

    /// <summary>ثبت یک وسیله نقلیه‌ی جدید.</summary>
    [HttpPost]
    public async Task<ActionResult<VehicleDto>> Create([FromBody] CreateVehicleRequest request, CancellationToken ct)
    {
        var vehicle = new Vehicle
        {
            PlateNumber = request.PlateNumber,
            Model = request.Model,
            Status = request.Status,
            DriverId = request.DriverId
        };

        var dto = await vehicles.CreateAsync(vehicle, ct);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    /// <summary>
    /// آخرین موقعیت شناخته‌شده‌ی وسیله.
    /// نکته: این متد فقط Redis را می‌خواند و به SQL سر نمی‌زند (جداکردن hot-data از cold-data).
    /// </summary>
    [HttpGet("{id:guid}/locations/last")]
    public async Task<ActionResult<LastLocationDto>> GetLastLocation(Guid id, CancellationToken ct)
    {
        var location = await cache.GetLastLocationAsync(id, ct);
        return location is null ? NotFound() : Ok(location);
    }

    /// <summary>اختصاص یا به‌روزرسانی حوزه‌ی مجاز (Geofence) وسیله.</summary>
    [HttpPost("{id:guid}/geofence")]
    public async Task<IActionResult> SetGeofence(Guid id, [FromBody] SetGeofenceRequest request, CancellationToken ct)
    {
        await vehicles.SetGeofenceAsync(id, request.CenterLatitude, request.CenterLongitude, request.RadiusMeters, ct);
        return NoContent();
    }
}