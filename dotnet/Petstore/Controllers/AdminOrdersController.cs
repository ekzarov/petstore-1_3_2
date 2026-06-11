using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Petstore.Data;
using Petstore.Models;
using Petstore.OrderProcessing;
using Petstore.Orders;

namespace Petstore.Controllers;

[ApiController]
[Route("api/admin/orders")]
[Authorize(Roles = AccountModelConstants.Roles.Admin)]
public sealed class AdminOrdersController(
    PetstoreCatalogContext context,
    IOrderProcessingService processingService,
    OrderTransitionRepository transitionRepository) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<AdminOrderSummaryDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorDto>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<AdminOrderSummaryDto>>> GetOrdersAsync(
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        if (status is not null && !OrderStatus.All.Contains(status))
        {
            return BadRequest(new ApiErrorDto(
                "orders.invalid_status",
                $"Unknown status. Valid values: {string.Join(", ", OrderStatus.All)}."));
        }

        var query = context.Orders.AsNoTracking();
        if (status is not null)
        {
            query = query.Where(order => order.Status == status);
        }

        var orders = await query
            .OrderByDescending(order => order.PlacedAt)
            .Select(order => new AdminOrderSummaryDto(
                order.Id.ToString(),
                order.PlacedAt,
                order.UserId,
                order.Total,
                order.Currency,
                order.Status))
            .ToListAsync(cancellationToken);

        return Ok(orders);
    }

    [HttpPost("{orderId}/approve")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorDto>(StatusCodes.Status409Conflict)]
    public Task<ActionResult> ApproveAsync(string orderId, CancellationToken cancellationToken)
    {
        return DecideAsync(orderId, approve: true, cancellationToken);
    }

    [HttpPost("{orderId}/deny")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorDto>(StatusCodes.Status409Conflict)]
    public Task<ActionResult> DenyAsync(string orderId, CancellationToken cancellationToken)
    {
        return DecideAsync(orderId, approve: false, cancellationToken);
    }

    [HttpGet("{orderId}/transitions")]
    [ProducesResponseType<IReadOnlyList<OrderTransitionDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<OrderTransitionDto>>> GetTransitionsAsync(
        string orderId,
        CancellationToken cancellationToken)
    {
        if (!int.TryParse(orderId, out var id))
        {
            return Ok(Array.Empty<OrderTransitionDto>());
        }

        var transitions = await transitionRepository.GetTransitionsAsync(id, cancellationToken);

        return Ok(transitions
            .Select(t => new OrderTransitionDto(t.FromStatus, t.ToStatus, t.Actor, t.OccurredAt))
            .ToList());
    }

    private async Task<ActionResult> DecideAsync(string orderId, bool approve, CancellationToken cancellationToken)
    {
        if (!int.TryParse(orderId, out var id))
        {
            return InvalidTransition();
        }

        var actor = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? OrderWorkflow.SystemActor;
        var succeeded = approve
            ? await processingService.ApproveAsync(id, actor, cancellationToken)
            : await processingService.DenyAsync(id, actor, cancellationToken);

        return succeeded ? Ok() : InvalidTransition();
    }

    private ObjectResult InvalidTransition()
    {
        return Conflict(new ApiErrorDto(
            "orders.invalid_transition",
            "The order does not exist or is not in a state that allows this decision."));
    }
}
