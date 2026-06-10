using Microsoft.EntityFrameworkCore;
using Petstore.Data;
using Petstore.Data.Entities;

namespace Petstore.Cart;

public sealed class CartRepository(PetstoreCatalogContext context) : ICartRepository
{
    public async Task<IReadOnlyList<CartLineEntity>> GetLinesAsync(string cartKey, CancellationToken cancellationToken)
    {
        return await context.CartLines
            .AsNoTracking()
            .Where(line => line.CartKey == cartKey)
            .OrderBy(line => line.ItemId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddItemAsync(string cartKey, string itemId, CancellationToken cancellationToken)
    {
        await EnsureCartAsync(cartKey, cancellationToken);

        var line = await context.CartLines
            .SingleOrDefaultAsync(l => l.CartKey == cartKey && l.ItemId == itemId, cancellationToken);
        if (line is null)
        {
            context.CartLines.Add(new CartLineEntity { CartKey = cartKey, ItemId = itemId, Quantity = 1 });
        }
        else
        {
            line.Quantity = CartRules.MergeQuantities(line.Quantity, 1);
        }

        await TouchAsync(cartKey, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> SetQuantityAsync(string cartKey, string itemId, int quantity, CancellationToken cancellationToken)
    {
        var line = await context.CartLines
            .SingleOrDefaultAsync(l => l.CartKey == cartKey && l.ItemId == itemId, cancellationToken);
        if (line is null)
        {
            return false;
        }

        if (quantity == 0)
        {
            context.CartLines.Remove(line);
        }
        else
        {
            line.Quantity = quantity;
        }

        await TouchAsync(cartKey, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> RemoveLineAsync(string cartKey, string itemId, CancellationToken cancellationToken)
    {
        return await SetQuantityAsync(cartKey, itemId, 0, cancellationToken);
    }

    public async Task EmptyAsync(string cartKey, CancellationToken cancellationToken)
    {
        await context.CartLines.Where(line => line.CartKey == cartKey).ExecuteDeleteAsync(cancellationToken);
    }

    public async Task MergeAsync(string anonymousKey, string userKey, CancellationToken cancellationToken)
    {
        var anonymousLines = await context.CartLines
            .Where(line => line.CartKey == anonymousKey)
            .ToListAsync(cancellationToken);
        if (anonymousLines.Count == 0)
        {
            return;
        }

        await EnsureCartAsync(userKey, cancellationToken);
        var userLines = await context.CartLines
            .Where(line => line.CartKey == userKey)
            .ToDictionaryAsync(line => line.ItemId, cancellationToken);

        foreach (var anonymousLine in anonymousLines)
        {
            if (userLines.TryGetValue(anonymousLine.ItemId, out var userLine))
            {
                userLine.Quantity = CartRules.MergeQuantities(userLine.Quantity, anonymousLine.Quantity);
            }
            else
            {
                context.CartLines.Add(new CartLineEntity
                {
                    CartKey = userKey,
                    ItemId = anonymousLine.ItemId,
                    Quantity = anonymousLine.Quantity
                });
            }
        }

        context.CartLines.RemoveRange(anonymousLines);
        var anonymousCart = await context.Carts.SingleOrDefaultAsync(cart => cart.CartKey == anonymousKey, cancellationToken);
        if (anonymousCart is not null)
        {
            context.Carts.Remove(anonymousCart);
        }

        await TouchAsync(userKey, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureCartAsync(string cartKey, CancellationToken cancellationToken)
    {
        var exists = await context.Carts.AnyAsync(cart => cart.CartKey == cartKey, cancellationToken);
        if (!exists)
        {
            context.Carts.Add(new CartEntity { CartKey = cartKey, UpdatedAt = DateTime.UtcNow });
        }
    }

    private async Task TouchAsync(string cartKey, CancellationToken cancellationToken)
    {
        var cart = await context.Carts.SingleOrDefaultAsync(c => c.CartKey == cartKey, cancellationToken);
        if (cart is not null)
        {
            cart.UpdatedAt = DateTime.UtcNow;
        }
    }
}
