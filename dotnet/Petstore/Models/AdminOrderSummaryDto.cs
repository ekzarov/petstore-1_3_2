namespace Petstore.Models;

public sealed record AdminOrderSummaryDto(
    string OrderId,
    DateTime PlacedAt,
    string UserId,
    decimal Total,
    string Currency,
    string Status);
