using System.Net.Http.Headers;
using Petstore.Models;

namespace Petstore.Tests;

[Collection(PetstoreCatalogDatabaseCollection.Name)]
[Trait("Category", DatabaseIntegrationTestCategory.Name)]
public sealed class SupplierRoleApiContractTests(PetstoreCatalogTestsFixture fixture) : DatabaseIntegrationTestBase(fixture)
{
    private static readonly ContactInfoDto Shipping = new(
        "Doe", "Jane", "1 Main Street", null, "Springfield", "IL", "62701", "USA", "jane@example.com", "555-0100");

    private static async Task<(HttpClient Client, SignInResponseDto Identity)> SignInAsync(
        CatalogApiFactory factory, string userId, string password)
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/signin", new SignInRequestDto(userId, password));
        var identity = (await response.Content.ReadFromJsonAsync<SignInResponseDto>())!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", identity.Token);
        return (client, identity);
    }

    [Fact]
    public async Task Supplier_Signs_In_With_Supplier_Role_And_Admin_Stays_Admin()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);

        var (supplierClient, supplier) = await SignInAsync(factory, "supplier", "supplier");
        var (adminClient, admin) = await SignInAsync(factory, "admin", "admin");
        supplierClient.Dispose();
        adminClient.Dispose();

        Assert.Equal("supplier", supplier.UserId);
        Assert.Equal("supplier", supplier.Role);
        Assert.Equal("admin", admin.Role);
    }

    [Fact]
    public async Task Supplier_Is_Forbidden_On_Every_Admin_Order_Endpoint()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);

        // Create a pending order so the endpoints would have real data to leak.
        var (customer, _) = await SignInAsync(factory, "j2ee", "j2ee");
        await customer.PostAsJsonAsync("/api/cart/items", new AddCartItemRequestDto("EST-1"));
        await customer.PutAsJsonAsync("/api/cart/items/EST-1", new SetCartQuantityRequestDto(31));
        var placed = await customer.PostAsJsonAsync("/api/orders", new PlaceOrderRequestDto(Shipping, null));
        var order = (await placed.Content.ReadFromJsonAsync<OrderDto>())!;
        customer.Dispose();

        var (supplier, _) = await SignInAsync(factory, "supplier", "supplier");
        using var _supplier = supplier;

        var attempts = new[]
        {
            await supplier.GetAsync("/api/admin/orders"),
            await supplier.GetAsync("/api/admin/orders?status=PENDING"),
            await supplier.GetAsync($"/api/admin/orders/{order.OrderId}"),
            await supplier.GetAsync($"/api/admin/orders/{order.OrderId}/transitions"),
            await supplier.PostAsync($"/api/admin/orders/{order.OrderId}/approve", null),
            await supplier.PostAsync($"/api/admin/orders/{order.OrderId}/deny", null)
        };

        Assert.All(attempts, response => Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode));

        // No decision leaked through: the order is still pending for the admin.
        var (admin, _) = await SignInAsync(factory, "admin", "admin");
        using var _admin = admin;
        var pending = await admin.GetFromJsonAsync<IReadOnlyList<AdminOrderSummaryDto>>("/api/admin/orders?status=PENDING");
        Assert.NotNull(pending);
        Assert.Contains(pending, o => o.OrderId == order.OrderId);
    }

    [Fact]
    public async Task Admin_Order_Administration_Still_Works_After_The_Split()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);

        var (customer, _) = await SignInAsync(factory, "j2ee", "j2ee");
        using var _customer = customer;
        await customer.PostAsJsonAsync("/api/cart/items", new AddCartItemRequestDto("EST-1"));
        await customer.PutAsJsonAsync("/api/cart/items/EST-1", new SetCartQuantityRequestDto(31));
        var placed = await customer.PostAsJsonAsync("/api/orders", new PlaceOrderRequestDto(Shipping, null));
        var order = (await placed.Content.ReadFromJsonAsync<OrderDto>())!;

        var (admin, _) = await SignInAsync(factory, "admin", "admin");
        using var _admin = admin;
        var approve = await admin.PostAsync($"/api/admin/orders/{order.OrderId}/approve", null);
        Assert.Equal(HttpStatusCode.OK, approve.StatusCode);

        var detail = await customer.GetFromJsonAsync<OrderDto>($"/api/orders/{order.OrderId}");
        Assert.NotNull(detail);
        Assert.Equal("COMPLETED", detail.Status); // approval still triggers fulfillment
    }
}
