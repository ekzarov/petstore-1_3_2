using System.Net.Http.Headers;
using Petstore.Models;

namespace Petstore.Tests;

[Collection(PetstoreCatalogDatabaseCollection.Name)]
[Trait("Category", DatabaseIntegrationTestCategory.Name)]
public sealed class AdminOrdersApiContractTests(PetstoreCatalogTestsFixture fixture) : DatabaseIntegrationTestBase(fixture)
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

    /// <summary>Places an order; quantity 31 x 16.50 = 511.50 stays above the 500 threshold.</summary>
    private static async Task<OrderDto> PlaceOrderAsync(HttpClient customer, int quantity)
    {
        await customer.PostAsJsonAsync("/api/cart/items", new AddCartItemRequestDto("EST-1"));
        await customer.PutAsJsonAsync("/api/cart/items/EST-1", new SetCartQuantityRequestDto(quantity));
        var response = await customer.PostAsJsonAsync("/api/orders", new PlaceOrderRequestDto(Shipping, null));
        return (await response.Content.ReadFromJsonAsync<OrderDto>())!;
    }

    [Fact]
    public async Task Small_Order_Is_Auto_Approved_On_Placement()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var customer = await SignInClientAsync(factory, "j2ee", "j2ee");

        var order = await PlaceOrderAsync(customer, 1);

        // Auto-approved, then fulfilled from seeded inventory (feature 011).
        Assert.Equal("COMPLETED", order.Status);
    }

    [Fact]
    public async Task Large_Order_Stays_Pending_And_Admin_Approves_It()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var customer = await SignInClientAsync(factory, "j2ee", "j2ee");
        var order = await PlaceOrderAsync(customer, 31);
        Assert.Equal("PENDING", order.Status);

        using var admin = await SignInClientAsync(factory, "admin", "admin");
        var approve = await admin.PostAsync($"/api/admin/orders/{order.OrderId}/approve", null);
        Assert.Equal(HttpStatusCode.OK, approve.StatusCode);

        // Customer sees the change through the unchanged 008 read API.
        // Approval triggers fulfillment (011), so the order completes.
        var detail = await customer.GetFromJsonAsync<OrderDto>($"/api/orders/{order.OrderId}");
        Assert.NotNull(detail);
        Assert.Equal("COMPLETED", detail.Status);

        // Audit trail: the admin decision plus the system fulfillment hops.
        var transitions = await admin.GetFromJsonAsync<IReadOnlyList<OrderTransitionDto>>(
            $"/api/admin/orders/{order.OrderId}/transitions");
        Assert.NotNull(transitions);
        Assert.Equal(3, transitions.Count);
        Assert.Equal(("PENDING", "APPROVED", "admin"), (transitions[0].FromStatus, transitions[0].ToStatus, transitions[0].Actor));
        Assert.Equal(("APPROVED", "SHIPPED", "system"), (transitions[1].FromStatus, transitions[1].ToStatus, transitions[1].Actor));
        Assert.Equal(("SHIPPED", "COMPLETED", "system"), (transitions[2].FromStatus, transitions[2].ToStatus, transitions[2].Actor));
    }

    [Fact]
    public async Task Large_Order_Can_Be_Denied_And_Denial_Is_Terminal()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var customer = await SignInClientAsync(factory, "j2ee", "j2ee");
        var order = await PlaceOrderAsync(customer, 31);

        using var admin = await SignInClientAsync(factory, "admin", "admin");
        var deny = await admin.PostAsync($"/api/admin/orders/{order.OrderId}/deny", null);
        Assert.Equal(HttpStatusCode.OK, deny.StatusCode);

        // The second decision hits the invalid-transition guard.
        var lateApprove = await admin.PostAsync($"/api/admin/orders/{order.OrderId}/approve", null);
        var error = await lateApprove.Content.ReadFromJsonAsync<ApiErrorDto>();
        Assert.Equal(HttpStatusCode.Conflict, lateApprove.StatusCode);
        Assert.NotNull(error);
        Assert.Equal("orders.invalid_transition", error.Code);

        var detail = await customer.GetFromJsonAsync<OrderDto>($"/api/orders/{order.OrderId}");
        Assert.NotNull(detail);
        Assert.Equal("DENIED", detail.Status);
    }

    [Fact]
    public async Task Orders_Can_Be_Listed_By_Status()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var customer = await SignInClientAsync(factory, "j2ee", "j2ee");
        var small = await PlaceOrderAsync(customer, 1);   // COMPLETED (auto-approved + fulfilled)
        var large = await PlaceOrderAsync(customer, 31);  // PENDING

        using var admin = await SignInClientAsync(factory, "admin", "admin");
        var pending = await admin.GetFromJsonAsync<IReadOnlyList<AdminOrderSummaryDto>>("/api/admin/orders?status=PENDING");
        var completed = await admin.GetFromJsonAsync<IReadOnlyList<AdminOrderSummaryDto>>("/api/admin/orders?status=COMPLETED");

        Assert.NotNull(pending);
        Assert.NotNull(completed);
        Assert.Contains(pending, o => o.OrderId == large.OrderId);
        Assert.DoesNotContain(pending, o => o.OrderId == small.OrderId);
        Assert.Contains(completed, o => o.OrderId == small.OrderId);
        Assert.All(pending, o => Assert.Equal("PENDING", o.Status));
        Assert.All(pending, o => Assert.Equal("j2ee", o.UserId));
    }

    [Fact]
    public async Task Admin_Can_Read_Any_Order_Detail()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var customer = await SignInClientAsync(factory, "j2ee", "j2ee");
        var order = await PlaceOrderAsync(customer, 31);

        using var admin = await SignInClientAsync(factory, "admin", "admin");
        var detail = await admin.GetFromJsonAsync<AdminOrderDetailDto>($"/api/admin/orders/{order.OrderId}");

        Assert.NotNull(detail);
        Assert.Equal(order.OrderId, detail.OrderId);
        Assert.Equal("j2ee", detail.UserId);
        Assert.Equal("PENDING", detail.Status);
        var line = Assert.Single(detail.Lines);
        Assert.Equal("EST-1", line.ItemId);
        Assert.Equal(31, line.Quantity);
        Assert.Equal("Doe", detail.ShippingContact.FamilyName);

        var unknown = await admin.GetAsync("/api/admin/orders/999999");
        Assert.Equal(HttpStatusCode.NotFound, unknown.StatusCode);
    }

    [Fact]
    public async Task Invalid_Status_Filter_Is_Rejected()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var admin = await SignInClientAsync(factory, "admin", "admin");

        var response = await admin.GetAsync("/api/admin/orders?status=BOGUS");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Customer_Role_Is_Forbidden_And_Anonymous_Unauthorized()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var customer = await SignInClientAsync(factory, "j2ee", "j2ee");
        var order = await PlaceOrderAsync(customer, 31);

        var customerList = await customer.GetAsync("/api/admin/orders?status=PENDING");
        var customerDecide = await customer.PostAsync($"/api/admin/orders/{order.OrderId}/approve", null);
        Assert.Equal(HttpStatusCode.Forbidden, customerList.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, customerDecide.StatusCode);

        using var anonymous = factory.CreateClient();
        var anonymousList = await anonymous.GetAsync("/api/admin/orders");
        Assert.Equal(HttpStatusCode.Unauthorized, anonymousList.StatusCode);
    }
}
