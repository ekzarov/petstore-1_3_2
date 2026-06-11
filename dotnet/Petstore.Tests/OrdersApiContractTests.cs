using System.Net.Http.Headers;
using Petstore.Models;

namespace Petstore.Tests;

[Collection(PetstoreCatalogDatabaseCollection.Name)]
[Trait("Category", DatabaseIntegrationTestCategory.Name)]
public sealed class OrdersApiContractTests(PetstoreCatalogTestsFixture fixture) : DatabaseIntegrationTestBase(fixture)
{
    private static readonly ContactInfoDto Shipping = new(
        "Doe", "Jane", "1 Main Street", null, "Springfield", "IL", "62701", "USA", "jane@example.com", "555-0100");

    private static async Task<HttpClient> SignInClientAsync(CatalogApiFactory factory, string userId, string password)
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/signin", new SignInRequestDto(userId, password));
        var token = (await response.Content.ReadFromJsonAsync<SignInResponseDto>())!.Token;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static async Task AddToCartAsync(HttpClient client, string itemId, int quantity = 1)
    {
        await client.PostAsJsonAsync("/api/cart/items", new AddCartItemRequestDto(itemId));
        if (quantity > 1)
        {
            await client.PutAsJsonAsync($"/api/cart/items/{itemId}", new SetCartQuantityRequestDto(quantity));
        }
    }

    [Fact]
    public async Task Place_Order_Creates_Pending_Order_And_Empties_Cart()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var client = await SignInClientAsync(factory, "j2ee", "j2ee");
        await AddToCartAsync(client, "EST-1", 2);
        await AddToCartAsync(client, "EST-2");

        var response = await client.PostAsJsonAsync("/api/orders", new PlaceOrderRequestDto(Shipping, null));
        var order = await response.Content.ReadFromJsonAsync<OrderDto>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(order);
        Assert.False(string.IsNullOrWhiteSpace(order.OrderId));
        Assert.Equal("APPROVED", order.Status); // below the 500 auto-approval threshold (feature 010)
        Assert.Equal(2, order.Lines.Count);
        Assert.Equal(16.50m * 3, order.Total);
        Assert.Equal("USD", order.Currency);
        Assert.Equal(Shipping, order.ShippingContact);
        Assert.Equal(Shipping, order.BillingContact); // defaults to shipping

        var cart = await client.GetFromJsonAsync<CartDto>("/api/cart");
        Assert.NotNull(cart);
        Assert.Empty(cart.Lines);
    }

    [Fact]
    public async Task Duplicate_Submit_Fails_With_Empty_Cart_Error()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var client = await SignInClientAsync(factory, "j2ee", "j2ee");
        await AddToCartAsync(client, "EST-1");

        var first = await client.PostAsJsonAsync("/api/orders", new PlaceOrderRequestDto(Shipping, null));
        var second = await client.PostAsJsonAsync("/api/orders", new PlaceOrderRequestDto(Shipping, null));
        var error = await second.Content.ReadFromJsonAsync<ApiErrorDto>();

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
        Assert.NotNull(error);
        Assert.Equal("orders.empty_cart", error.Code);
    }

    [Fact]
    public async Task Place_Order_With_Empty_Cart_Is_Rejected()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var client = await SignInClientAsync(factory, "shopper", "j2ee");

        var response = await client.PostAsJsonAsync("/api/orders", new PlaceOrderRequestDto(Shipping, null));
        var error = await response.Content.ReadFromJsonAsync<ApiErrorDto>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(error);
        Assert.Equal("orders.empty_cart", error.Code);
    }

    [Fact]
    public async Task Place_Order_Anonymous_Is_Unauthorized()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/orders", new PlaceOrderRequestDto(Shipping, null));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Place_Order_Validation_Names_Fields()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var client = await SignInClientAsync(factory, "j2ee", "j2ee");
        await AddToCartAsync(client, "EST-1");

        var response = await client.PostAsJsonAsync(
            "/api/orders",
            new PlaceOrderRequestDto(Shipping with { City = "", Email = " " }, null));
        var error = await response.Content.ReadFromJsonAsync<ApiErrorDto>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(error);
        Assert.Equal("orders.validation", error.Code);
        Assert.Contains("shippingContact.city", error.Message);
        Assert.Contains("shippingContact.email", error.Message);

        // Nothing was created and the cart is untouched.
        var orders = await client.GetFromJsonAsync<IReadOnlyList<OrderSummaryDto>>("/api/orders");
        Assert.NotNull(orders);
        Assert.Empty(orders);
        var cart = await client.GetFromJsonAsync<CartDto>("/api/cart");
        Assert.NotNull(cart);
        Assert.Single(cart.Lines);
    }

    [Fact]
    public async Task Order_List_And_Detail_Return_Own_Data()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var client = await SignInClientAsync(factory, "j2ee", "j2ee");
        await AddToCartAsync(client, "EST-1");
        var placed = await client.PostAsJsonAsync("/api/orders", new PlaceOrderRequestDto(Shipping, null));
        var created = await placed.Content.ReadFromJsonAsync<OrderDto>();

        var list = await client.GetFromJsonAsync<IReadOnlyList<OrderSummaryDto>>("/api/orders");
        Assert.NotNull(list);
        var summary = Assert.Single(list);
        Assert.Equal(created!.OrderId, summary.OrderId);
        Assert.Equal("APPROVED", summary.Status);
        Assert.Equal(created.Total, summary.Total);

        var detail = await client.GetFromJsonAsync<OrderDto>($"/api/orders/{created.OrderId}");
        Assert.NotNull(detail);
        Assert.Equal(created.OrderId, detail.OrderId);
        var line = Assert.Single(detail.Lines);
        Assert.Equal("EST-1", line.ItemId);
        Assert.Equal(line.UnitPrice * line.Quantity, line.Subtotal);
    }

    [Fact]
    public async Task Foreign_And_Unknown_Orders_Look_Identical()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var owner = await SignInClientAsync(factory, "j2ee", "j2ee");
        await AddToCartAsync(owner, "EST-1");
        var placed = await owner.PostAsJsonAsync("/api/orders", new PlaceOrderRequestDto(Shipping, null));
        var created = await placed.Content.ReadFromJsonAsync<OrderDto>();

        using var stranger = await SignInClientAsync(factory, "shopper", "j2ee");
        var foreign = await stranger.GetAsync($"/api/orders/{created!.OrderId}");
        var unknown = await stranger.GetAsync("/api/orders/999999");

        Assert.Equal(HttpStatusCode.NotFound, foreign.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, unknown.StatusCode);
        Assert.Equal(
            await foreign.Content.ReadFromJsonAsync<ApiErrorDto>(),
            await unknown.Content.ReadFromJsonAsync<ApiErrorDto>());
    }

    [Fact]
    public async Task Order_List_For_Customer_Without_Orders_Is_Empty()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var client = await SignInClientAsync(factory, "j2ee-ja", "j2ee");

        var orders = await client.GetFromJsonAsync<IReadOnlyList<OrderSummaryDto>>("/api/orders");

        Assert.NotNull(orders);
        Assert.Empty(orders);
    }
}
