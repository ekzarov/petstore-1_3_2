using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using Petstore.Models;

namespace Petstore.Tests;

[Collection(PetstoreCatalogDatabaseCollection.Name)]
[Trait("Category", DatabaseIntegrationTestCategory.Name)]
public sealed class FulfillmentIntegrationTests(PetstoreCatalogTestsFixture fixture) : DatabaseIntegrationTestBase(fixture)
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

    private static async Task<OrderDto> PlaceOrderAsync(HttpClient customer, string itemId, int quantity)
    {
        await customer.PostAsJsonAsync("/api/cart/items", new AddCartItemRequestDto(itemId));
        await customer.PutAsJsonAsync($"/api/cart/items/{itemId}", new SetCartQuantityRequestDto(quantity));
        var response = await customer.PostAsJsonAsync("/api/orders", new PlaceOrderRequestDto(Shipping, null));
        return (await response.Content.ReadFromJsonAsync<OrderDto>())!;
    }

    private async Task SetInventoryDirectAsync(string itemId, int quantity)
    {
        await using var context = Fixture.CreateContext();
        var existing = await context.SupplierInventory.SingleOrDefaultAsync(i => i.ItemId == itemId);
        if (existing is null)
        {
            context.SupplierInventory.Add(new Petstore.Data.Entities.SupplierInventoryEntity
            {
                ItemId = itemId,
                QuantityOnHand = quantity
            });
        }
        else
        {
            existing.QuantityOnHand = quantity;
        }

        await context.SaveChangesAsync();
    }

    private async Task<int> GetInventoryDirectAsync(string itemId)
    {
        await using var context = Fixture.CreateContext();
        return await context.SupplierInventory
            .Where(i => i.ItemId == itemId)
            .Select(i => (int?)i.QuantityOnHand)
            .SingleOrDefaultAsync() ?? 0;
    }

    [Fact]
    public async Task Approved_Order_With_Stock_Completes_And_Decrements_Inventory()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var customer = await SignInClientAsync(factory, "j2ee", "j2ee");

        var before = await GetInventoryDirectAsync("EST-1");
        var order = await PlaceOrderAsync(customer, "EST-1", 2); // 33.00 -> auto-approved -> fulfilled

        Assert.Equal("COMPLETED", order.Status);
        Assert.Equal(before - 2, await GetInventoryDirectAsync("EST-1"));

        await using var context = Fixture.CreateContext();
        var shipment = Assert.Single(await context.Shipments.AsNoTracking()
            .Include(s => s.Lines)
            .Where(s => s.OrderId == int.Parse(order.OrderId))
            .ToListAsync());
        var shipmentLine = Assert.Single(shipment.Lines);
        Assert.Equal("EST-1", shipmentLine.ItemId);
        Assert.Equal(2, shipmentLine.QuantityShipped);
    }

    [Fact]
    public async Task Partial_Stock_Ships_Partially_Then_Completes_After_Replenishment()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var customer = await SignInClientAsync(factory, "j2ee", "j2ee");
        using var admin = await SignInClientAsync(factory, "admin", "admin");

        // Only 3 of 10 ordered units in stock.
        await SetInventoryDirectAsync("EST-2", 3);
        var order = await PlaceOrderAsync(customer, "EST-2", 10); // 165.00 -> auto-approved

        var afterPartial = await customer.GetFromJsonAsync<OrderDto>($"/api/orders/{order.OrderId}");
        Assert.NotNull(afterPartial);
        Assert.Equal("SHIPPED_PART", afterPartial.Status);
        Assert.Equal(0, await GetInventoryDirectAsync("EST-2"));

        // Replenish through the admin API: this must re-run fulfillment.
        var replenish = await admin.PutAsJsonAsync("/api/admin/inventory/EST-2", new SetInventoryRequestDto(50));
        Assert.Equal(HttpStatusCode.OK, replenish.StatusCode);

        var afterReplenish = await customer.GetFromJsonAsync<OrderDto>($"/api/orders/{order.OrderId}");
        Assert.NotNull(afterReplenish);
        Assert.Equal("COMPLETED", afterReplenish.Status);
        Assert.Equal(50 - 7, await GetInventoryDirectAsync("EST-2")); // remaining 7 shipped

        await using var context = Fixture.CreateContext();
        var shipments = await context.Shipments.AsNoTracking()
            .Include(s => s.Lines)
            .Where(s => s.OrderId == int.Parse(order.OrderId))
            .OrderBy(s => s.Id)
            .ToListAsync();
        Assert.Equal(2, shipments.Count);
        Assert.Equal(3, shipments[0].Lines.Single().QuantityShipped);
        Assert.Equal(7, shipments[1].Lines.Single().QuantityShipped);
    }

    [Fact]
    public async Task No_Stock_Leaves_Order_Approved_With_No_Shipment()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var customer = await SignInClientAsync(factory, "j2ee", "j2ee");

        await SetInventoryDirectAsync("EST-2", 0);
        var order = await PlaceOrderAsync(customer, "EST-2", 2);

        var detail = await customer.GetFromJsonAsync<OrderDto>($"/api/orders/{order.OrderId}");
        Assert.NotNull(detail);
        Assert.Equal("APPROVED", detail.Status);

        await using var context = Fixture.CreateContext();
        Assert.Empty(await context.Shipments.AsNoTracking()
            .Where(s => s.OrderId == int.Parse(order.OrderId)).ToListAsync());
    }

    [Fact]
    public async Task Two_Orders_Competing_For_Stock_Never_Overship()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var customer1 = await SignInClientAsync(factory, "j2ee", "j2ee");
        using var customer2 = await SignInClientAsync(factory, "shopper", "j2ee");

        await SetInventoryDirectAsync("EST-1", 5);
        var order1 = await PlaceOrderAsync(customer1, "EST-1", 4); // ships 4, leaves 1
        var order2 = await PlaceOrderAsync(customer2, "EST-1", 4); // only 1 left -> partial

        var detail1 = await customer1.GetFromJsonAsync<OrderDto>($"/api/orders/{order1.OrderId}");
        var detail2 = await customer2.GetFromJsonAsync<OrderDto>($"/api/orders/{order2.OrderId}");
        Assert.NotNull(detail1);
        Assert.NotNull(detail2);
        Assert.Equal("COMPLETED", detail1.Status);
        Assert.Equal("SHIPPED_PART", detail2.Status);
        Assert.Equal(0, await GetInventoryDirectAsync("EST-1"));

        // Total shipped across both orders never exceeds the 5 units that existed.
        await using var context = Fixture.CreateContext();
        var totalShipped = await context.Shipments.AsNoTracking()
            .Include(s => s.Lines)
            .Where(s => s.OrderId == int.Parse(order1.OrderId) || s.OrderId == int.Parse(order2.OrderId))
            .SelectMany(s => s.Lines)
            .SumAsync(l => l.QuantityShipped);
        Assert.Equal(5, totalShipped);
    }

    [Fact]
    public async Task ReRunning_Fulfillment_Is_Idempotent()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var customer = await SignInClientAsync(factory, "j2ee", "j2ee");
        using var admin = await SignInClientAsync(factory, "admin", "admin");

        var order = await PlaceOrderAsync(customer, "EST-1", 2); // COMPLETED
        var inventoryAfter = await GetInventoryDirectAsync("EST-1");

        var run = await admin.PostAsync("/api/admin/fulfillment/run", null);
        Assert.Equal(HttpStatusCode.OK, run.StatusCode);

        Assert.Equal(inventoryAfter, await GetInventoryDirectAsync("EST-1"));
        await using var context = Fixture.CreateContext();
        Assert.Single(await context.Shipments.AsNoTracking()
            .Where(s => s.OrderId == int.Parse(order.OrderId)).ToListAsync());
    }

    [Fact]
    public async Task Denied_Orders_Are_Never_Fulfilled()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var customer = await SignInClientAsync(factory, "j2ee", "j2ee");
        using var admin = await SignInClientAsync(factory, "admin", "admin");

        var order = await PlaceOrderAsync(customer, "EST-1", 31); // 511.50 -> PENDING
        await admin.PostAsync($"/api/admin/orders/{order.OrderId}/deny", null);

        var run = await admin.PostAsync("/api/admin/fulfillment/run", null);
        Assert.Equal(HttpStatusCode.OK, run.StatusCode);

        await using var context = Fixture.CreateContext();
        Assert.Empty(await context.Shipments.AsNoTracking()
            .Where(s => s.OrderId == int.Parse(order.OrderId)).ToListAsync());
        var detail = await customer.GetFromJsonAsync<OrderDto>($"/api/orders/{order.OrderId}");
        Assert.NotNull(detail);
        Assert.Equal("DENIED", detail.Status);
    }
}
