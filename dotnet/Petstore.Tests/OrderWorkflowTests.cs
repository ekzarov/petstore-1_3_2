using Petstore.OrderProcessing;
using Petstore.Orders;

namespace Petstore.Tests;

public sealed class OrderWorkflowTests
{
    [Theory]
    [InlineData(OrderStatus.Pending, OrderStatus.Approved)]
    [InlineData(OrderStatus.Pending, OrderStatus.Denied)]
    [InlineData(OrderStatus.Approved, OrderStatus.ShippedPart)]
    [InlineData(OrderStatus.Approved, OrderStatus.Shipped)]
    [InlineData(OrderStatus.ShippedPart, OrderStatus.ShippedPart)]
    [InlineData(OrderStatus.ShippedPart, OrderStatus.Shipped)]
    [InlineData(OrderStatus.Shipped, OrderStatus.Completed)]
    public void Legal_Transitions_Are_Allowed(string from, string to)
    {
        Assert.True(OrderWorkflow.IsLegal(from, to));
    }

    [Theory]
    [InlineData(OrderStatus.Pending, OrderStatus.Shipped)]
    [InlineData(OrderStatus.Pending, OrderStatus.Completed)]
    [InlineData(OrderStatus.Pending, OrderStatus.Pending)]
    [InlineData(OrderStatus.Approved, OrderStatus.Pending)]
    [InlineData(OrderStatus.Approved, OrderStatus.Denied)]
    [InlineData(OrderStatus.Approved, OrderStatus.Completed)]
    [InlineData(OrderStatus.Denied, OrderStatus.Approved)]
    [InlineData(OrderStatus.Denied, OrderStatus.Pending)]
    [InlineData(OrderStatus.Completed, OrderStatus.Pending)]
    [InlineData(OrderStatus.Completed, OrderStatus.Shipped)]
    [InlineData(OrderStatus.Shipped, OrderStatus.Approved)]
    [InlineData("UNKNOWN", OrderStatus.Approved)]
    public void Illegal_Transitions_Are_Rejected(string from, string to)
    {
        Assert.False(OrderWorkflow.IsLegal(from, to));
    }

    [Fact]
    public void Denied_And_Completed_Are_Terminal()
    {
        foreach (var to in OrderStatus.All)
        {
            Assert.False(OrderWorkflow.IsLegal(OrderStatus.Denied, to));
            Assert.False(OrderWorkflow.IsLegal(OrderStatus.Completed, to));
        }
    }
}
