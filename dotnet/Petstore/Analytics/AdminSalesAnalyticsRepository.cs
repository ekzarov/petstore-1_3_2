using Microsoft.EntityFrameworkCore;
using Petstore.Data;

namespace Petstore.Analytics;

public sealed class AdminSalesAnalyticsRepository(PetstoreCatalogContext context) : IAdminSalesAnalyticsRepository
{
    public async Task<IReadOnlyList<AdminCategorySalesRow>> GetCategorySalesAsync(
        DateTime fromInclusive,
        DateTime toExclusive,
        CancellationToken cancellationToken)
    {
        // Order lines of qualifying orders in range, mapped to their catalog
        // category via item -> product (plan DD-004). Lines whose item left
        // the catalog fall into the UNKNOWN bucket instead of being dropped.
        var rows = await (
            from line in context.OrderLines
            join order in context.Orders on line.OrderId equals order.Id
            where SalesAnalyticsConstants.QualifyingStatuses.Contains(order.Status)
                && order.PlacedAt >= fromInclusive
                && order.PlacedAt < toExclusive
            join item in context.Items on line.ItemId equals item.Id into itemGroup
            from item in itemGroup.DefaultIfEmpty()
            join product in context.Products on item.ProductId equals product.Id into productGroup
            from product in productGroup.DefaultIfEmpty()
            join category in context.Categories on product.CategoryId equals category.Id into categoryGroup
            from category in categoryGroup.DefaultIfEmpty()
            group new { line.UnitPrice, line.Quantity } by new
            {
                CategoryId = product != null ? product.CategoryId : SalesAnalyticsConstants.UnknownCategoryId,
                CategoryName = category != null ? category.Name : null
            }
            into grouped
            select new AdminCategorySalesRow(
                grouped.Key.CategoryId,
                grouped.Key.CategoryName,
                grouped.Sum(entry => entry.UnitPrice * entry.Quantity),
                grouped.Sum(entry => entry.Quantity)))
            .ToListAsync(cancellationToken);

        return rows;
    }
}
