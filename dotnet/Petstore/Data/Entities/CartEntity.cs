namespace Petstore.Data.Entities;

public sealed class CartEntity
{
    public required string CartKey { get; set; }

    public required DateTime UpdatedAt { get; set; }

    public List<CartLineEntity> Lines { get; set; } = [];
}
