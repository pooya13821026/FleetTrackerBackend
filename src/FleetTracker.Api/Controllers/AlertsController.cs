using FleetTracker.Application.Dtos;
using FleetTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FleetTracker.Api.Controllers;

[ApiController]
[Route("api/alerts")]
[Authorize]
public class AlertsController(IAlertService service) : ControllerBase
{
    /// <summary>لیست همه‌ی هشدارهای باز (رفع‌نشده).</summary>
    [HttpGet("open")]
    public async Task<ActionResult<List<AlertDto>>> GetOpen(CancellationToken ct)
        => Ok(await service.GetOpenAlertsAsync(ct));

    /// <summary>لیست هشدارهای یک وسیله نقلیه.</summary>
    [HttpGet("vehicle/{vehicleId:long}")]
    public async Task<ActionResult<List<AlertDto>>> GetByVehicle(Guid vehicleId, CancellationToken ct)
        => Ok(await service.GetByVehicleAsync(vehicleId, ct));

    /// <summary>رفع/بستن یک هشدار.</summary>
    [HttpPost("{id:long}/resolve")]
    public async Task<IActionResult> Resolve(Guid id, CancellationToken ct)
    {
        var ok = await service.ResolveAsync(id, ct);
        return ok ? NoContent() : NotFound();
    }
}
