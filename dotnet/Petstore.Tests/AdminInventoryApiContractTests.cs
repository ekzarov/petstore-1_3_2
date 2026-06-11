using System.Net.Http.Headers;
using Petstore.Models;

namespace Petstore.Tests;

[Collection(PetstoreCatalogDatabaseCollection.Name)]
[Trait("Category", DatabaseIntegrationTestCategory.Name)]
public sealed class AdminInventoryApiContractTests(PetstoreCatalogTestsFixture fixture) : DatabaseIntegrationTestBase(fixture)
{
    private static async Task<HttpClient> SignInClientAsync(CatalogApiFactory factory, string userId, string password)
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/signin", new SignInRequestDto(userId, password));
        var token = (await response.Content.ReadFromJsonAsync<SignInResponseDto>())!.Token;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    [Fact]
    public async Task Inventory_Is_Seeded_For_All_Catalog_Items()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var admin = await SignInClientAsync(factory, "admin", "admin");

        var inventory = await admin.GetFromJsonAsync<IReadOnlyList<InventoryItemDto>>("/api/admin/inventory");

        Assert.NotNull(inventory);
        Assert.Contains(inventory, item => item.ItemId == "EST-1");
        Assert.Contains(inventory, item => item.ItemId == "EST-2");
        Assert.All(inventory, item => Assert.Equal(100, item.QuantityOnHand));
    }

    [Fact]
    public async Task Set_Inventory_Persists_And_Negative_Is_Rejected()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var admin = await SignInClientAsync(factory, "admin", "admin");

        var set = await admin.PutAsJsonAsync("/api/admin/inventory/EST-1", new SetInventoryRequestDto(7));
        var updated = await set.Content.ReadFromJsonAsync<InventoryItemDto>();
        Assert.Equal(HttpStatusCode.OK, set.StatusCode);
        Assert.NotNull(updated);
        Assert.Equal(7, updated.QuantityOnHand);

        var negative = await admin.PutAsJsonAsync("/api/admin/inventory/EST-1", new SetInventoryRequestDto(-1));
        var error = await negative.Content.ReadFromJsonAsync<ApiErrorDto>();
        Assert.Equal(HttpStatusCode.BadRequest, negative.StatusCode);
        Assert.NotNull(error);
        Assert.Equal("inventory.validation", error.Code);

        var inventory = await admin.GetFromJsonAsync<IReadOnlyList<InventoryItemDto>>("/api/admin/inventory");
        Assert.NotNull(inventory);
        Assert.Equal(7, inventory.Single(item => item.ItemId == "EST-1").QuantityOnHand);
    }

    [Fact]
    public async Task Customer_And_Anonymous_Are_Rejected()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var customer = await SignInClientAsync(factory, "j2ee", "j2ee");

        var customerGet = await customer.GetAsync("/api/admin/inventory");
        var customerSet = await customer.PutAsJsonAsync("/api/admin/inventory/EST-1", new SetInventoryRequestDto(1));
        var customerRun = await customer.PostAsync("/api/admin/fulfillment/run", null);
        Assert.Equal(HttpStatusCode.Forbidden, customerGet.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, customerSet.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, customerRun.StatusCode);

        using var anonymous = factory.CreateClient();
        var anonymousGet = await anonymous.GetAsync("/api/admin/inventory");
        Assert.Equal(HttpStatusCode.Unauthorized, anonymousGet.StatusCode);
    }
}
