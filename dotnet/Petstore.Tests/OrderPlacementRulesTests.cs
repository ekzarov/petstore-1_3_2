using Petstore.Data.Entities;
using Petstore.Models;
using Petstore.Orders;

namespace Petstore.Tests;

public sealed class OrderPlacementRulesTests
{
    private static readonly ItemDto Est1 = new("EST-1", "FI-SW-01", "Large Angelfish", ["Large"], null, 16.50m, "USD");
    private static readonly ItemDto Est2 = new("EST-2", "FI-SW-01", "Small Angelfish", ["Small"], null, 16.50m, "USD");

    private static CartLineEntity CartLine(string itemId, int quantity)
    {
        return new CartLineEntity { CartKey = "user:test", ItemId = itemId, Quantity = quantity };
    }

    [Fact]
    public void FreezeLines_Copies_Catalog_Values_And_Quantities()
    {
        var (lines, missing) = OrderPlacementRules.FreezeLines(
            [CartLine("EST-1", 2), CartLine("EST-2", 3)],
            new Dictionary<string, ItemDto> { ["EST-1"] = Est1, ["EST-2"] = Est2 });

        Assert.Empty(missing);
        Assert.Equal(2, lines.Count);
        var first = lines.Single(line => line.ItemId == "EST-1");
        Assert.Equal("Large Angelfish", first.Name);
        Assert.Equal(16.50m, first.UnitPrice);
        Assert.Equal("USD", first.Currency);
        Assert.Equal(2, first.Quantity);
    }

    [Fact]
    public void FreezeLines_Reports_Missing_Items()
    {
        var (lines, missing) = OrderPlacementRules.FreezeLines(
            [CartLine("EST-1", 1), CartLine("GONE", 1)],
            new Dictionary<string, ItemDto> { ["EST-1"] = Est1 });

        Assert.Single(lines);
        Assert.Equal(["GONE"], missing);
    }

    [Fact]
    public void ComputeTotal_Sums_Price_Times_Quantity()
    {
        var (lines, _) = OrderPlacementRules.FreezeLines(
            [CartLine("EST-1", 2), CartLine("EST-2", 3)],
            new Dictionary<string, ItemDto> { ["EST-1"] = Est1, ["EST-2"] = Est2 });

        Assert.Equal(16.50m * 2 + 16.50m * 3, OrderPlacementRules.ComputeTotal(lines));
    }

    [Fact]
    public void ComputeTotal_Of_No_Lines_Is_Zero()
    {
        Assert.Equal(0m, OrderPlacementRules.ComputeTotal([]));
    }

    [Fact]
    public void ToContactBlock_Maps_All_Fields()
    {
        var contact = new ContactInfoDto(
            "Doe", "Jane", "1 Main St", "Unit 2", "Springfield", "IL", "62701", "USA", "jane@example.com", "555-0100");

        var block = OrderPlacementRules.ToContactBlock(contact);

        Assert.Equal("Doe", block.FamilyName);
        Assert.Equal("Jane", block.GivenName);
        Assert.Equal("1 Main St", block.Street1);
        Assert.Equal("Unit 2", block.Street2);
        Assert.Equal("Springfield", block.City);
        Assert.Equal("IL", block.State);
        Assert.Equal("62701", block.Zip);
        Assert.Equal("USA", block.Country);
        Assert.Equal("jane@example.com", block.Email);
        Assert.Equal("555-0100", block.Phone);
    }
}
