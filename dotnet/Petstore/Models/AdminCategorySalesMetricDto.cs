namespace Petstore.Models;

public sealed record AdminCategorySalesMetricDto(
    string CategoryId,
    string CategoryName,
    decimal Revenue,
    decimal RevenuePercent,
    int SalesCount);
