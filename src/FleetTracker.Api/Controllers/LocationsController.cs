using FleetTracker.Application.Dtos;
using FleetTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FleetTracker.Api.Controllers;

/// <summary>
/// دریافت موقعیت‌های لحظه‌ای از دستگاه‌ها/شبیه‌ساز.
/// این endpoint برای ingestion با حجم بالا استفاده می‌شود و فاقد [Authorize] است
/// تا شبیه‌ساز بدون نیاز به توکن بتواند داده بفرستد (در محیط واقعی از API Key استفاده می‌شود).
/// </summary>
[ApiController]
[Route("api/locations")]
public class LocationsController(ILocationService service, ICacheService cache) : ControllerBase
{
    /// <summary>ثبت یک موقعیت جدید و اجرای کل pipeline (ذخیره، کش، اعلان، بررسی Geofence).</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> Ingest([FromBody] LocationIngestRequest request, CancellationToken ct)
    {
        await service.IngestAsync(request, ct);
        return Accepted();
    }

    /// <summary>دریافت آخرین موقعیت شناخته‌شده‌ی همه‌ی وسایل نقلیه.</summary>
    [HttpGet("last")]
    [Authorize]
    [ProducesResponseType(typeof(Dictionary<Guid, LastLocationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllLastLocations(CancellationToken ct)
    {
        var locations = await cache.GetAllLastLocationsAsync(ct);
        return Ok(locations);
    }
}
