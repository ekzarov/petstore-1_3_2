using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using Petstore.Models;

namespace Petstore.Tests;

[Collection(PetstoreCatalogDatabaseCollection.Name)]
[Trait("Category", DatabaseIntegrationTestCategory.Name)]
public sealed class OrderPlacementTransactionTests(PetstoreCatalogTestsFixture fixture) : DatabaseIntegrationTestBase(fixture)
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

    [Fact]
    public async Task Order_Prices_Stay_Frozen_When_Catalog_Price_Changes()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var client = await SignInClientAsync(factory, "j2ee", "j2ee");
        await client.PostAsJsonAsync("/api/cart/items", new AddCartItemRequestDto("EST-1"));
        var placed = await client.PostAsJsonAsync("/api/orders", new PlaceOrderRequestDto(Shipping, null));
        var created = await placed.Content.ReadFromJsonAsync<OrderDto>();
        Assert.NotNull(created);

        // Change the catalog price after placement.
        await using (var context = Fixture.CreateContext())
        {
            await context.Items
                .Where(item => item.Id == "EST-1")
                .ExecuteUpdateAsync(setters => setters.SetProperty(item => item.Price, 99.99m));
        }

        var reread = await client.GetFromJsonAsync<OrderDto>($"/api/orders/{created.OrderId}");

        Assert.NotNull(reread);
        Assert.Equal(16.50m, Assert.Single(reread.Lines).UnitPrice);
        Assert.Equal(created.Total, reread.Total);
    }

    [Fact]
    public async Task Stored_Order_Is_Self_Contained_For_Downstream_Processing()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var client = await SignInClientAsync(factory, "j2ee", "j2ee");
        await client.PostAsJsonAsync("/api/cart/items", new AddCartItemRequestDto("EST-2"));
        var placed = await client.PostAsJsonAsync("/api/orders", new PlaceOrderRequestDto(Shipping, null));
        var created = await placed.Content.ReadFromJsonAsync<OrderDto>();
        Assert.NotNull(created);

        await using var context = Fixture.CreateContext();
        var stored = await context.Orders
            .AsNoTracking()
            .Include(order => order.Lines)
            .SingleAsync(order => order.Id == int.Parse(created.OrderId));

        Assert.Equal("j2ee", stored.UserId);
        Assert.Equal("COMPLETED", stored.Status); // auto-approved and fulfilled (features 010+011)
        Assert.Equal("USD", stored.Currency);
        Assert.True(stored.Total > 0);
        Assert.True(stored.PlacedAt > DateTime.UtcNow.AddMinutes(-5));
        Assert.Equal("Doe", stored.ShippingContact.FamilyName);
        Assert.Equal("Doe", stored.BillingContact.FamilyName);
        var line = Assert.Single(stored.Lines);
        Assert.Equal("EST-2", line.ItemId);
        Assert.Equal("Small Angelfish", line.Name);
        Assert.Equal(16.50m, line.UnitPrice);
        Assert.Equal(1, line.Quantity);
    }

    [Fact]
    public async Task Failed_Placement_Leaves_Cart_Intact_And_Creates_Nothing()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var client = await SignInClientAsync(factory, "j2ee", "j2ee");
        await client.PostAsJsonAsync("/api/cart/items", new AddCartItemRequestDto("EST-1"));

        // Remove the item from the catalog so freezing fails mid-placement.
        await using (var context = Fixture.CreateContext())
        {
            await context.Items.Where(item => item.Id == "EST-1").ExecuteDeleteAsync();
        }

        var response = await client.PostAsJsonAsync("/api/orders", new PlaceOrderRequestDto(Shipping, null));
        var error = await response.Content.ReadFromJsonAsync<ApiErrorDto>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(error);
        Assert.Equal("orders.items_unavailable", error.Code);
        Assert.Contains("EST-1", error.Message);

        await using var verify = Fixture.CreateContext();
        Assert.Equal(0, await verify.Orders.CountAsync());
        Assert.Equal(1, await verify.CartLines.CountAsync(line => line.CartKey == "user:j2ee"));
    }
}
