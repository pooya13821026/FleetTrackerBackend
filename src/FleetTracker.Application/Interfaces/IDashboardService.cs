using FleetTracker.Application.Dtos;

namespace FleetTracker.Application.Interfaces;

public interface IDashboardService
{
    /// <summary>محاسبه‌ی خلاصه‌ی داشبورد ناوگان.</summary>
    Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken ct = default);
}
