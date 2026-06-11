using Microsoft.EntityFrameworkCore;
using Petstore.Data;
using Petstore.Data.Entities;

namespace Petstore.OrderProcessing;

public sealed class OrderTransitionRepository(PetstoreCatalogContext context)
{
    /// <summary>
    /// Applies a status transition optimistically: the UPDATE only succeeds
    /// when the order is still in <paramref name="fromStatus"/>, so a
    /// concurrent decision gets zero affected rows and no audit record
    /// (plan DD-006). Returns false for missing orders, wrong current
    /// status, or an illegal transition.
    /// </summary>
    public async Task<bool> TryTransitionAsync(
        int orderId,
        string fromStatus,
        string toStatus,
        string actor,
        CancellationToken cancellationToken)
    {
        if (!OrderWorkflow.IsLegal(fromStatus, toStatus))
        {
            return false;
        }

        var affected = await context.Orders
            .Where(order => order.Id == orderId && order.Status == fromStatus)
            .ExecuteUpdateAsync(setters => setters.SetProperty(order => order.Status, toStatus), cancellationToken);

        if (affected == 0)
        {
            return false;
        }

        context.OrderStatusTransitions.Add(new OrderStatusTransitionEntity
        {
            OrderId = orderId,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            Actor = actor,
            OccurredAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<IReadOnlyList<OrderStatusTransitionEntity>> GetTransitionsAsync(
        int orderId,
        CancellationToken cancellationToken)
    {
        return await context.OrderStatusTransitions
            .AsNoTracking()
            .Where(transition => transition.OrderId == orderId)
            .OrderBy(transition => transition.OccurredAt)
            .ThenBy(transition => transition.Id)
            .ToListAsync(cancellationToken);
    }
}
