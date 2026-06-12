using Petstore.Catalog;

namespace Petstore.Tests;

[Collection(PetstoreCatalogDatabaseCollection.Name)]
[Trait("Category", DatabaseIntegrationTestCategory.Name)]
public sealed class CatalogRepositoryTests(PetstoreCatalogTestsFixture fixture) : DatabaseIntegrationTestBase(fixture)
{
    [Fact]
    public async Task GetCategoriesAsync_Returns_Seeded_Categories_Ordered_By_Name()
    {
        await using var context = Fixture.CreateContext();
        var repository = new CatalogRepository(context);

        var categories = await repository.GetCategoriesAsync();

        Assert.Equal(["Birds", "Cats", "Dogs", "Fish", "Reptiles"], categories.Select(category => category.Name));
    }

    [Fact]
    public async Task GetProductsByCategoryAsync_Returns_Products_For_Known_Category()
    {
        await using var context = Fixture.CreateContext();
        var repository = new CatalogRepository(context);

        var products = await repository.GetProductsByCategoryAsync("DOGS");

        Assert.NotNull(products);
        Assert.Equal(
            ["K9-BD-01", "K9-CW-01", "K9-DL-01", "K9-RT-01", "K9-RT-02", "K9-PO-02"],
            products.Select(product => product.Id));
    }

    [Fact]
    public async Task GetProductsByCategoryAsync_Returns_Null_For_Unknown_Category()
    {
        await using var context = Fixture.CreateContext();
        var repository = new CatalogRepository(context);

        var products = await repository.GetProductsByCategoryAsync("UNKNOWN");

        Assert.Null(products);
    }

    [Fact]
    public async Task GetItemsByProductAsync_Returns_Seeded_Items_Ordered_By_Name()
    {
        await using var context = Fixture.CreateContext();
        var repository = new CatalogRepository(context);

        var items = await repository.GetItemsByProductAsync("FI-SW-01");

        Assert.NotNull(items);
        Assert.Equal(["EST-1", "EST-2"], items.Select(item => item.Id));
    }

    [Fact]
    public async Task GetItemsByProductAsync_Returns_Seeded_Items_For_Non_Fish_Product()
    {
        await using var context = Fixture.CreateContext();
        var repository = new CatalogRepository(context);

        var items = await repository.GetItemsByProductAsync("K9-BD-01");

        Assert.NotNull(items);
        Assert.Equal(["EST-7", "EST-6"], items.Select(item => item.Id));
    }

    [Fact]
    public async Task GetItemsByProductAsync_Returns_Null_For_Unknown_Product()
    {
        await using var context = Fixture.CreateContext();
        var repository = new CatalogRepository(context);

        var items = await repository.GetItemsByProductAsync("UNKNOWN");

        Assert.Null(items);
    }

    [Fact]
    public async Task GetItemAsync_Returns_Item_For_Known_Id()
    {
        await using var context = Fixture.CreateContext();
        var repository = new CatalogRepository(context);

        var item = await repository.GetItemAsync("EST-1");

        Assert.NotNull(item);
        Assert.Equal("Large Angelfish", item.Name);
        Assert.Equal("FI-SW-01", item.ProductId);
        Assert.Equal(["Large", "Cuddly"], item.Attributes);
        Assert.Equal(16.50m, item.Price);
    }

    [Fact]
    public async Task GetItemAsync_Returns_Null_For_Unknown_Id()
    {
        await using var context = Fixture.CreateContext();
        var repository = new CatalogRepository(context);

        var item = await repository.GetItemAsync("UNKNOWN");

        Assert.Null(item);
    }
}
