namespace Petstore.Analytics;

public interface IAdminSalesAnalyticsRepository
{
    /// <summary>
    /// Aggregates revenue and sold quantity per category for qualifying
    /// orders placed in [fromInclusive, toExclusive).
    /// </summary>
    Task<IReadOnlyList<AdminCategorySalesRow>> GetCategorySalesAsync(
        DateTime fromInclusive,
        DateTime toExclusive,
        CancellationToken cancellationToken);
}
