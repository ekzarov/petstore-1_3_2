using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Petstore.Accounts;
using Petstore.Data.Entities;
using Petstore.Models;
using Petstore.Orders;

namespace Petstore.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public sealed class OrdersController(
    OrderPlacementService placementService,
    IOrderRepository orderRepository) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<OrderDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ApiErrorDto>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<OrderDto>> PlaceOrderAsync(
        PlaceOrderRequestDto request,
        CancellationToken cancellationToken)
    {
        var missing = new List<string>();
        if (request.ShippingContact is null)
        {
            missing.Add("shippingContact");
        }
        else
        {
            missing.AddRange(ContactValidation.MissingContactFields(request.ShippingContact)
                .Select(field => $"shippingContact.{field[8..]}".Replace("..", ".")));
        }

        if (request.BillingContact is not null)
        {
            missing.AddRange(ContactValidation.MissingContactFields(request.BillingContact)
                .Select(field => $"billingContact.{field[8..]}".Replace("..", ".")));
        }

        if (missing.Count > 0)
        {
            return BadRequest(new ApiErrorDto(
                "orders.validation",
                $"Missing or invalid fields: {string.Join(", ", missing)}."));
        }

        var result = await placementService.PlaceOrderAsync(
            CurrentUserId(),
            request.ShippingContact!,
            request.BillingContact,
            cancellationToken);

        if (!result.Succeeded)
        {
            return BadRequest(new ApiErrorDto(result.ErrorCode!, result.ErrorMessage!));
        }

        var dto = ToDto(result.Order!);

        return Created($"/api/orders/{dto.OrderId}", dto);
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<OrderSummaryDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<OrderSummaryDto>>> GetOrdersAsync(CancellationToken cancellationToken)
    {
        var orders = await orderRepository.GetOrdersByUserAsync(CurrentUserId(), cancellationToken);

        return Ok(orders
            .Select(order => new OrderSummaryDto(
                order.Id.ToString(),
                order.PlacedAt,
                order.Total,
                order.Currency,
                order.Status))
            .ToList());
    }

    [HttpGet("{orderId}")]
    [ProducesResponseType<OrderDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorDto>(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<OrderDto>> GetOrderAsync(string orderId, CancellationToken cancellationToken)
    {
        // Foreign and unknown ids look identical (no ownership disclosure).
        if (!int.TryParse(orderId, out var id))
        {
            return OrderNotFound();
        }

        var order = await orderRepository.GetOrderAsync(id, CurrentUserId(), cancellationToken);
        if (order is null)
        {
            return OrderNotFound();
        }

        return Ok(ToDto(order));
    }

    private ObjectResult OrderNotFound()
    {
        return NotFound(new ApiErrorDto("orders.not_found", "Order was not found."));
    }

    private string CurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Authenticated request without a user id claim.");
    }

    private static OrderDto ToDto(OrderEntity order)
    {
        return new OrderDto(
            order.Id.ToString(),
            order.PlacedAt,
            order.Status,
            order.Total,
            order.Currency,
            ToContactDto(order.ShippingContact),
            ToContactDto(order.BillingContact),
            order.Lines
                .OrderBy(line => line.ItemId)
                .Select(line => new OrderLineDto(
                    line.ItemId,
                    line.Name,
                    line.UnitPrice,
                    line.Currency,
                    line.Quantity,
                    line.UnitPrice * line.Quantity))
                .ToList());
    }

    private static ContactInfoDto ToContactDto(OrderContactBlock contact)
    {
        return new ContactInfoDto(
            contact.FamilyName,
            contact.GivenName,
            contact.Street1,
            contact.Street2,
            contact.City,
            contact.State,
            contact.Zip,
            contact.Country,
            contact.Email,
            contact.Phone);
    }
}
