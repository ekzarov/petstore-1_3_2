using Petstore.Data.Entities;
using Petstore.Models;

namespace Petstore.Orders;

public static class OrderPlacementRules
{
    /// <summary>
    /// Freezes cart lines into order lines using current catalog data.
    /// Returns the ids of cart items that no longer exist in the catalog.
    /// </summary>
    public static (IReadOnlyList<OrderLineEntity> Lines, IReadOnlyList<string> MissingItemIds) FreezeLines(
        IReadOnlyList<CartLineEntity> cartLines,
        IReadOnlyDictionary<string, ItemDto> catalogItems)
    {
        var lines = new List<OrderLineEntity>(cartLines.Count);
        var missing = new List<string>();

        foreach (var cartLine in cartLines)
        {
            if (!catalogItems.TryGetValue(cartLine.ItemId, out var item))
            {
                missing.Add(cartLine.ItemId);
                continue;
            }

            lines.Add(new OrderLineEntity
            {
                ItemId = item.Id,
                Name = item.Name,
                UnitPrice = item.Price,
                Currency = item.Currency,
                Quantity = cartLine.Quantity
            });
        }

        return (lines, missing);
    }

    public static decimal ComputeTotal(IReadOnlyList<OrderLineEntity> lines)
    {
        return lines.Sum(line => line.UnitPrice * line.Quantity);
    }

    public static OrderContactBlock ToContactBlock(ContactInfoDto contact)
    {
        return new OrderContactBlock
        {
            FamilyName = contact.FamilyName,
            GivenName = contact.GivenName,
            Street1 = contact.Street1,
            Street2 = contact.Street2,
            City = contact.City,
            State = contact.State,
            Zip = contact.Zip,
            Country = contact.Country,
            Email = contact.Email,
            Phone = contact.Phone
        };
    }
}
