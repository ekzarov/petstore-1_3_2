namespace Petstore.Models;

public sealed record ProductDto(
    string Id,
    string CategoryId,
    string Name,
    string? Description = null);
