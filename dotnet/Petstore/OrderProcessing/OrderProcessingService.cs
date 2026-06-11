using Microsoft.EntityFrameworkCore;
using Petstore.Data;
using Petstore.Orders;
using Petstore.Supplier;

namespace Petstore.OrderProcessing;

public sealed class OrderProcessingService(
    PetstoreCatalogContext context,
    OrderTransitionRepository transitionRepository,
    IConfiguration configuration,
    IFulfillmentService fulfillmentService) : IOrderProcessingService
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
            var approved = await transitionRepository.TryTransitionAsync(
                orderId, OrderStatus.Pending, OrderStatus.Approved, OrderWorkflow.SystemActor, cancellationToken);
            if (approved)
            {
                await fulfillmentService.FulfillOrderAsync(orderId, cancellationToken);
            }
        }
    }

    public async Task<bool> ApproveAsync(int orderId, string actor, CancellationToken cancellationToken)
    {
        var approved = await transitionRepository.TryTransitionAsync(
            orderId, OrderStatus.Pending, OrderStatus.Approved, actor, cancellationToken);
        if (approved)
        {
            // Approved orders go straight to fulfillment (011 DD-001).
            await fulfillmentService.FulfillOrderAsync(orderId, cancellationToken);
        }

        return approved;
    }

    public Task<bool> DenyAsync(int orderId, string actor, CancellationToken cancellationToken)
    {
        return transitionRepository.TryTransitionAsync(
            orderId, OrderStatus.Pending, OrderStatus.Denied, actor, cancellationToken);
    }
}
