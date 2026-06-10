using Petstore.Catalog;
using Petstore.Data.Entities;
using Petstore.Models;

namespace Petstore.Cart;

public sealed class CartViewBuilder(ICatalogRepository catalogRepository)
{
    public const string DefaultCurrency = "USD";

    public async Task<CartDto> BuildAsync(IReadOnlyList<CartLineEntity> lines, CancellationToken cancellationToken)
    {
        var dtos = new List<CartLineDto>(lines.Count);
        foreach (var line in lines)
        {
            var item = await catalogRepository.GetItemAsync(line.ItemId, cancellationToken);
            if (item is null)
            {
                // The item left the catalog after it was added: keep the line
                // visible but flag it and exclude it from the total.
                dtos.Add(new CartLineDto(line.ItemId, line.ItemId, 0m, DefaultCurrency, line.Quantity, 0m, true));
                continue;
            }

            var subtotal = item.Price * line.Quantity;
            dtos.Add(new CartLineDto(item.Id, item.Name, item.Price, item.Currency, line.Quantity, subtotal, false));
        }

        var available = dtos.Where(dto => !dto.Unavailable).ToList();
        var total = available.Sum(dto => dto.Subtotal);
        var itemCount = available.Sum(dto => dto.Quantity);
        var currency = available.Select(dto => dto.Currency).FirstOrDefault() ?? DefaultCurrency;

        return new CartDto(dtos, itemCount, total, currency);
    }
}
