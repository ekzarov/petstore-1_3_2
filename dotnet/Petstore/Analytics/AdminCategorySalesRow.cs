namespace Petstore.Analytics;

/// <summary>Raw aggregation row produced by the repository before percentage math.</summary>
public sealed record AdminCategorySalesRow(
    string CategoryId,
    string? CategoryName,
    decimal Revenue,
    int SalesCount);
