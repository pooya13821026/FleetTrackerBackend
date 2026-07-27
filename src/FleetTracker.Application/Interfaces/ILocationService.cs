using FleetTracker.Application.Dtos;

namespace FleetTracker.Application.Interfaces;

public interface ILocationService
{
    /// <summary>
    /// دریافت و پردازش یک موقعیت جدید: ذخیره در SQL، کش آخرین موقعیت در Redis،
    /// اعلان لحظه‌ای و بررسی خروج از حوزه‌ی مجاز.
    /// </summary>
    Task IngestAsync(LocationIngestRequest request, CancellationToken ct = default);
}
