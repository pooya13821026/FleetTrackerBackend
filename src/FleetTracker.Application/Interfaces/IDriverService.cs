using FleetTracker.Application.Dtos;

namespace FleetTracker.Application.Interfaces;

public interface IDriverService
{
    Task<List<DriverDto>> GetAllAsync(CancellationToken ct = default);

    Task<DriverDto?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<DriverDto> CreateAsync(string fullName, string? nationalCode, string phoneNumber, CancellationToken ct = default);

    Task<DriverDto?> UpdateAsync(Guid id, string fullName, string? nationalCode, string phoneNumber, CancellationToken ct = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
