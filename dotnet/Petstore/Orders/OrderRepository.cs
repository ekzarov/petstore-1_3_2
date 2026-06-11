using Microsoft.EntityFrameworkCore;
using Petstore.Data;
using Petstore.Data.Entities;

namespace Petstore.Orders;

public sealed class OrderRepository(PetstoreCatalogContext context) : IOrderRepository
{
    public async Task<IReadOnlyList<OrderEntity>> GetOrdersByUserAsync(string userId, CancellationToken cancellationToken)
    {
        return await context.Orders
            .AsNoTracking()
            .Where(order => order.UserId == userId)
            .OrderByDescending(order => order.PlacedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<OrderEntity?> GetOrderAsync(int orderId, string userId, CancellationToken cancellationToken)
    {
        return context.Orders
            .AsNoTracking()
            .Include(order => order.Lines)
            .SingleOrDefaultAsync(order => order.Id == orderId && order.UserId == userId, cancellationToken);
    }
}
