namespace Petstore.Data.Entities;

public sealed class ShipmentLineEntity
{
    public int ShipmentId { get; set; }

    public required string ItemId { get; set; }

    public required int QuantityShipped { get; set; }

    public ShipmentEntity? Shipment { get; set; }
}
