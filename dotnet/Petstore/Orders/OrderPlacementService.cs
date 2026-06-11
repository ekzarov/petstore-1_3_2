using Microsoft.EntityFrameworkCore;
using Petstore.Catalog;
using Petstore.Data;
using Petstore.Data.Entities;
using Petstore.Models;

namespace Petstore.Orders;

public sealed record PlacementResult(
    OrderEntity? Order,
    string? ErrorCode,
    string? ErrorMessage)
{
    public bool Succeeded => Order is not null;

    public static PlacementResult Success(OrderEntity order) => new(order, null, null);

    public static PlacementResult Failure(string code, string message) => new(null, code, message);
}

public sealed class OrderPlacementService(
    PetstoreCatalogContext context,
    ICatalogRepository catalogRepository)
{
    public async Task<PlacementResult> PlaceOrderAsync(
        string userId,
        ContactInfoDto shippingContact,
        ContactInfoDto? billingContact,
        CancellationToken cancellationToken)
    {
        var cartKey = $"user:{userId}";
        var cartLines = await context.CartLines
            .Where(line => line.CartKey == cartKey)
            .ToListAsync(cancellationToken);

        if (cartLines.Count == 0)
        {
            return PlacementResult.Failure("orders.empty_cart", "The cart is empty; there is nothing to order.");
        }

        var catalogItems = new Dictionary<string, ItemDto>();
        foreach (var cartLine in cartLines)
        {
            var item = await catalogRepository.GetItemAsync(cartLine.ItemId, cancellationToken);
            if (item is not null)
            {
                catalogItems[item.Id] = item;
            }
        }

        var (frozenLines, missingItemIds) = OrderPlacementRules.FreezeLines(cartLines, catalogItems);
        if (missingItemIds.Count > 0)
        {
            return PlacementResult.Failure(
                "orders.items_unavailable",
                $"These items are no longer available: {string.Join(", ", missingItemIds)}.");
        }

        var order = new OrderEntity
        {
            UserId = userId,
            PlacedAt = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            Currency = frozenLines[0].Currency,
            Total = OrderPlacementRules.ComputeTotal(frozenLines),
            ShippingContact = OrderPlacementRules.ToContactBlock(shippingContact),
            BillingContact = OrderPlacementRules.ToContactBlock(billingContact ?? shippingContact),
            Lines = [.. frozenLines]
        };

        // One SaveChanges = one transaction: the order insert and the cart
        // emptying either both happen or neither does. A duplicate submit
        // finds an empty cart and fails with orders.empty_cart (DD-002).
        context.Orders.Add(order);
        context.CartLines.RemoveRange(cartLines);
        await context.SaveChangesAsync(cancellationToken);

        return PlacementResult.Success(order);
    }
}
