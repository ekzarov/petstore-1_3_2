namespace Petstore.Data.Entities;

public sealed class ShipmentEntity
{
    public int Id { get; set; }

    public required int OrderId { get; set; }

    public required DateTime OccurredAt { get; set; }

    public List<ShipmentLineEntity> Lines { get; set; } = [];
}
