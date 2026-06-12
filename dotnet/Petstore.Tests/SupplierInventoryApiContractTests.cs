using System.Net.Http.Headers;
using Petstore.Models;

namespace Petstore.Tests;

[Collection(PetstoreCatalogDatabaseCollection.Name)]
[Trait("Category", DatabaseIntegrationTestCategory.Name)]
public sealed class SupplierInventoryApiContractTests(PetstoreCatalogTestsFixture fixture) : DatabaseIntegrationTestBase(fixture)
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
    public async Task Inventory_Is_Seeded_And_Readable_By_Supplier()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var supplier = await SignInClientAsync(factory, "supplier", "supplier");

        var inventory = await supplier.GetFromJsonAsync<IReadOnlyList<InventoryItemDto>>("/api/supplier/inventory");

        Assert.NotNull(inventory);
        Assert.Contains(inventory, item => item.ItemId == "EST-1");
        Assert.Contains(inventory, item => item.ItemId == "EST-2");
        Assert.All(inventory, item => Assert.Equal(100, item.QuantityOnHand));
    }

    [Fact]
    public async Task Admin_Remains_A_Superuser_For_Inventory()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var admin = await SignInClientAsync(factory, "admin", "admin");

        var list = await admin.GetAsync("/api/supplier/inventory");
        var set = await admin.PutAsJsonAsync("/api/supplier/inventory/EST-1", new SetInventoryRequestDto(42));
        var run = await admin.PostAsync("/api/supplier/fulfillment/run", null);

        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        Assert.Equal(HttpStatusCode.OK, set.StatusCode);
        Assert.Equal(HttpStatusCode.OK, run.StatusCode);
    }

    [Fact]
    public async Task Supplier_Set_Inventory_Persists_And_Negative_Is_Rejected()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var supplier = await SignInClientAsync(factory, "supplier", "supplier");

        var set = await supplier.PutAsJsonAsync("/api/supplier/inventory/EST-1", new SetInventoryRequestDto(7));
        var updated = await set.Content.ReadFromJsonAsync<InventoryItemDto>();
        Assert.Equal(HttpStatusCode.OK, set.StatusCode);
        Assert.NotNull(updated);
        Assert.Equal(7, updated.QuantityOnHand);

        var negative = await supplier.PutAsJsonAsync("/api/supplier/inventory/EST-1", new SetInventoryRequestDto(-1));
        var error = await negative.Content.ReadFromJsonAsync<ApiErrorDto>();
        Assert.Equal(HttpStatusCode.BadRequest, negative.StatusCode);
        Assert.NotNull(error);
        Assert.Equal("inventory.validation", error.Code);

        var inventory = await supplier.GetFromJsonAsync<IReadOnlyList<InventoryItemDto>>("/api/supplier/inventory");
        Assert.NotNull(inventory);
        Assert.Equal(7, inventory.Single(item => item.ItemId == "EST-1").QuantityOnHand);
    }

    [Fact]
    public async Task Customer_And_Anonymous_Are_Rejected()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var customer = await SignInClientAsync(factory, "j2ee", "j2ee");

        var customerGet = await customer.GetAsync("/api/supplier/inventory");
        var customerSet = await customer.PutAsJsonAsync("/api/supplier/inventory/EST-1", new SetInventoryRequestDto(1));
        var customerRun = await customer.PostAsync("/api/supplier/fulfillment/run", null);
        Assert.Equal(HttpStatusCode.Forbidden, customerGet.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, customerSet.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, customerRun.StatusCode);

        using var anonymous = factory.CreateClient();
        var anonymousGet = await anonymous.GetAsync("/api/supplier/inventory");
        Assert.Equal(HttpStatusCode.Unauthorized, anonymousGet.StatusCode);
    }

    [Fact]
    public async Task Old_Admin_Inventory_Routes_Are_Removed()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var admin = await SignInClientAsync(factory, "admin", "admin");

        var oldList = await admin.GetAsync("/api/admin/inventory");
        var oldRun = await admin.PostAsync("/api/admin/fulfillment/run", null);

        Assert.Equal(HttpStatusCode.NotFound, oldList.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, oldRun.StatusCode);
    }
}
