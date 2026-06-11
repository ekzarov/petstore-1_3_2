using Microsoft.EntityFrameworkCore;
using Petstore.Data;
using Petstore.Data.Entities;
using Petstore.OrderProcessing;
using Petstore.Orders;

namespace Petstore.Supplier;

public sealed class FulfillmentService(
    PetstoreCatalogContext context,
    IInventoryRepository inventoryRepository,
    OrderTransitionRepository transitionRepository) : IFulfillmentService
{
    private static readonly string[] EligibleStatuses = [OrderStatus.Approved, OrderStatus.ShippedPart];

    public async Task FulfillOrderAsync(int orderId, CancellationToken cancellationToken)
    {
        // Read the status as a projection: the tracked entity in this scope
        // can be stale after the ExecuteUpdate-based status transition.
        var status = await context.Orders
            .Where(o => o.Id == orderId)
            .Select(o => o.Status)
            .SingleOrDefaultAsync(cancellationToken);
        if (status is null || !EligibleStatuses.Contains(status))
        {
            return;
        }

        var orderLines = await context.OrderLines
            .Where(line => line.OrderId == orderId)
            .ToListAsync(cancellationToken);

        var shippedLines = new List<ShipmentLineEntity>();
        foreach (var line in orderLines)
        {
            var remaining = line.Quantity - line.QuantityShipped;
            if (remaining <= 0)
            {
                continue;
            }

            var onHand = await inventoryRepository.GetOnHandAsync(line.ItemId, cancellationToken);
            var take = FulfillmentRules.TakeQuantity(remaining, onHand);
            if (take == 0)
            {
                continue;
            }

            // Guarded decrement: a concurrent shipment may have consumed the
            // stock between the read and this update; fall back to whatever
            // is still available once.
            if (!await inventoryRepository.TryDecrementAsync(line.ItemId, take, cancellationToken))
            {
                var freshOnHand = await inventoryRepository.GetOnHandAsync(line.ItemId, cancellationToken);
                take = FulfillmentRules.TakeQuantity(remaining, freshOnHand);
                if (take == 0 || !await inventoryRepository.TryDecrementAsync(line.ItemId, take, cancellationToken))
                {
                    continue;
                }
            }

            line.QuantityShipped += take;
            shippedLines.Add(new ShipmentLineEntity { ItemId = line.ItemId, QuantityShipped = take });
        }

        if (shippedLines.Count == 0)
        {
            return;
        }

        // Invoice record: the shipment evidence driving the status change.
        context.Shipments.Add(new ShipmentEntity
        {
            OrderId = orderId,
            OccurredAt = DateTime.UtcNow,
            Lines = shippedLines
        });
        await context.SaveChangesAsync(cancellationToken);

        var outcome = FulfillmentRules.Classify(
            orderLines.Select(line => (line.Quantity, line.QuantityShipped)).ToList());

        if (outcome == ShipmentOutcome.FullyShipped)
        {
            // Completion follows immediately after the final invoice (plan DD-004).
            await transitionRepository.TryTransitionAsync(
                orderId, status, OrderStatus.Shipped, OrderWorkflow.SystemActor, cancellationToken);
            await transitionRepository.TryTransitionAsync(
                orderId, OrderStatus.Shipped, OrderStatus.Completed, OrderWorkflow.SystemActor, cancellationToken);
        }
        else if (outcome == ShipmentOutcome.PartiallyShipped && status == OrderStatus.Approved)
        {
            await transitionRepository.TryTransitionAsync(
                orderId, OrderStatus.Approved, OrderStatus.ShippedPart, OrderWorkflow.SystemActor, cancellationToken);
        }
        // SHIPPED_PART -> SHIPPED_PART after another partial pass needs no transition.
    }

    public async Task<int> FulfillAllEligibleAsync(CancellationToken cancellationToken)
    {
        var orderIds = await context.Orders
            .AsNoTracking()
            .Where(order => EligibleStatuses.Contains(order.Status))
            .Select(order => order.Id)
            .ToListAsync(cancellationToken);

        foreach (var orderId in orderIds)
        {
            await FulfillOrderAsync(orderId, cancellationToken);
        }

        return orderIds.Count;
    }

    public async Task FulfillOrdersForItemAsync(string itemId, CancellationToken cancellationToken)
    {
        var orderIds = await context.OrderLines
            .AsNoTracking()
            .Where(line => line.ItemId == itemId && line.QuantityShipped < line.Quantity)
            .Where(line => EligibleStatuses.Contains(line.Order!.Status))
            .Select(line => line.OrderId)
            .Distinct()
            .ToListAsync(cancellationToken);

        foreach (var orderId in orderIds)
        {
            await FulfillOrderAsync(orderId, cancellationToken);
        }
    }
}
