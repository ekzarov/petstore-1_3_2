using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Petstore.Models;
using Petstore.Supplier;

namespace Petstore.Controllers;

[ApiController]
[Route("api/supplier")]
[Authorize(Policy = "SupplierOperations")]
public sealed class SupplierInventoryController(
    IInventoryRepository inventoryRepository,
    IFulfillmentService fulfillmentService) : ControllerBase
{
    [HttpGet("inventory")]
    [ProducesResponseType<IReadOnlyList<InventoryItemDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<InventoryItemDto>>> GetInventoryAsync(CancellationToken cancellationToken)
    {
        var inventory = await inventoryRepository.GetAllAsync(cancellationToken);

        return Ok(inventory
            .Select(item => new InventoryItemDto(item.ItemId, item.QuantityOnHand))
            .ToList());
    }

    [HttpPut("inventory/{itemId}")]
    [ProducesResponseType<InventoryItemDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorDto>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<InventoryItemDto>> SetInventoryAsync(
        string itemId,
        SetInventoryRequestDto request,
        CancellationToken cancellationToken)
    {
        if (request.Quantity < 0)
        {
            return BadRequest(new ApiErrorDto("inventory.validation", "Quantity must be zero or greater."));
        }

        await inventoryRepository.SetQuantityAsync(itemId, request.Quantity, cancellationToken);

        // Replenishment automatically re-runs fulfillment for affected orders
        // (013 DD-003, matching the legacy supplier submit flow).
        await fulfillmentService.FulfillOrdersForItemAsync(itemId, cancellationToken);

        var onHand = await inventoryRepository.GetOnHandAsync(itemId, cancellationToken);

        return Ok(new InventoryItemDto(itemId, onHand));
    }

    [HttpPost("fulfillment/run")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> RunFulfillmentAsync(CancellationToken cancellationToken)
    {
        // Operational recovery action; blind mode returns only the count (013 DD-004).
        var processed = await fulfillmentService.FulfillAllEligibleAsync(cancellationToken);

        return Ok(new { processed });
    }
}
