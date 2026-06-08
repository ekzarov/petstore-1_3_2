namespace Petstore.Models;

public sealed record CategoryDto(
    string Id,
    string Name,
    string? Description = null);
