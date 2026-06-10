namespace Petstore.Data;

public static class CartModelConstants
{
    public static class Tables
    {
        public const string Carts = "Carts";
        public const string CartLines = "CartLines";
    }

    public static class Lengths
    {
        public const int CartKey = 128;
    }

    public static class Quantity
    {
        public const int Min = 1;
        public const int Max = 99;
    }
}
