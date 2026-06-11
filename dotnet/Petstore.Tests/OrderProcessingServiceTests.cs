using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Petstore.OrderProcessing;
using Petstore.Orders;
using Petstore.Data.Entities;

namespace Petstore.Tests;

[Collection(PetstoreCatalogDatabaseCollection.Name)]
[Trait("Category", DatabaseIntegrationTestCategory.Name)]
public sealed class OrderProcessingServiceTests(PetstoreCatalogTestsFixture fixture) : DatabaseIntegrationTestBase(fixture)
{
    private static IConfiguration ConfigWithThreshold(decimal threshold)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OrderProcessing:AutoApprovalThreshold"] = threshold.ToString()
            })
            .Build();
    }

    private async Task<int> CreateOrderAsync(decimal total, string status = OrderStatus.Pending)
    {
        await using var context = Fixture.CreateContext();
        var order = new OrderEntity
        {
            UserId = "j2ee",
            PlacedAt = DateTime.UtcNow,
            Status = status,
            Currency = "USD",
            Total = total,
            ShippingContact = Contact(),
            BillingContact = Contact(),
            Lines = [new OrderLineEntity { ItemId = "EST-1", Name = "Large Angelfish", UnitPrice = total, Currency = "USD", Quantity = 1 }]
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();
        return order.Id;
    }

    private static OrderContactBlock Contact()
    {
        return new OrderContactBlock
        {
            FamilyName = "Doe",
            GivenName = "Jane",
            Street1 = "1 Main St",
            City = "Springfield",
            State = "IL",
            Zip = "62701",
            Country = "USA",
            Email = "jane@example.com",
            Phone = "555-0100"
        };
    }

    [Fact]
    public async Task Order_Below_Threshold_Is_Auto_Approved_As_System()
    {
        var orderId = await CreateOrderAsync(499.99m);

        await using var context = Fixture.CreateContext();
        var service = new OrderProcessingService(context, new OrderTransitionRepository(context), ConfigWithThreshold(500m), new Petstore.Supplier.FulfillmentService(context, new Petstore.Supplier.InventoryRepository(context), new OrderTransitionRepository(context)));
        await service.EvaluateNewOrderAsync(orderId, CancellationToken.None);

        var order = await context.Orders.AsNoTracking().SingleAsync(o => o.Id == orderId);
        Assert.Equal(OrderStatus.Approved, order.Status);

        var transition = Assert.Single(await context.OrderStatusTransitions.AsNoTracking().Where(t => t.OrderId == orderId).ToListAsync());
        Assert.Equal(OrderStatus.Pending, transition.FromStatus);
        Assert.Equal(OrderStatus.Approved, transition.ToStatus);
        Assert.Equal("system", transition.Actor);
    }

    [Fact]
    public async Task Order_At_Threshold_Stays_Pending()
    {
        var orderId = await CreateOrderAsync(500m);

        await using var context = Fixture.CreateContext();
        var service = new OrderProcessingService(context, new OrderTransitionRepository(context), ConfigWithThreshold(500m), new Petstore.Supplier.FulfillmentService(context, new Petstore.Supplier.InventoryRepository(context), new OrderTransitionRepository(context)));
        await service.EvaluateNewOrderAsync(orderId, CancellationToken.None);

        var order = await context.Orders.AsNoTracking().SingleAsync(o => o.Id == orderId);
        Assert.Equal(OrderStatus.Pending, order.Status);
        Assert.Empty(await context.OrderStatusTransitions.AsNoTracking().Where(t => t.OrderId == orderId).ToListAsync());
    }

    [Fact]
    public async Task Re_Evaluation_Of_Decided_Order_Is_A_NoOp()
    {
        var orderId = await CreateOrderAsync(100m);

        await using var context = Fixture.CreateContext();
        var service = new OrderProcessingService(context, new OrderTransitionRepository(context), ConfigWithThreshold(500m), new Petstore.Supplier.FulfillmentService(context, new Petstore.Supplier.InventoryRepository(context), new OrderTransitionRepository(context)));
        await service.EvaluateNewOrderAsync(orderId, CancellationToken.None);
        await service.EvaluateNewOrderAsync(orderId, CancellationToken.None);

        Assert.Single(await context.OrderStatusTransitions.AsNoTracking().Where(t => t.OrderId == orderId).ToListAsync());
    }

    [Fact]
    public async Task Concurrent_Decisions_Produce_Exactly_One_Winner()
    {
        var orderId = await CreateOrderAsync(900m);

        await using var context1 = Fixture.CreateContext();
        await using var context2 = Fixture.CreateContext();
        var repo1 = new OrderTransitionRepository(context1);
        var repo2 = new OrderTransitionRepository(context2);

        var approve = await repo1.TryTransitionAsync(orderId, OrderStatus.Pending, OrderStatus.Approved, "admin", CancellationToken.None);
        var deny = await repo2.TryTransitionAsync(orderId, OrderStatus.Pending, OrderStatus.Denied, "admin", CancellationToken.None);

        Assert.True(approve);
        Assert.False(deny);

        await using var verify = Fixture.CreateContext();
        var order = await verify.Orders.AsNoTracking().SingleAsync(o => o.Id == orderId);
        Assert.Equal(OrderStatus.Approved, order.Status);
        Assert.Single(await verify.OrderStatusTransitions.AsNoTracking().Where(t => t.OrderId == orderId).ToListAsync());
    }

    [Fact]
    public async Task Illegal_Transition_Changes_Nothing()
    {
        var orderId = await CreateOrderAsync(900m, OrderStatus.Denied);

        await using var context = Fixture.CreateContext();
        var repo = new OrderTransitionRepository(context);
        var result = await repo.TryTransitionAsync(orderId, OrderStatus.Denied, OrderStatus.Approved, "admin", CancellationToken.None);

        Assert.False(result);
        var order = await context.Orders.AsNoTracking().SingleAsync(o => o.Id == orderId);
        Assert.Equal(OrderStatus.Denied, order.Status);
    }
}
