namespace Petstore.Data.Entities;

public sealed class CartLineEntity
{
    public required string CartKey { get; set; }

    public required string ItemId { get; set; }

    public required int Quantity { get; set; }

    public CartEntity? Cart { get; set; }
}
