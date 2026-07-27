namespace FleetTracker.Api.DTO;

public sealed record LoginResponse(string Token, DateTime ExpiresAtUtc);