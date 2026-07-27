using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FleetTracker.Api.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace FleetTracker.Api.Controllers;

/// <summary>
/// احراز هویت ساده مبتنی بر JWT.
/// برای MVP از یوزر/پسورد ثابت در تنظیمات استفاده می‌شود؛
/// در محیط واقعی باید به Identity/外付け IDP متصل شود.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController(IConfiguration config) : ControllerBase
{
    /// <summary>ورود و دریافت توکن دسترسی.</summary>
    [HttpPost("login")]
    public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
    {
        var expectedUser = config["Auth:Username"] ?? "admin";
        var expectedPass = config["Auth:Password"] ?? throw new InvalidOperationException("Auth:Password تنظیم نشده است.");

        if (request.Username != expectedUser || request.Password != expectedPass)
            return Unauthorized(new { message = "نام کاربری یا گذرواژه نادرست است." });

        var token = GenerateJwtToken(request.Username);
        return Ok(new LoginResponse(token, DateTime.UtcNow.AddHours(8)));
    }

    private string GenerateJwtToken(string username)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.UniqueName, username),
            new Claim(ClaimTypes.Role, "Operator")
        };

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
