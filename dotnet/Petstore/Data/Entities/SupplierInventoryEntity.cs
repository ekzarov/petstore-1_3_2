namespace Petstore.Data.Entities;

public sealed class SupplierInventoryEntity
{
    public required string ItemId { get; set; }

    public required int QuantityOnHand { get; set; }
}
