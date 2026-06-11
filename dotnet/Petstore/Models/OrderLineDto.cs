namespace Petstore.Models;

public sealed record OrderLineDto(
    string ItemId,
    string Name,
    decimal UnitPrice,
    string Currency,
    int Quantity,
    decimal Subtotal);
