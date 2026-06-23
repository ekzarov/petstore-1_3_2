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

    [Fact]
    public async Task Aggregates_Multiple_Lines_In_Same_Category_And_Filters_Unsold_Categories()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var customer = await SignInClientAsync(factory, "j2ee", "j2ee");
        using var admin = await SignInClientAsync(factory, "admin", "admin");

        // Fetch 2 items of category FISH and 1 item of category BIRDS
        var fishItems = await GetItemsOfCategoryAsync(customer, "FISH", 2);
        var birdItems = await GetItemsOfCategoryAsync(customer, "BIRDS", 1);

        Assert.Equal(2, fishItems.Count);
        Assert.Single(birdItems);

        var fish1 = fishItems[0];
        var fish2 = fishItems[1];
        var bird = birdItems[0];

        // Place one order with multiple line items from FISH and BIRDS
        await customer.PostAsJsonAsync("/api/cart/items", new AddCartItemRequestDto(fish1.ItemId));
        await customer.PutAsJsonAsync($"/api/cart/items/{fish1.ItemId}", new SetCartQuantityRequestDto(2));

        await customer.PostAsJsonAsync("/api/cart/items", new AddCartItemRequestDto(fish2.ItemId));
        await customer.PutAsJsonAsync($"/api/cart/items/{fish2.ItemId}", new SetCartQuantityRequestDto(3));

        await customer.PostAsJsonAsync("/api/cart/items", new AddCartItemRequestDto(bird.ItemId));
        await customer.PutAsJsonAsync($"/api/cart/items/{bird.ItemId}", new SetCartQuantityRequestDto(1));

        var orderResponse = await customer.PostAsJsonAsync("/api/orders", new PlaceOrderRequestDto(Shipping, null));
        Assert.Equal(HttpStatusCode.Created, orderResponse.StatusCode);

        // Fetch analytics
        var analytics = await admin.GetFromJsonAsync<AdminSalesAnalyticsDto>($"/api/admin/analytics/sales?{Range}");

        Assert.NotNull(analytics);
        
        // Assert that only FISH and BIRDS are returned (DOGS, CATS, REPTILES are unsold and must be excluded)
        Assert.Equal(2, analytics.Categories.Count);
        
        var fishCategory = analytics.Categories.Single(c => c.CategoryId == "FISH");
        var birdCategory = analytics.Categories.Single(c => c.CategoryId == "BIRDS");

        // Assert grouping and summing for multiple lines in the same category (FISH)
        var expectedFishRevenue = (fish1.Price * 2) + (fish2.Price * 3);
        Assert.Equal(expectedFishRevenue, fishCategory.Revenue);
        Assert.Equal(5, fishCategory.SalesCount);

        // Assert single line BIRDS
        var expectedBirdRevenue = bird.Price * 1;
        Assert.Equal(expectedBirdRevenue, birdCategory.Revenue);
        Assert.Equal(1, birdCategory.SalesCount);

        // Total
        Assert.Equal(expectedFishRevenue + expectedBirdRevenue, analytics.TotalRevenue);
        Assert.Equal(6, analytics.TotalSalesCount);
        Assert.Equal(100m, fishCategory.RevenuePercent + birdCategory.RevenuePercent);
    }

    [Fact]
    public async Task Unknown_Or_Deleted_Category_Fallback_To_Unknown_Group()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var admin = await SignInClientAsync(factory, "admin", "admin");

        // Directly insert an order with an unknown/deleted item ID
        await using (var context = Fixture.CreateContext())
        {
            var unknownItemOrder = new Petstore.Data.Entities.OrderEntity
            {
                UserId = "j2ee",
                PlacedAt = new DateTime(2026, 6, 15, 12, 0, 0, DateTimeKind.Utc),
                Status = Petstore.Orders.OrderStatus.Completed,
                Currency = "USD",
                Total = 100.00m,
                ShippingContact = new Petstore.Data.Entities.OrderContactBlock
                {
                    FamilyName = "Doe", GivenName = "Jane", Street1 = "1 Main Street", City = "Springfield",
                    State = "IL", Zip = "62701", Country = "USA", Email = "jane@example.com", Phone = "555-0100"
                },
                BillingContact = new Petstore.Data.Entities.OrderContactBlock
                {
                    FamilyName = "Doe", GivenName = "Jane", Street1 = "1 Main Street", City = "Springfield",
                    State = "IL", Zip = "62701", Country = "USA", Email = "jane@example.com", Phone = "555-0100"
                },
                Lines =
                [
                    new Petstore.Data.Entities.OrderLineEntity
                    {
                        ItemId = "EST-FAKE-UNKNOWN-ITEM",
                        Name = "Fake Unknown Item",
                        UnitPrice = 50.00m,
                        Currency = "USD",
                        Quantity = 2,
                        QuantityShipped = 2
                    }
                ]
            };

            context.Orders.Add(unknownItemOrder);
            await context.SaveChangesAsync();
        }

        // Retrieve analytics
        var analytics = await admin.GetFromJsonAsync<AdminSalesAnalyticsDto>($"/api/admin/analytics/sales?{Range}");
        
        Assert.NotNull(analytics);
        
        // Assert category groups under UNKNOWN
        var unknownCategory = analytics.Categories.Single(c => c.CategoryId == "UNKNOWN");
        Assert.Equal("UNKNOWN", unknownCategory.CategoryName);
        Assert.Equal(100.00m, unknownCategory.Revenue);
        Assert.Equal(2, unknownCategory.SalesCount);
    }

    private static async Task<List<(string ItemId, decimal Price)>> GetItemsOfCategoryAsync(HttpClient client, string categoryId, int count)
    {
        var itemsList = new List<(string ItemId, decimal Price)>();
        var products = await client.GetFromJsonAsync<IReadOnlyList<ProductDto>>($"/api/catalog/categories/{categoryId}/products");
        foreach (var product in products!)
        {
            var items = await client.GetFromJsonAsync<IReadOnlyList<ItemDto>>($"/api/catalog/products/{product.Id}/items");
            if (items != null)
            {
                foreach (var item in items)
                {
                    itemsList.Add((item.Id, item.Price));
                    if (itemsList.Count == count)
                    {
                        return itemsList;
                    }
                }
            }
        }
        return itemsList;
    }

    [Fact]
    public async Task Analytics_Retrieval_Is_Read_Only_And_Does_Not_Mutate_Database()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var customer = await SignInClientAsync(factory, "j2ee", "j2ee");
        using var admin = await SignInClientAsync(factory, "admin", "admin");

        var (fishItem, _) = await FirstItemOfCategoryAsync(customer, "FISH");
        await PlaceOrderAsync(customer, fishItem, 2);

        // Capture database state before calling analytics
        int initialOrdersCount;
        int initialOrderLinesCount;
        int initialCategoriesCount;
        int initialItemsCount;
        var initialInventory = new List<(string ItemId, int Quantity)>();

        await using (var context = Fixture.CreateContext())
        {
            initialOrdersCount = await context.Orders.CountAsync();
            initialOrderLinesCount = await context.OrderLines.CountAsync();
            initialCategoriesCount = await context.Categories.CountAsync();
            initialItemsCount = await context.Items.CountAsync();
            
            var inventoryList = await context.SupplierInventory
                .OrderBy(inv => inv.ItemId)
                .Select(inv => new { inv.ItemId, inv.QuantityOnHand })
                .AsNoTracking()
                .ToListAsync();
            initialInventory = inventoryList.Select(inv => (inv.ItemId, inv.QuantityOnHand)).ToList();
        }

        // Call the analytics endpoint
        var analyticsResponse = await admin.GetAsync($"/api/admin/analytics/sales?{Range}");
        Assert.Equal(HttpStatusCode.OK, analyticsResponse.StatusCode);

        // Verify database state after calling analytics is unchanged
        await using (var context = Fixture.CreateContext())
        {
            Assert.Equal(initialOrdersCount, await context.Orders.CountAsync());
            Assert.Equal(initialOrderLinesCount, await context.OrderLines.CountAsync());
            Assert.Equal(initialCategoriesCount, await context.Categories.CountAsync());
            Assert.Equal(initialItemsCount, await context.Items.CountAsync());
            
            var currentInventoryList = await context.SupplierInventory
                .OrderBy(inv => inv.ItemId)
                .Select(inv => new { inv.ItemId, inv.QuantityOnHand })
                .AsNoTracking()
                .ToListAsync();
            var currentInventory = currentInventoryList.Select(inv => (inv.ItemId, inv.QuantityOnHand)).ToList();

            Assert.Equal(initialInventory, currentInventory);
        }
    }

    [Fact]
    public async Task Date_Filtering_Boundary_Conditions()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var customer = await SignInClientAsync(factory, "j2ee", "j2ee");
        using var admin = await SignInClientAsync(factory, "admin", "admin");

        var (fishItem, _) = await FirstItemOfCategoryAsync(customer, "FISH");

        var o1 = await PlaceOrderAsync(customer, fishItem, 1);
        var o2 = await PlaceOrderAsync(customer, fishItem, 1);
        var o3 = await PlaceOrderAsync(customer, fishItem, 1);

        // Update the database records directly with exact boundary PlacedAt values
        await using (var context = Fixture.CreateContext())
        {
            await context.Orders
                .Where(o => o.Id == int.Parse(o1.OrderId))
                .ExecuteUpdateAsync(setters => setters.SetProperty(
                    o => o.PlacedAt,
                    new DateTime(2026, 6, 15, 0, 0, 0, DateTimeKind.Utc)));

            await context.Orders
                .Where(o => o.Id == int.Parse(o2.OrderId))
                .ExecuteUpdateAsync(setters => setters.SetProperty(
                    o => o.PlacedAt,
                    new DateTime(2026, 6, 16, 0, 0, 0, DateTimeKind.Utc)));

            await context.Orders
                .Where(o => o.Id == int.Parse(o3.OrderId))
                .ExecuteUpdateAsync(setters => setters.SetProperty(
                    o => o.PlacedAt,
                    new DateTime(2026, 6, 15, 23, 59, 59, DateTimeKind.Utc)));
        }

        // Request single-day analytics for 2026-06-15
        var analytics = await admin.GetFromJsonAsync<AdminSalesAnalyticsDto>(
            "/api/admin/analytics/sales?startDate=2026-06-15&endDate=2026-06-15");

        Assert.NotNull(analytics);
        // Should include o1 and o3 but exclude o2
        Assert.Equal(2, analytics.TotalSalesCount);
    }
}
