namespace Petstore.Data;

public static class CatalogModelConstants
{
    public static class Tables
    {
        public const string Categories = "CatalogCategories";
        public const string Products = "CatalogProducts";
        public const string Items = "CatalogItems";
    }

    public static class Lengths
    {
        public const int Id = 32;
        public const int Name = 128;
        public const int Description = 512;
        public const int Currency = 3;
    }

    public static class Precision
    {
        public const int Price = 18;
        public const int PriceScale = 2;
    }
}
