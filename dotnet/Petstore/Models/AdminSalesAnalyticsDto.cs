namespace Petstore.Models;

public sealed record AdminSalesAnalyticsDto(
    string StartDate,
    string EndDate,
    decimal TotalRevenue,
    int TotalSalesCount,
    IReadOnlyList<AdminCategorySalesMetricDto> Categories);
