using Petstore.Models;

namespace Petstore.Analytics;

public interface IAdminSalesAnalyticsService
{
    Task<AdminSalesAnalyticsDto> GetSalesAnalyticsAsync(
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken);
}
