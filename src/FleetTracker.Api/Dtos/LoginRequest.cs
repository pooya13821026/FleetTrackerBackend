using System.ComponentModel.DataAnnotations;

namespace FleetTracker.Api.Dtos;

public sealed class LoginRequest
{
    [Required]
    public string Username { get; set; } = default!;

    [Required]
    public string Password { get; set; } = default!;
}

public sealed record LoginResponse(string Token, DateTime ExpiresAtUtc);
