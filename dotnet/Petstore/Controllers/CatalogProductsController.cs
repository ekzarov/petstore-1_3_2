using Microsoft.AspNetCore.Mvc;
using Petstore.Catalog;
using Petstore.Models;

namespace Petstore.Controllers;

[ApiController]
[Route("api/catalog/products")]
public sealed class CatalogProductsController(ICatalogRepository catalogRepository) : ControllerBase
{
    [HttpGet("{productId}/items")]
    [ProducesResponseType<IReadOnlyList<ItemDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorDto>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ItemDto>>> GetItemsByProductAsync(
        string productId,
        CancellationToken cancellationToken)
    {
        var items = await catalogRepository.GetItemsByProductAsync(productId, cancellationToken);
        if (items is null)
        {
            return NotFound(new ApiErrorDto("catalog.product_not_found", "Product was not found."));
        }

        return Ok(items);
    }
}
