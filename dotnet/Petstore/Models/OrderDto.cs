namespace Petstore.Models;

public sealed record OrderDto(
    string OrderId,
    DateTime PlacedAt,
    string Status,
    decimal Total,
    string Currency,
    ContactInfoDto ShippingContact,
    ContactInfoDto BillingContact,
    IReadOnlyList<OrderLineDto> Lines);
