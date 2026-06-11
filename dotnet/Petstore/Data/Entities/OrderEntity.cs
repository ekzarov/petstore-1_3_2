namespace Petstore.Data.Entities;

public sealed class OrderEntity
{
    public int Id { get; set; }

    public required string UserId { get; set; }

    public required DateTime PlacedAt { get; set; }

    public required string Status { get; set; }

    public required string Currency { get; set; }

    public required decimal Total { get; set; }

    public required OrderContactBlock ShippingContact { get; set; }

    public required OrderContactBlock BillingContact { get; set; }

    public List<OrderLineEntity> Lines { get; set; } = [];
}
