namespace FleetTracker.Api.Dtos;

public sealed class CreateVehicleRequest
{
    public string PlateNumber { get; set; } = default!;
    public string Model { get; set; } = default!;
    public Domain.Enums.VehicleStatus Status { get; set; } = Domain.Enums.VehicleStatus.Offline;
    public Guid? DriverId { get; set; }
}

public sealed class SetGeofenceRequest
{
    public double CenterLatitude { get; set; }
    public double CenterLongitude { get; set; }
    public double RadiusMeters { get; set; }
}
