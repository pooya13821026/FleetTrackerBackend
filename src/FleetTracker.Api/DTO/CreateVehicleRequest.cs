namespace FleetTracker.Api.DTO;

public sealed class CreateVehicleRequest
{
    public string PlateNumber { get; set; } = default!;
    public string Model { get; set; } = default!;
    public Domain.Enums.VehicleStatus Status { get; set; } = Domain.Enums.VehicleStatus.Offline;
    public Guid? DriverId { get; set; }
}