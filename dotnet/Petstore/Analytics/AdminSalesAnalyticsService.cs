using Petstore.Models;

namespace Petstore.Analytics;

public sealed class AdminSalesAnalyticsService(IAdminSalesAnalyticsRepository repository) : IAdminSalesAnalyticsService
{
    public async Task<AdminSalesAnalyticsDto> GetSalesAnalyticsAsync(
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken)
    {
        // Inclusive calendar range -> half-open UTC timestamp range.
        var fromInclusive = startDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var toExclusive = endDate.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        var rows = await repository.GetCategorySalesAsync(fromInclusive, toExclusive, cancellationToken);

        var totalRevenue = rows.Sum(row => row.Revenue);
        var totalSalesCount = rows.Sum(row => row.SalesCount);

        var categories = rows
            .OrderByDescending(row => row.Revenue)
            .ThenBy(row => row.CategoryId)
            .Select(row => new AdminCategorySalesMetricDto(
                row.CategoryId,
                row.CategoryName ?? row.CategoryId,
                row.Revenue,
                ComputePercent(row.Revenue, totalRevenue),
                row.SalesCount))
            .ToList();

        return new AdminSalesAnalyticsDto(
            startDate.ToString(SalesAnalyticsDateRangeRequest.DateFormat),
            endDate.ToString(SalesAnalyticsDateRangeRequest.DateFormat),
            totalRevenue,
            totalSalesCount,
            categories);
    }

    public static decimal ComputePercent(decimal revenue, decimal totalRevenue)
    {
        if (totalRevenue == 0)
        {
            return 0m;
        }

        return Math.Round(revenue / totalRevenue * 100m, SalesAnalyticsConstants.PercentScale);
    }
}
