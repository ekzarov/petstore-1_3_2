using Petstore.Models;

namespace Petstore.Catalog;

public interface ICatalogRepository
{
    Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default);

    Task<CategoryDto?> GetCategoryAsync(string categoryId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProductDto>?> GetProductsByCategoryAsync(
        string categoryId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ItemDto>?> GetItemsByProductAsync(
        string productId,
        CancellationToken cancellationToken = default);

    Task<ItemDto?> GetItemAsync(string itemId, CancellationToken cancellationToken = default);
}
