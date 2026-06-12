using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using Petstore.Models;

namespace Petstore.Tests;

[Collection(PetstoreCatalogDatabaseCollection.Name)]
[Trait("Category", DatabaseIntegrationTestCategory.Name)]
public sealed class AdminSalesAnalyticsApiContractTests(PetstoreCatalogTestsFixture fixture) : DatabaseIntegrationTestBase(fixture)
{
    private const string Range = "startDate=2026-01-01&endDate=2030-12-31";

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

    /// <summary>Finds a sellable item id in the given category through the public catalog API.</summary>
    private static async Task<(string ItemId, decimal Price)> FirstItemOfCategoryAsync(HttpClient client, string categoryId)
    {
        var products = await client.GetFromJsonAsync<IReadOnlyList<ProductDto>>($"/api/catalog/categories/{categoryId}/products");
        foreach (var product in products!)
        {
            var items = await client.GetFromJsonAsync<IReadOnlyList<ItemDto>>($"/api/catalog/products/{product.Id}/items");
            if (items is { Count: > 0 })
            {
                return (items[0].Id, items[0].Price);
            }
        }

        throw new InvalidOperationException($"No sellable items found in category {categoryId}.");
    }

    private static async Task<OrderDto> PlaceOrderAsync(HttpClient customer, string itemId, int quantity)
    {
        await customer.PostAsJsonAsync("/api/cart/items", new AddCartItemRequestDto(itemId));
        await customer.PutAsJsonAsync($"/api/cart/items/{itemId}", new SetCartQuantityRequestDto(quantity));
        var response = await customer.PostAsJsonAsync("/api/orders", new PlaceOrderRequestDto(Shipping, null));
        return (await response.Content.ReadFromJsonAsync<OrderDto>())!;
    }

    [Fact]
    public async Task Aggregates_Revenue_Counts_And_Percentages_By_Category()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var customer = await SignInClientAsync(factory, "j2ee", "j2ee");
        using var admin = await SignInClientAsync(factory, "admin", "admin");

        var (fishItem, fishPrice) = await FirstItemOfCategoryAsync(customer, "FISH");
        var (birdItem, birdPrice) = await FirstItemOfCategoryAsync(customer, "BIRDS");

        await PlaceOrderAsync(customer, fishItem, 2);  // COMPLETED (auto)
        await PlaceOrderAsync(customer, birdItem, 1);  // COMPLETED (auto)

        var analytics = await admin.GetFromJsonAsync<AdminSalesAnalyticsDto>($"/api/admin/analytics/sales?{Range}");

        Assert.NotNull(analytics);
        var fish = analytics.Categories.Single(c => c.CategoryId == "FISH");
        var birds = analytics.Categories.Single(c => c.CategoryId == "BIRDS");
        Assert.Equal(fishPrice * 2, fish.Revenue);
        Assert.Equal(2, fish.SalesCount);
        Assert.Equal("Fish", fish.CategoryName);
        Assert.Equal(birdPrice * 1, birds.Revenue);
        Assert.Equal(1, birds.SalesCount);
        Assert.Equal(fish.Revenue + birds.Revenue, analytics.TotalRevenue);
        Assert.Equal(3, analytics.TotalSalesCount);
        Assert.Equal(100m, fish.RevenuePercent + birds.RevenuePercent);
    }

    [Fact]
    public async Task Status_Policy_Excludes_Pending_Approved_And_Denied()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var customer = await SignInClientAsync(factory, "j2ee", "j2ee");
        using var admin = await SignInClientAsync(factory, "admin", "admin");
        using var supplier = await SignInClientAsync(factory, "supplier", "supplier");

        // PENDING: above threshold, never decided.
        await PlaceOrderAsync(customer, "EST-1", 31);

        // DENIED: above threshold, denied by admin.
        var denied = await PlaceOrderAsync(customer, "EST-1", 31);
        await admin.PostAsync($"/api/admin/orders/{denied.OrderId}/deny", null);

        // APPROVED only: zero stock keeps it un-shipped.
        await supplier.PutAsJsonAsync("/api/supplier/inventory/EST-2", new SetInventoryRequestDto(0));
        var approvedOnly = await PlaceOrderAsync(customer, "EST-2", 1);
        var approvedDetail = await customer.GetFromJsonAsync<OrderDto>($"/api/orders/{approvedOnly.OrderId}");
        Assert.Equal("APPROVED", approvedDetail!.Status);

        var analytics = await admin.GetFromJsonAsync<AdminSalesAnalyticsDto>($"/api/admin/analytics/sales?{Range}");

        Assert.NotNull(analytics);
        Assert.Equal(0m, analytics.TotalRevenue);
        Assert.Equal(0, analytics.TotalSalesCount);
        Assert.Empty(analytics.Categories);
    }

    [Fact]
    public async Task Status_Policy_Includes_Partially_Shipped_Orders()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var customer = await SignInClientAsync(factory, "j2ee", "j2ee");
        using var admin = await SignInClientAsync(factory, "admin", "admin");
        using var supplier = await SignInClientAsync(factory, "supplier", "supplier");

        await supplier.PutAsJsonAsync("/api/supplier/inventory/EST-1", new SetInventoryRequestDto(1));
        var order = await PlaceOrderAsync(customer, "EST-1", 3); // ships 1 of 3 -> SHIPPED_PART
        var detail = await customer.GetFromJsonAsync<OrderDto>($"/api/orders/{order.OrderId}");
        Assert.Equal("SHIPPED_PART", detail!.Status);

        var analytics = await admin.GetFromJsonAsync<AdminSalesAnalyticsDto>($"/api/admin/analytics/sales?{Range}");

        Assert.NotNull(analytics);
        var fish = Assert.Single(analytics.Categories);
        Assert.Equal("FISH", fish.CategoryId);
        // The whole ordered line counts, not only the shipped part.
        Assert.Equal(3, fish.SalesCount);
        Assert.Equal(100m, fish.RevenuePercent);
    }

    [Fact]
    public async Task Date_Filtering_Excludes_Orders_Outside_The_Range()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var customer = await SignInClientAsync(factory, "j2ee", "j2ee");
        using var admin = await SignInClientAsync(factory, "admin", "admin");

        var inRange = await PlaceOrderAsync(customer, "EST-1", 1);
        var outOfRange = await PlaceOrderAsync(customer, "EST-2", 1);

        // Move one order to a known past date directly in the database.
        await using (var context = Fixture.CreateContext())
        {
            await context.Orders
                .Where(order => order.Id == int.Parse(outOfRange.OrderId))
                .ExecuteUpdateAsync(setters => setters.SetProperty(
                    order => order.PlacedAt,
                    new DateTime(2020, 1, 15, 12, 0, 0, DateTimeKind.Utc)));
        }

        var now = await admin.GetFromJsonAsync<AdminSalesAnalyticsDto>($"/api/admin/analytics/sales?{Range}");
        var past = await admin.GetFromJsonAsync<AdminSalesAnalyticsDto>(
            "/api/admin/analytics/sales?startDate=2020-01-15&endDate=2020-01-15");

        Assert.NotNull(now);
        Assert.NotNull(past);
        // Current range sees only the in-range order's quantity.
        Assert.Equal(1, now.TotalSalesCount);
        // The inclusive single-day range catches the moved order.
        Assert.Equal(1, past.TotalSalesCount);
        Assert.NotEqual(inRange.OrderId, outOfRange.OrderId);
    }

    [Fact]
    public async Task Empty_Range_Returns_Zero_Data()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var admin = await SignInClientAsync(factory, "admin", "admin");

        var analytics = await admin.GetFromJsonAsync<AdminSalesAnalyticsDto>(
            "/api/admin/analytics/sales?startDate=1999-01-01&endDate=1999-01-31");

        Assert.NotNull(analytics);
        Assert.Equal("1999-01-01", analytics.StartDate);
        Assert.Equal("1999-01-31", analytics.EndDate);
        Assert.Equal(0m, analytics.TotalRevenue);
        Assert.Equal(0, analytics.TotalSalesCount);
        Assert.Empty(analytics.Categories);
    }

    [Theory]
    [InlineData("startDate=2026-07-01&endDate=2026-06-01")]
    [InlineData("startDate=garbage&endDate=2026-06-01")]
    [InlineData("endDate=2026-06-01")]
    [InlineData("")]
    public async Task Invalid_Ranges_Return_Validation_Error(string query)
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var admin = await SignInClientAsync(factory, "admin", "admin");

        var response = await admin.GetAsync($"/api/admin/analytics/sales?{query}");
        var error = await response.Content.ReadFromJsonAsync<ApiErrorDto>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(error);
        Assert.Equal("analytics.validation", error.Code);
    }

    [Fact]
    public async Task Only_Admin_Can_Read_Analytics()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);

        using var supplier = await SignInClientAsync(factory, "supplier", "supplier");
        using var customer = await SignInClientAsync(factory, "j2ee", "j2ee");
        using var anonymous = factory.CreateClient();

        var supplierResponse = await supplier.GetAsync($"/api/admin/analytics/sales?{Range}");
        var customerResponse = await customer.GetAsync($"/api/admin/analytics/sales?{Range}");
        var anonymousResponse = await anonymous.GetAsync($"/api/admin/analytics/sales?{Range}");

        Assert.Equal(HttpStatusCode.Forbidden, supplierResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, customerResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, anonymousResponse.StatusCode);
    }
}
