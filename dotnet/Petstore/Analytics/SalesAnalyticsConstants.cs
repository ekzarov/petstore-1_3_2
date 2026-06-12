using Petstore.Orders;

namespace Petstore.Analytics;

public static class SalesAnalyticsConstants
{
    /// <summary>
    /// Statuses that count as sales (plan DD-001): shipped activity and
    /// completed orders. PENDING, APPROVED, and DENIED are excluded.
    /// </summary>
    public static readonly IReadOnlyList<string> QualifyingStatuses =
        [OrderStatus.Completed, OrderStatus.Shipped, OrderStatus.ShippedPart];

    public const int PercentScale = 2;

    /// <summary>Bucket for order lines whose item no longer maps to a catalog category.</summary>
    public const string UnknownCategoryId = "UNKNOWN";
}
