using Microsoft.AspNetCore.Mvc;
using Petstore.Catalog;
using Petstore.Models;

namespace Petstore.Controllers;

[ApiController]
[Route("api/catalog/items")]
public sealed class CatalogItemsController(ICatalogRepository catalogRepository) : ControllerBase
{
    [HttpGet("{itemId}")]
    [ProducesResponseType<ItemDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorDto>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ItemDto>> GetItemAsync(string itemId, CancellationToken cancellationToken)
    {
        var item = await catalogRepository.GetItemAsync(itemId, cancellationToken);
        if (item is null)
        {
            return NotFound(new ApiErrorDto("catalog.item_not_found", "Item was not found."));
        }

        return Ok(item);
    }
}
