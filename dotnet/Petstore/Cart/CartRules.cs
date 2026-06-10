using Petstore.Data;

namespace Petstore.Cart;

public static class CartRules
{
    public static bool IsValidSetQuantity(int quantity)
    {
        // 0 means remove; 1..Max are valid amounts.
        return quantity >= 0 && quantity <= CartModelConstants.Quantity.Max;
    }

    public static int MergeQuantities(int existing, int added)
    {
        return Math.Min(existing + added, CartModelConstants.Quantity.Max);
    }
}
