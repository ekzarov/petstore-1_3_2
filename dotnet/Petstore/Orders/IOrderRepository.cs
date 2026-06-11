using Petstore.Data.Entities;

namespace Petstore.Orders;

public interface IOrderRepository
{
    Task<IReadOnlyList<OrderEntity>> GetOrdersByUserAsync(string userId, CancellationToken cancellationToken);

    Task<OrderEntity?> GetOrderAsync(int orderId, string userId, CancellationToken cancellationToken);
}
