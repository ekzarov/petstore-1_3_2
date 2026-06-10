using Petstore.Models;

namespace Petstore.Tests;

[Collection(PetstoreCatalogDatabaseCollection.Name)]
[Trait("Category", DatabaseIntegrationTestCategory.Name)]
public sealed class CartApiContractTests(PetstoreCatalogTestsFixture fixture) : DatabaseIntegrationTestBase(fixture)
{
    private const string CartHeader = "X-Cart-Id";

    private static HttpClient CreateAnonymousClient(CatalogApiFactory factory, string cartId)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(CartHeader, cartId);
        return client;
    }

    [Fact]
    public async Task Add_Item_Creates_Line_With_Catalog_Price()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var client = CreateAnonymousClient(factory, Guid.NewGuid().ToString());

        var response = await client.PostAsJsonAsync("/api/cart/items", new AddCartItemRequestDto("EST-1"));
        var cart = await response.Content.ReadFromJsonAsync<CartDto>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(cart);
        var line = Assert.Single(cart.Lines);
        Assert.Equal("EST-1", line.ItemId);
        Assert.Equal("Large Angelfish", line.Name);
        Assert.Equal(16.50m, line.UnitPrice);
        Assert.Equal("USD", line.Currency);
        Assert.Equal(1, line.Quantity);
        Assert.Equal(16.50m, line.Subtotal);
        Assert.Equal(1, cart.ItemCount);
        Assert.Equal(16.50m, cart.Total);
    }

    [Fact]
    public async Task Duplicate_Add_Merges_Into_One_Line()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var client = CreateAnonymousClient(factory, Guid.NewGuid().ToString());

        await client.PostAsJsonAsync("/api/cart/items", new AddCartItemRequestDto("EST-1"));
        var response = await client.PostAsJsonAsync("/api/cart/items", new AddCartItemRequestDto("EST-1"));
        var cart = await response.Content.ReadFromJsonAsync<CartDto>();

        Assert.NotNull(cart);
        var line = Assert.Single(cart.Lines);
        Assert.Equal(2, line.Quantity);
        Assert.Equal(33.00m, cart.Total);
    }

    [Fact]
    public async Task Add_Unknown_Item_Returns_404_And_Leaves_Cart_Unchanged()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var client = CreateAnonymousClient(factory, Guid.NewGuid().ToString());

        await client.PostAsJsonAsync("/api/cart/items", new AddCartItemRequestDto("EST-1"));
        var response = await client.PostAsJsonAsync("/api/cart/items", new AddCartItemRequestDto("NOPE"));
        var error = await response.Content.ReadFromJsonAsync<ApiErrorDto>();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.NotNull(error);
        Assert.Equal("cart.item_not_found", error.Code);

        var cart = await client.GetFromJsonAsync<CartDto>("/api/cart");
        Assert.NotNull(cart);
        Assert.Single(cart.Lines);
    }

    [Fact]
    public async Task Get_Cart_With_Multiple_Items_Computes_Totals()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var client = CreateAnonymousClient(factory, Guid.NewGuid().ToString());

        await client.PostAsJsonAsync("/api/cart/items", new AddCartItemRequestDto("EST-1"));
        await client.PostAsJsonAsync("/api/cart/items", new AddCartItemRequestDto("EST-2"));
        await client.PutAsJsonAsync("/api/cart/items/EST-2", new SetCartQuantityRequestDto(3));

        var cart = await client.GetFromJsonAsync<CartDto>("/api/cart");

        Assert.NotNull(cart);
        Assert.Equal(2, cart.Lines.Count);
        Assert.All(cart.Lines, line => Assert.Equal(line.UnitPrice * line.Quantity, line.Subtotal));
        Assert.Equal(cart.Lines.Sum(line => line.Subtotal), cart.Total);
        Assert.Equal(4, cart.ItemCount);
    }

    [Fact]
    public async Task Get_Cart_Without_Identity_Returns_Empty_Cart()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var client = factory.CreateClient();

        var cart = await client.GetFromJsonAsync<CartDto>("/api/cart");

        Assert.NotNull(cart);
        Assert.Empty(cart.Lines);
        Assert.Equal(0, cart.ItemCount);
        Assert.Equal(0m, cart.Total);
    }

    [Fact]
    public async Task Write_Without_Identity_Returns_400()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/cart/items", new AddCartItemRequestDto("EST-1"));
        var error = await response.Content.ReadFromJsonAsync<ApiErrorDto>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(error);
        Assert.Equal("cart.missing_identity", error.Code);
    }

    [Fact]
    public async Task Set_Quantity_Zero_Removes_Line()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var client = CreateAnonymousClient(factory, Guid.NewGuid().ToString());

        await client.PostAsJsonAsync("/api/cart/items", new AddCartItemRequestDto("EST-1"));
        var response = await client.PutAsJsonAsync("/api/cart/items/EST-1", new SetCartQuantityRequestDto(0));
        var cart = await response.Content.ReadFromJsonAsync<CartDto>();

        Assert.NotNull(cart);
        Assert.Empty(cart.Lines);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(100)]
    public async Task Set_Quantity_Out_Of_Bounds_Is_Rejected(int quantity)
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var client = CreateAnonymousClient(factory, Guid.NewGuid().ToString());

        await client.PostAsJsonAsync("/api/cart/items", new AddCartItemRequestDto("EST-1"));
        var response = await client.PutAsJsonAsync("/api/cart/items/EST-1", new SetCartQuantityRequestDto(quantity));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var cart = await client.GetFromJsonAsync<CartDto>("/api/cart");
        Assert.NotNull(cart);
        Assert.Equal(1, Assert.Single(cart.Lines).Quantity);
    }

    [Fact]
    public async Task Remove_Line_And_Empty_Cart_Work()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var client = CreateAnonymousClient(factory, Guid.NewGuid().ToString());

        await client.PostAsJsonAsync("/api/cart/items", new AddCartItemRequestDto("EST-1"));
        await client.PostAsJsonAsync("/api/cart/items", new AddCartItemRequestDto("EST-2"));

        var afterRemove = await client.DeleteAsync("/api/cart/items/EST-1");
        var cart = await afterRemove.Content.ReadFromJsonAsync<CartDto>();
        Assert.NotNull(cart);
        Assert.Equal("EST-2", Assert.Single(cart.Lines).ItemId);

        var afterEmpty = await client.DeleteAsync("/api/cart");
        var empty = await afterEmpty.Content.ReadFromJsonAsync<CartDto>();
        Assert.NotNull(empty);
        Assert.Empty(empty.Lines);
    }

    [Fact]
    public async Task Update_Unknown_Line_Returns_404()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var client = CreateAnonymousClient(factory, Guid.NewGuid().ToString());

        var response = await client.PutAsJsonAsync("/api/cart/items/EST-1", new SetCartQuantityRequestDto(2));
        var error = await response.Content.ReadFromJsonAsync<ApiErrorDto>();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.NotNull(error);
        Assert.Equal("cart.line_not_found", error.Code);
    }

    [Fact]
    public async Task Carts_Are_Isolated_Per_Identity_And_Persist_Across_Requests()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        var cartId = Guid.NewGuid().ToString();

        using (var client = CreateAnonymousClient(factory, cartId))
        {
            await client.PostAsJsonAsync("/api/cart/items", new AddCartItemRequestDto("EST-1"));
        }

        using (var sameIdentity = CreateAnonymousClient(factory, cartId))
        {
            var cart = await sameIdentity.GetFromJsonAsync<CartDto>("/api/cart");
            Assert.NotNull(cart);
            Assert.Single(cart.Lines);
        }

        using (var otherIdentity = CreateAnonymousClient(factory, Guid.NewGuid().ToString()))
        {
            var cart = await otherIdentity.GetFromJsonAsync<CartDto>("/api/cart");
            Assert.NotNull(cart);
            Assert.Empty(cart.Lines);
        }
    }
}
