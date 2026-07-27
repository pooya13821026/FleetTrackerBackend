namespace FleetTracker.Api.DTO;

public sealed class SetGeofenceRequest
{
    public double CenterLatitude { get; set; }
    public double CenterLongitude { get; set; }
    public double RadiusMeters { get; set; }
}