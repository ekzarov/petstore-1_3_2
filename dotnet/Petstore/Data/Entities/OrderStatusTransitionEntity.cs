namespace Petstore.Data.Entities;

public sealed class OrderStatusTransitionEntity
{
    public int Id { get; set; }

    public required int OrderId { get; set; }

    public required string FromStatus { get; set; }

    public required string ToStatus { get; set; }

    public required string Actor { get; set; }

    public required DateTime OccurredAt { get; set; }
}
