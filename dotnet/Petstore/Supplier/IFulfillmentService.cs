namespace Petstore.Supplier;

public interface IFulfillmentService
{
    /// <summary>
    /// Ships what inventory allows for one APPROVED/SHIPPED_PART order and
    /// advances its status through the feature-010 workflow. Idempotent:
    /// already-shipped quantities are never shipped again.
    /// </summary>
    Task FulfillOrderAsync(int orderId, CancellationToken cancellationToken);

    /// <summary>Runs fulfillment for every eligible order (operational safety valve).</summary>
    Task<int> FulfillAllEligibleAsync(CancellationToken cancellationToken);

    /// <summary>Re-runs fulfillment for eligible orders containing the given item (after replenishment).</summary>
    Task FulfillOrdersForItemAsync(string itemId, CancellationToken cancellationToken);
}
