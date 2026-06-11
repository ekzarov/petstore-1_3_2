using Microsoft.EntityFrameworkCore;
using Petstore.Data;
using Petstore.Data.Entities;

namespace Petstore.Supplier;

public static class InventorySeeder
{
    public const int DefaultQuantityPerItem = 100;

    /// <summary>Seeds 100 units for every catalog item lacking an inventory row (plan DD-003). Idempotent.</summary>
    public static async Task SeedAsync(PetstoreCatalogContext context, CancellationToken cancellationToken = default)
    {
        var seededIds = await context.SupplierInventory.Select(inventory => inventory.ItemId).ToListAsync(cancellationToken);
        var missingIds = await context.Items
            .Select(item => item.Id)
            .Where(id => !seededIds.Contains(id))
            .ToListAsync(cancellationToken);

        foreach (var itemId in missingIds)
        {
            context.SupplierInventory.Add(new SupplierInventoryEntity
            {
                ItemId = itemId,
                QuantityOnHand = DefaultQuantityPerItem
            });
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
