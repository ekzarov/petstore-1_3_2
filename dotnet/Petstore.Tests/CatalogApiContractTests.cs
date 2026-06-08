using System.Globalization;
using System.Xml.Linq;
using Petstore.Models;

namespace Petstore.Tests;

[Collection(PetstoreCatalogDatabaseCollection.Name)]
[Trait("Category", DatabaseIntegrationTestCategory.Name)]
public sealed class CatalogApiContractTests(PetstoreCatalogTestsFixture fixture) : DatabaseIntegrationTestBase(fixture)
{
    [Fact]
    public async Task Get_Catalog_Categories_Returns_Seeded_Categories()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/catalog/categories");
        var categories = await response.Content.ReadFromJsonAsync<IReadOnlyList<CategoryDto>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(categories);
        Assert.Equal(["BIRDS", "CATS", "DOGS", "FISH", "REPTILES"], categories.Select(category => category.Id));
        Assert.Equal(["Birds", "Cats", "Dogs", "Fish", "Reptiles"], categories.Select(category => category.Name));
        Assert.All(categories, category => Assert.Null(category.Description));
    }

    [Fact]
    public async Task Get_Catalog_Category_Products_Returns_Fish_Products()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/catalog/categories/FISH/products");
        var products = await response.Content.ReadFromJsonAsync<IReadOnlyList<ProductDto>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(products);
        Assert.Equal(["FI-SW-01", "FI-FW-02"], products.Select(product => product.Id));
        Assert.All(products, product => Assert.Equal("FISH", product.CategoryId));

        var angelfish = Assert.Single(products, product => product.Id == "FI-SW-01");
        Assert.Equal("Angelfish", angelfish.Name);
        Assert.Equal("Salt Water fish from Australia", angelfish.Description);

        var goldfish = Assert.Single(products, product => product.Id == "FI-FW-02");
        Assert.Equal("Goldfish", goldfish.Name);
        Assert.Equal("Fresh Water fish from China", goldfish.Description);
    }

    [Fact]
    public async Task Get_Catalog_Product_Items_Returns_Angelfish_Items()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/catalog/products/FI-SW-01/items");
        var items = await response.Content.ReadFromJsonAsync<IReadOnlyList<ItemDto>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(items);
        Assert.Equal(["EST-1", "EST-2"], items.Select(item => item.Id));
        Assert.All(items, item => Assert.Equal("FI-SW-01", item.ProductId));

        var largeAngelfish = Assert.Single(items, item => item.Id == "EST-1");
        Assert.Equal("Large Angelfish", largeAngelfish.Name);
        Assert.Equal(["Large", "Cuddly"], largeAngelfish.Attributes);
        Assert.Equal(16.50m, largeAngelfish.Price);
        Assert.Equal("USD", largeAngelfish.Currency);

        var smallAngelfish = Assert.Single(items, item => item.Id == "EST-2");
        Assert.Equal("Small Angelfish", smallAngelfish.Name);
        Assert.Equal(["Small"], smallAngelfish.Attributes);
        Assert.Equal(16.50m, smallAngelfish.Price);
        Assert.Equal("USD", smallAngelfish.Currency);
    }

    [Fact]
    public async Task Get_Catalog_Item_Returns_Large_Angelfish()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/catalog/items/EST-1");
        var item = await response.Content.ReadFromJsonAsync<ItemDto>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(item);
        Assert.Equal("EST-1", item.Id);
        Assert.Equal("FI-SW-01", item.ProductId);
        Assert.Equal("Large Angelfish", item.Name);
        Assert.Equal(["Large", "Cuddly"], item.Attributes);
        Assert.Equal("Fresh Water fish from Japan", item.Description);
        Assert.Equal(16.50m, item.Price);
        Assert.Equal("USD", item.Currency);
    }

    [Fact]
    public async Task Fish_Angelfish_Path_Matches_Legacy_Xml_Parity_Anchors()
    {
        var legacy = LegacyCatalogAnchor.Load();
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var client = factory.CreateClient();

        var categories = await client.GetFromJsonAsync<IReadOnlyList<CategoryDto>>("/api/catalog/categories");
        var products = await client.GetFromJsonAsync<IReadOnlyList<ProductDto>>("/api/catalog/categories/FISH/products");
        var items = await client.GetFromJsonAsync<IReadOnlyList<ItemDto>>("/api/catalog/products/FI-SW-01/items");

        Assert.NotNull(categories);
        Assert.NotNull(products);
        Assert.NotNull(items);

        var fish = Assert.Single(categories, category => category.Id == legacy.CategoryId);
        Assert.Equal(legacy.CategoryName, fish.Name);

        var angelfish = Assert.Single(products, product => product.Id == legacy.ProductId);
        Assert.Equal(legacy.CategoryId, angelfish.CategoryId);
        Assert.Equal(legacy.ProductName, angelfish.Name);
        Assert.Equal(legacy.ProductDescription, angelfish.Description);

        AssertLegacyItem(legacy.Items["EST-1"], Assert.Single(items, item => item.Id == "EST-1"));
        AssertLegacyItem(legacy.Items["EST-2"], Assert.Single(items, item => item.Id == "EST-2"));
    }

    private static void AssertLegacyItem(LegacyCatalogItem expected, ItemDto actual)
    {
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.ProductId, actual.ProductId);
        Assert.Equal(expected.DisplayName, actual.Name);
        Assert.Equal(expected.Attributes, actual.Attributes);
        Assert.Equal(expected.Description, actual.Description);
        Assert.Equal(expected.Price, actual.Price);
        Assert.Equal("USD", actual.Currency);
    }

    private sealed record LegacyCatalogAnchor(
        string CategoryId,
        string CategoryName,
        string ProductId,
        string ProductName,
        string ProductDescription,
        IReadOnlyDictionary<string, LegacyCatalogItem> Items)
    {
        public static LegacyCatalogAnchor Load()
        {
            var document = XDocument.Load(GetLegacyCatalogPath());
            var catalog = document.Root?.Element("Catalog")
                ?? throw new InvalidOperationException("Legacy catalog XML does not contain a Catalog element.");

            var category = catalog.Element("Categories")?
                .Elements("Category")
                .Single(element => (string?)element.Attribute("id") == "FISH")
                ?? throw new InvalidOperationException("Legacy catalog XML does not contain category FISH.");

            var product = catalog.Element("Products")?
                .Elements("Product")
                .Single(element => (string?)element.Attribute("id") == "FI-SW-01")
                ?? throw new InvalidOperationException("Legacy catalog XML does not contain product FI-SW-01.");

            var categoryName = GetEnglishDetails(category, "CategoryDetails").Element("Name")?.Value
                ?? throw new InvalidOperationException("Legacy category FISH does not contain an English name.");
            var productDetails = GetEnglishDetails(product, "ProductDetails");
            var productName = productDetails.Element("Name")?.Value
                ?? throw new InvalidOperationException("Legacy product FI-SW-01 does not contain an English name.");
            var productDescription = productDetails.Element("Description")?.Value
                ?? throw new InvalidOperationException("Legacy product FI-SW-01 does not contain an English description.");

            var items = catalog.Element("Items")?
                .Elements("Item")
                .Where(element => (string?)element.Attribute("product") == "FI-SW-01")
                .Where(element => ((string?)element.Attribute("id")) is "EST-1" or "EST-2")
                .Select(element => LegacyCatalogItem.FromXml(element, productName))
                .ToDictionary(item => item.Id)
                ?? throw new InvalidOperationException("Legacy catalog XML does not contain Angelfish items.");

            return new LegacyCatalogAnchor("FISH", categoryName, "FI-SW-01", productName, productDescription, items);
        }

        private static XElement GetEnglishDetails(XElement parent, string detailsElementName)
        {
            return parent.Elements(detailsElementName)
                .Single(element => (string?)element.Attribute(XNamespace.Xml + "lang") == "en-US");
        }

        private static string GetLegacyCatalogPath()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory is not null)
            {
                var path = Path.Combine(
                    directory.FullName,
                    "src",
                    "apps",
                    "petstore",
                    "src",
                    "docroot",
                    "populate",
                    "Populate-UTF8.xml");

                if (File.Exists(path))
                {
                    return path;
                }

                directory = directory.Parent;
            }

            throw new FileNotFoundException("Could not locate legacy Populate-UTF8.xml from test output directory.");
        }
    }

    private sealed record LegacyCatalogItem(
        string Id,
        string ProductId,
        string DisplayName,
        IReadOnlyList<string> Attributes,
        string Description,
        decimal Price)
    {
        public static LegacyCatalogItem FromXml(XElement element, string productName)
        {
            var id = (string?)element.Attribute("id")
                ?? throw new InvalidOperationException("Legacy item does not contain an id.");
            var productId = (string?)element.Attribute("product")
                ?? throw new InvalidOperationException($"Legacy item {id} does not contain a product id.");
            var details = element.Elements("ItemDetails")
                .Single(detailsElement => (string?)detailsElement.Attribute(XNamespace.Xml + "lang") == "en-US");
            var attributes = details.Elements("Attribute").Select(attribute => attribute.Value).ToArray();
            var description = details.Element("Description")?.Value
                ?? throw new InvalidOperationException($"Legacy item {id} does not contain an English description.");
            var price = decimal.Parse(
                details.Element("ListPrice")?.Value
                ?? throw new InvalidOperationException($"Legacy item {id} does not contain a list price."),
                CultureInfo.InvariantCulture);

            return new LegacyCatalogItem(id, productId, $"{attributes[0]} {productName}", attributes, description, price);
        }
    }
}
