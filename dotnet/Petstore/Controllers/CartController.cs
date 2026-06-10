using Microsoft.AspNetCore.Mvc;
using Petstore.Cart;
using Petstore.Catalog;
using Petstore.Models;

namespace Petstore.Controllers;

[ApiController]
[Route("api/cart")]
public sealed class CartController(
    ICartRepository cartRepository,
    ICatalogRepository catalogRepository,
    CartViewBuilder viewBuilder) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<CartDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<CartDto>> GetCartAsync(CancellationToken cancellationToken)
    {
        var identity = await ResolveAndMergeAsync(cancellationToken);
        if (!identity.HasIdentity)
        {
            return Ok(new CartDto([], 0, 0m, CartViewBuilder.DefaultCurrency));
        }

        var lines = await cartRepository.GetLinesAsync(identity.Key!, cancellationToken);

        return Ok(await viewBuilder.BuildAsync(lines, cancellationToken));
    }

    [HttpPost("items")]
    [ProducesResponseType<CartDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorDto>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiErrorDto>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CartDto>> AddItemAsync(
        AddCartItemRequestDto request,
        CancellationToken cancellationToken)
    {
        var identity = await ResolveAndMergeAsync(cancellationToken);
        if (!identity.HasIdentity)
        {
            return MissingIdentity();
        }

        if (string.IsNullOrWhiteSpace(request.ItemId))
        {
            return BadRequest(new ApiErrorDto("cart.validation", "Missing or invalid fields: itemId."));
        }

        var item = await catalogRepository.GetItemAsync(request.ItemId, cancellationToken);
        if (item is null)
        {
            return NotFound(new ApiErrorDto("cart.item_not_found", "Item was not found."));
        }

        await cartRepository.AddItemAsync(identity.Key!, request.ItemId, cancellationToken);
        var lines = await cartRepository.GetLinesAsync(identity.Key!, cancellationToken);

        return Ok(await viewBuilder.BuildAsync(lines, cancellationToken));
    }

    [HttpPut("items/{itemId}")]
    [ProducesResponseType<CartDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorDto>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiErrorDto>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CartDto>> SetQuantityAsync(
        string itemId,
        SetCartQuantityRequestDto request,
        CancellationToken cancellationToken)
    {
        var identity = await ResolveAndMergeAsync(cancellationToken);
        if (!identity.HasIdentity)
        {
            return MissingIdentity();
        }

        if (!CartRules.IsValidSetQuantity(request.Quantity))
        {
            return BadRequest(new ApiErrorDto(
                "cart.validation",
                $"Quantity must be between 0 and {Data.CartModelConstants.Quantity.Max}."));
        }

        var found = await cartRepository.SetQuantityAsync(identity.Key!, itemId, request.Quantity, cancellationToken);
        if (!found)
        {
            return NotFound(new ApiErrorDto("cart.line_not_found", "Cart line was not found."));
        }

        var lines = await cartRepository.GetLinesAsync(identity.Key!, cancellationToken);

        return Ok(await viewBuilder.BuildAsync(lines, cancellationToken));
    }

    [HttpDelete("items/{itemId}")]
    [ProducesResponseType<CartDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorDto>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CartDto>> RemoveLineAsync(string itemId, CancellationToken cancellationToken)
    {
        var identity = await ResolveAndMergeAsync(cancellationToken);
        if (!identity.HasIdentity)
        {
            return MissingIdentity();
        }

        var found = await cartRepository.RemoveLineAsync(identity.Key!, itemId, cancellationToken);
        if (!found)
        {
            return NotFound(new ApiErrorDto("cart.line_not_found", "Cart line was not found."));
        }

        var lines = await cartRepository.GetLinesAsync(identity.Key!, cancellationToken);

        return Ok(await viewBuilder.BuildAsync(lines, cancellationToken));
    }

    [HttpDelete]
    [ProducesResponseType<CartDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<CartDto>> EmptyAsync(CancellationToken cancellationToken)
    {
        var identity = await ResolveAndMergeAsync(cancellationToken);
        if (!identity.HasIdentity)
        {
            return MissingIdentity();
        }

        await cartRepository.EmptyAsync(identity.Key!, cancellationToken);

        return Ok(new CartDto([], 0, 0m, CartViewBuilder.DefaultCurrency));
    }

    private async Task<CartIdentity> ResolveAndMergeAsync(CancellationToken cancellationToken)
    {
        var identity = CartIdentityResolver.Resolve(HttpContext);
        if (identity.HasIdentity && identity.AnonymousKey is not null)
        {
            await cartRepository.MergeAsync(identity.AnonymousKey, identity.Key!, cancellationToken);
        }

        return identity;
    }

    private ObjectResult MissingIdentity()
    {
        return BadRequest(new ApiErrorDto(
            "cart.missing_identity",
            $"Provide an {CartIdentityResolver.CartIdHeader} header or sign in."));
    }
}
