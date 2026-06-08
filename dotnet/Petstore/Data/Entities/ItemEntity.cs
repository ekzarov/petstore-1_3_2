namespace Petstore.Data.Entities;

public sealed class ItemEntity
{
    public required string Id { get; set; }

    public required string ProductId { get; set; }

    public required string Name { get; set; }

    public List<string> Attributes { get; set; } = [];

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public required string Currency { get; set; }

    public ProductEntity? Product { get; set; }
}
