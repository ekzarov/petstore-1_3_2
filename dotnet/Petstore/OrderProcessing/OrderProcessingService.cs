using Microsoft.EntityFrameworkCore;
using Petstore.Data;
using Petstore.Orders;

namespace Petstore.OrderProcessing;

public sealed class OrderProcessingService(
    PetstoreCatalogContext context,
    OrderTransitionRepository transitionRepository,
    IConfiguration configuration) : IOrderProcessingService
{
    public async Task EvaluateNewOrderAsync(int orderId, CancellationToken cancellationToken)
    {
        var order = await context.Orders
            .AsNoTracking()
            .SingleOrDefaultAsync(o => o.Id == orderId, cancellationToken);
        if (order is null || order.Status != OrderStatus.Pending)
        {
            return;
        }

        var threshold = configuration.GetValue("OrderProcessing:AutoApprovalThreshold", 500m);
        if (order.Total < threshold)
        {
            await transitionRepository.TryTransitionAsync(
                orderId, OrderStatus.Pending, OrderStatus.Approved, OrderWorkflow.SystemActor, cancellationToken);
        }
    }

    public Task<bool> ApproveAsync(int orderId, string actor, CancellationToken cancellationToken)
    {
        return transitionRepository.TryTransitionAsync(
            orderId, OrderStatus.Pending, OrderStatus.Approved, actor, cancellationToken);
    }

    public Task<bool> DenyAsync(int orderId, string actor, CancellationToken cancellationToken)
    {
        return transitionRepository.TryTransitionAsync(
            orderId, OrderStatus.Pending, OrderStatus.Denied, actor, cancellationToken);
    }
}
