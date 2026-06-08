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
}
