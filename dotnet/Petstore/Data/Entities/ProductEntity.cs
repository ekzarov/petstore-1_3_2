namespace Petstore.Data.Entities;

public sealed class ProductEntity
{
    public required string Id { get; set; }

    public required string CategoryId { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public CategoryEntity? Category { get; set; }

    public List<ItemEntity> Items { get; set; } = [];
}
