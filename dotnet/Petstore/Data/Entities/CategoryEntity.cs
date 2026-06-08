namespace Petstore.Data.Entities;

public sealed class CategoryEntity
{
    public required string Id { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public List<ProductEntity> Products { get; set; } = [];
}
