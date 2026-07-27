using FleetTracker.Application.Dtos;
using FleetTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FleetTracker.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController(IDashboardService service) : ControllerBase
{
    /// <summary>خلاصه‌ی وضعیت ناوگان برای داشبورد.</summary>
    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryDto>> Summary(CancellationToken ct)
        => Ok(await service.GetSummaryAsync(ct));
}
