namespace Petstore.Models;

public sealed record ItemDto(
    string Id,
    string ProductId,
    string Name,
    IReadOnlyList<string> Attributes,
    string? Description,
    decimal Price,
    string Currency);
