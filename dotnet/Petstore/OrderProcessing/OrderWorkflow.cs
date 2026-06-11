using Petstore.Orders;

namespace Petstore.OrderProcessing;

/// <summary>
/// Legal status transition graph (plan DD-005). Mirrors the legacy
/// processmanager lifecycle: PENDING -> APPROVED|DENIED;
/// APPROVED -> SHIPPED_PART|SHIPPED; SHIPPED_PART -> SHIPPED_PART|SHIPPED;
/// SHIPPED -> COMPLETED. DENIED and COMPLETED are terminal.
/// </summary>
public static class OrderWorkflow
{
    public const string SystemActor = "system";

    private static readonly IReadOnlyDictionary<string, string[]> LegalTransitions =
        new Dictionary<string, string[]>
        {
            [OrderStatus.Pending] = [OrderStatus.Approved, OrderStatus.Denied],
            [OrderStatus.Approved] = [OrderStatus.ShippedPart, OrderStatus.Shipped],
            [OrderStatus.ShippedPart] = [OrderStatus.ShippedPart, OrderStatus.Shipped],
            [OrderStatus.Shipped] = [OrderStatus.Completed],
            [OrderStatus.Denied] = [],
            [OrderStatus.Completed] = []
        };

    public static bool IsLegal(string from, string to)
    {
        return LegalTransitions.TryGetValue(from, out var targets) && targets.Contains(to);
    }
}
