using Petstore.Cart;

namespace Petstore.Tests;

public sealed class CartRulesTests
{
    [Theory]
    [InlineData(0, true)]
    [InlineData(1, true)]
    [InlineData(50, true)]
    [InlineData(99, true)]
    [InlineData(-1, false)]
    [InlineData(100, false)]
    public void IsValidSetQuantity_Enforces_Bounds(int quantity, bool expected)
    {
        Assert.Equal(expected, CartRules.IsValidSetQuantity(quantity));
    }

    [Theory]
    [InlineData(1, 1, 2)]
    [InlineData(98, 1, 99)]
    [InlineData(99, 1, 99)]
    [InlineData(60, 60, 99)]
    public void MergeQuantities_Sums_And_Caps_At_Max(int existing, int added, int expected)
    {
        Assert.Equal(expected, CartRules.MergeQuantities(existing, added));
    }
}
