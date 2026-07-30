using FleetTracker.Application.Dtos;
using FleetTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FleetTracker.Api.Controllers;

[ApiController]
[Route("api/drivers")]
[Authorize]
public class DriversController(IDriverService drivers) : ControllerBase
{
    /// <summary>لیست همه‌ی راننده‌ها.</summary>
    [HttpGet]
    public async Task<ActionResult<List<DriverDto>>> GetAll(CancellationToken ct)
        => Ok(await drivers.GetAllAsync(ct));

    /// <summary>دریافت یک راننده با شناسه.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DriverDto>> GetById(Guid id, CancellationToken ct)
    {
        var driver = await drivers.GetByIdAsync(id, ct);
        return driver is null ? NotFound() : Ok(driver);
    }

    /// <summary>ثبت راننده‌ی جدید.</summary>
    [HttpPost]
    public async Task<ActionResult<DriverDto>> Create([FromBody] CreateDriverRequest request, CancellationToken ct)
    {
        var dto = await drivers.CreateAsync(request.FullName, request.NationalCode, request.PhoneNumber, ct);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    /// <summary>ویرایش اطلاعات راننده.</summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<DriverDto>> Update(Guid id, [FromBody] UpdateDriverRequest request, CancellationToken ct)
    {
        var dto = await drivers.UpdateAsync(id, request.FullName, request.NationalCode, request.PhoneNumber, ct);
        return dto is null ? NotFound() : Ok(dto);
    }

    /// <summary>حذف راننده.</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        try
        {
            var deleted = await drivers.DeleteAsync(id, ct);
            return deleted ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}

public sealed class CreateDriverRequest
{
    public string FullName { get; set; } = default!;
    public string? NationalCode { get; set; }
    public string PhoneNumber { get; set; } = default!;
}

public sealed class UpdateDriverRequest
{
    public string FullName { get; set; } = default!;
    public string? NationalCode { get; set; }
    public string PhoneNumber { get; set; } = default!;
}
