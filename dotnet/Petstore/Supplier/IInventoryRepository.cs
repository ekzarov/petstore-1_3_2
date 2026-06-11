using Petstore.Data.Entities;

namespace Petstore.Supplier;

public interface IInventoryRepository
{
    Task<IReadOnlyList<SupplierInventoryEntity>> GetAllAsync(CancellationToken cancellationToken);

    Task<int> GetOnHandAsync(string itemId, CancellationToken cancellationToken);

    Task SetQuantityAsync(string itemId, int quantity, CancellationToken cancellationToken);

    /// <summary>
    /// Concurrency-safe decrement: succeeds only when at least
    /// <paramref name="take"/> units are still on hand.
    /// </summary>
    Task<bool> TryDecrementAsync(string itemId, int take, CancellationToken cancellationToken);
}
