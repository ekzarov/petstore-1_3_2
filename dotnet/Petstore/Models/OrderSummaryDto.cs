namespace Petstore.Models;

public sealed record OrderSummaryDto(
    string OrderId,
    DateTime PlacedAt,
    decimal Total,
    string Currency,
    string Status);
