using Microsoft.EntityFrameworkCore;
using Petstore.Data;
using Petstore.Data.Entities;

namespace Petstore.Supplier;

public sealed class InventoryRepository(PetstoreCatalogContext context) : IInventoryRepository
{
    public async Task<IReadOnlyList<SupplierInventoryEntity>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await context.SupplierInventory
            .AsNoTracking()
            .OrderBy(inventory => inventory.ItemId)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetOnHandAsync(string itemId, CancellationToken cancellationToken)
    {
        // A missing inventory row reads as zero stock (spec edge case).
        return await context.SupplierInventory
            .AsNoTracking()
            .Where(inventory => inventory.ItemId == itemId)
            .Select(inventory => (int?)inventory.QuantityOnHand)
            .SingleOrDefaultAsync(cancellationToken) ?? 0;
    }

    public async Task SetQuantityAsync(string itemId, int quantity, CancellationToken cancellationToken)
    {
        var existing = await context.SupplierInventory
            .SingleOrDefaultAsync(inventory => inventory.ItemId == itemId, cancellationToken);
        if (existing is null)
        {
            context.SupplierInventory.Add(new SupplierInventoryEntity { ItemId = itemId, QuantityOnHand = quantity });
        }
        else
        {
            existing.QuantityOnHand = quantity;
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> TryDecrementAsync(string itemId, int take, CancellationToken cancellationToken)
    {
        var affected = await context.SupplierInventory
            .Where(inventory => inventory.ItemId == itemId && inventory.QuantityOnHand >= take)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(
                    inventory => inventory.QuantityOnHand,
                    inventory => inventory.QuantityOnHand - take),
                cancellationToken);

        return affected == 1;
    }
}
