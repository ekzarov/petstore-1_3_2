namespace Petstore.Data.Entities;

public sealed class OrderLineEntity
{
    public int OrderId { get; set; }

    public required string ItemId { get; set; }

    public required string Name { get; set; }

    public required decimal UnitPrice { get; set; }

    public required string Currency { get; set; }

    public required int Quantity { get; set; }

    public int QuantityShipped { get; set; }

    public OrderEntity? Order { get; set; }
}
