using Microsoft.AspNetCore.Mvc;
using Petstore.Catalog;
using Petstore.Models;

namespace Petstore.Controllers;

[ApiController]
[Route("api/catalog/categories")]
public sealed class CatalogCategoriesController(ICatalogRepository catalogRepository) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<CategoryDto>>(StatusCodes.Status200OK)]
    public async Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken)
    {
        return await catalogRepository.GetCategoriesAsync(cancellationToken);
    }

    [HttpGet("{categoryId}/products")]
    [ProducesResponseType<IReadOnlyList<ProductDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorDto>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetProductsByCategoryAsync(
        string categoryId,
        CancellationToken cancellationToken)
    {
        var products = await catalogRepository.GetProductsByCategoryAsync(categoryId, cancellationToken);
        if (products is null)
        {
            return NotFound(new ApiErrorDto("catalog.category_not_found", "Category was not found."));
        }

        return Ok(products);
    }
}
