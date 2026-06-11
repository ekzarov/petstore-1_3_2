using Petstore.Supplier;

namespace Petstore.Tests;

public sealed class FulfillmentRulesTests
{
    [Theory]
    [InlineData(5, 10, 5)]   // enough stock: ship all remaining
    [InlineData(10, 5, 5)]   // limited stock: ship what is on hand
    [InlineData(5, 0, 0)]    // no stock: ship nothing
    [InlineData(0, 10, 0)]   // nothing remaining: ship nothing
    [InlineData(5, -3, 0)]   // corrupt negative stock reads as zero
    public void TakeQuantity_Ships_Min_Of_Remaining_And_OnHand(int remaining, int onHand, int expected)
    {
        Assert.Equal(expected, FulfillmentRules.TakeQuantity(remaining, onHand));
    }

    [Fact]
    public void Classify_FullyShipped_When_All_Lines_Complete()
    {
        Assert.Equal(
            ShipmentOutcome.FullyShipped,
            FulfillmentRules.Classify([(2, 2), (3, 3)]));
    }

    [Fact]
    public void Classify_PartiallyShipped_When_Some_Quantity_Shipped()
    {
        Assert.Equal(
            ShipmentOutcome.PartiallyShipped,
            FulfillmentRules.Classify([(2, 2), (3, 1)]));
    }

    [Fact]
    public void Classify_NothingShipped_When_No_Quantity_Shipped()
    {
        Assert.Equal(
            ShipmentOutcome.NothingShipped,
            FulfillmentRules.Classify([(2, 0), (3, 0)]));
    }
}
