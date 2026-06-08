using Microsoft.EntityFrameworkCore;
using Petstore.Data;
using Petstore.Models;

namespace Petstore.Catalog;

public sealed class CatalogRepository(PetstoreCatalogContext context) : ICatalogRepository
{
    public async Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await context.Categories
            .AsNoTracking()
            .OrderBy(category => category.Name)
            .Select(category => new CategoryDto(category.Id, category.Name, category.Description))
            .ToListAsync(cancellationToken);
    }

    public async Task<CategoryDto?> GetCategoryAsync(string categoryId, CancellationToken cancellationToken = default)
    {
        return await context.Categories
            .AsNoTracking()
            .Where(category => category.Id == categoryId)
            .Select(category => new CategoryDto(category.Id, category.Name, category.Description))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ProductDto>?> GetProductsByCategoryAsync(
        string categoryId,
        CancellationToken cancellationToken = default)
    {
        var categoryExists = await context.Categories
            .AsNoTracking()
            .AnyAsync(category => category.Id == categoryId, cancellationToken);

        if (!categoryExists)
        {
            return null;
        }

        return await context.Products
            .AsNoTracking()
            .Where(product => product.CategoryId == categoryId)
            .OrderBy(product => product.Name)
            .Select(product => new ProductDto(product.Id, product.CategoryId, product.Name, product.Description))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ItemDto>?> GetItemsByProductAsync(
        string productId,
        CancellationToken cancellationToken = default)
    {
        var productExists = await context.Products
            .AsNoTracking()
            .AnyAsync(product => product.Id == productId, cancellationToken);

        if (!productExists)
        {
            return null;
        }

        return await context.Items
            .AsNoTracking()
            .Where(item => item.ProductId == productId)
            .OrderBy(item => item.Name)
            .Select(item => new ItemDto(
                item.Id,
                item.ProductId,
                item.Name,
                item.Attributes,
                item.Description,
                item.Price,
                item.Currency))
            .ToListAsync(cancellationToken);
    }

    public async Task<ItemDto?> GetItemAsync(string itemId, CancellationToken cancellationToken = default)
    {
        return await context.Items
            .AsNoTracking()
            .Where(item => item.Id == itemId)
            .Select(item => new ItemDto(
                item.Id,
                item.ProductId,
                item.Name,
                item.Attributes,
                item.Description,
                item.Price,
                item.Currency))
            .SingleOrDefaultAsync(cancellationToken);
    }
}
