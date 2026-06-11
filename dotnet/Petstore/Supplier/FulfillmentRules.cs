namespace Petstore.Supplier;

public enum ShipmentOutcome
{
    NothingShipped,
    PartiallyShipped,
    FullyShipped
}

public static class FulfillmentRules
{
    /// <summary>Quantity to ship now: never more than remaining, never more than on hand, never negative.</summary>
    public static int TakeQuantity(int remaining, int onHand)
    {
        return Math.Max(0, Math.Min(remaining, onHand));
    }

    /// <summary>
    /// Classifies an order after a fulfillment pass from its line states
    /// (ordered quantity, shipped-so-far quantity).
    /// </summary>
    public static ShipmentOutcome Classify(IReadOnlyList<(int Quantity, int QuantityShipped)> lines)
    {
        if (lines.All(line => line.QuantityShipped >= line.Quantity))
        {
            return ShipmentOutcome.FullyShipped;
        }

        return lines.Any(line => line.QuantityShipped > 0)
            ? ShipmentOutcome.PartiallyShipped
            : ShipmentOutcome.NothingShipped;
    }
}
