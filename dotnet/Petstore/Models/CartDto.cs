namespace Petstore.Models;

public sealed record CartDto(
    IReadOnlyList<CartLineDto> Lines,
    int ItemCount,
    decimal Total,
    string Currency);
