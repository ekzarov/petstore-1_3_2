namespace Petstore.Models;

public sealed record CartLineDto(
    string ItemId,
    string Name,
    decimal UnitPrice,
    string Currency,
    int Quantity,
    decimal Subtotal,
    bool Unavailable);
