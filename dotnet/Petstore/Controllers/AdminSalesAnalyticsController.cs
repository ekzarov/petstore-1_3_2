using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Petstore.Analytics;
using Petstore.Data;
using Petstore.Models;

namespace Petstore.Controllers;

[ApiController]
[Route("api/admin/analytics")]
[Authorize(Roles = AccountModelConstants.Roles.Admin)]
public sealed class AdminSalesAnalyticsController(IAdminSalesAnalyticsService analyticsService) : ControllerBase
{
    [HttpGet("sales")]
    [ProducesResponseType<AdminSalesAnalyticsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorDto>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AdminSalesAnalyticsDto>> GetSalesAsync(
        [FromQuery] string? startDate,
        [FromQuery] string? endDate,
        CancellationToken cancellationToken)
    {
        var request = new SalesAnalyticsDateRangeRequest(startDate, endDate);
        var validationError = request.TryParse(out var start, out var end);
        if (validationError is not null)
        {
            return BadRequest(new ApiErrorDto("analytics.validation", validationError));
        }

        return Ok(await analyticsService.GetSalesAnalyticsAsync(start, end, cancellationToken));
    }
}
