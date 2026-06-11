namespace Petstore.Models;

public sealed record AdminOrderDetailDto(
    string OrderId,
    DateTime PlacedAt,
    string UserId,
    string Status,
    decimal Total,
    string Currency,
    ContactInfoDto ShippingContact,
    ContactInfoDto BillingContact,
    IReadOnlyList<OrderLineDto> Lines);
