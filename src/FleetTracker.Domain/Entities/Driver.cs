namespace FleetTracker.Domain.Entities;

/// <summary>
/// راننده‌ی اختصاص‌داده‌شده به یک یا چند وسیله نقلیه.
/// </summary>
public class Driver
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
}
