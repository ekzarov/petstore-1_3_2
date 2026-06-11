namespace Petstore.OrderProcessing;

public interface IOrderProcessingService
{
    /// <summary>
    /// Evaluates a newly placed order: below the configured threshold it is
    /// auto-approved as the system actor; otherwise it stays PENDING for a
    /// manual decision. Safe to re-run: non-PENDING orders are no-ops.
    /// </summary>
    Task EvaluateNewOrderAsync(int orderId, CancellationToken cancellationToken);

    Task<bool> ApproveAsync(int orderId, string actor, CancellationToken cancellationToken);

    Task<bool> DenyAsync(int orderId, string actor, CancellationToken cancellationToken);
}
