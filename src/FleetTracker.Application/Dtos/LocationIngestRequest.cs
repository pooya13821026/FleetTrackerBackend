using System.ComponentModel.DataAnnotations;

namespace FleetTracker.Application.Dtos;

/// <summary>
/// درخواست ثبت موقعیت لحظه‌ای وسیله نقلیه (مثلاً از طرف دستگاه GPS یا شبیه‌ساز).
/// </summary>
public sealed class LocationIngestRequest
{
    [Required]
    public Guid VehicleId { get; set; }

    [Range(-90, 90)]
    public double Latitude { get; set; }

    [Range(-180, 180)]
    public double Longitude { get; set; }

    [Range(0, double.MaxValue)]
    public double SpeedKmh { get; set; }
}
