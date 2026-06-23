using Petstore.Analytics;
using Petstore.Models;

namespace Petstore.Tests;

public sealed class AdminSalesAnalyticsServiceTests
{
    private sealed class FakeRepository(IReadOnlyList<AdminCategorySalesRow> rows) : IAdminSalesAnalyticsRepository
    {
        public DateTime? FromInclusive { get; private set; }

        public DateTime? ToExclusive { get; private set; }

        public Task<IReadOnlyList<AdminCategorySalesRow>> GetCategorySalesAsync(
            DateTime fromInclusive, DateTime toExclusive, CancellationToken cancellationToken)
        {
            FromInclusive = fromInclusive;
            ToExclusive = toExclusive;
            return Task.FromResult(rows);
        }
    }

    private static Task<AdminSalesAnalyticsDto> RunAsync(params AdminCategorySalesRow[] rows)
    {
        var service = new AdminSalesAnalyticsService(new FakeRepository(rows));
        return service.GetSalesAnalyticsAsync(new DateOnly(2026, 6, 1), new DateOnly(2026, 6, 30), CancellationToken.None);
    }

    [Fact]
    public async Task Totals_And_Percentages_Are_Computed_From_Rows()
    {
        var result = await RunAsync(
            new AdminCategorySalesRow("FISH", "Fish", 120.00m, 4),
            new AdminCategorySalesRow("BIRDS", "Birds", 73.00m, 2));

        Assert.Equal(193.00m, result.TotalRevenue);
        Assert.Equal(6, result.TotalSalesCount);
        Assert.Equal(2, result.Categories.Count);
        // Ordered by revenue descending.
        Assert.Equal("FISH", result.Categories[0].CategoryId);
        Assert.Equal(62.18m, result.Categories[0].RevenuePercent);
        Assert.Equal(37.82m, result.Categories[1].RevenuePercent);
        Assert.Equal(4, result.Categories[0].SalesCount);
    }

    [Fact]
    public async Task Single_Category_Gets_Hundred_Percent()
    {
        var result = await RunAsync(new AdminCategorySalesRow("CATS", "Cats", 50m, 5));

        Assert.Equal(100m, Assert.Single(result.Categories).RevenuePercent);
    }

    [Fact]
    public async Task Empty_Rows_Produce_Zero_Totals_And_No_Categories()
    {
        var result = await RunAsync();

        Assert.Equal(0m, result.TotalRevenue);
        Assert.Equal(0, result.TotalSalesCount);
        Assert.Empty(result.Categories);
    }

    [Fact]
    public async Task Missing_Category_Name_Falls_Back_To_Id()
    {
        var result = await RunAsync(new AdminCategorySalesRow("UNKNOWN", null, 10m, 1));

        Assert.Equal("UNKNOWN", Assert.Single(result.Categories).CategoryName);
    }

    [Fact]
    public async Task Inclusive_Date_Range_Maps_To_Half_Open_Timestamps()
    {
        var repository = new FakeRepository([]);
        var service = new AdminSalesAnalyticsService(repository);

        await service.GetSalesAnalyticsAsync(new DateOnly(2026, 6, 1), new DateOnly(2026, 6, 30), CancellationToken.None);

        Assert.Equal(new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc), repository.FromInclusive);
        Assert.Equal(new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc), repository.ToExclusive);
    }

    [Fact]
    public async Task Single_Day_Date_Range_Maps_To_Half_Open_Timestamps()
    {
        var repository = new FakeRepository([]);
        var service = new AdminSalesAnalyticsService(repository);

        await service.GetSalesAnalyticsAsync(new DateOnly(2026, 6, 15), new DateOnly(2026, 6, 15), CancellationToken.None);

        Assert.Equal(new DateTime(2026, 6, 15, 0, 0, 0, DateTimeKind.Utc), repository.FromInclusive);
        Assert.Equal(new DateTime(2026, 6, 16, 0, 0, 0, DateTimeKind.Utc), repository.ToExclusive);
    }

    [Fact]
    public async Task Service_Sorts_Categories_By_Revenue_Descending_And_CategoryId_Ascending()
    {
        var result = await RunAsync(
            new AdminCategorySalesRow("CATS", "Cats", 50.00m, 2),
            new AdminCategorySalesRow("FISH", "Fish", 120.00m, 4),
            new AdminCategorySalesRow("DOGS", "Dogs", 50.00m, 3),
            new AdminCategorySalesRow("BIRDS", "Birds", 73.00m, 2));

        Assert.Equal(4, result.Categories.Count);
        
        // 1st: FISH (120.00m) - highest revenue
        Assert.Equal("FISH", result.Categories[0].CategoryId);
        
        // 2nd: BIRDS (73.00m) - second highest revenue
        Assert.Equal("BIRDS", result.Categories[1].CategoryId);
        
        // 3rd & 4th have equal revenue (50.00m), so sorted alphabetically by CategoryId: CATS then DOGS
        Assert.Equal("CATS", result.Categories[2].CategoryId);
        Assert.Equal("DOGS", result.Categories[3].CategoryId);
    }

    [Theory]
    [InlineData(0, 100, 0)]
    [InlineData(100, 0, 0)]      // zero total guard
    [InlineData(50, 200, 25)]
    [InlineData(1, 3, 33.33)]
    public void ComputePercent_Rounds_To_Two_Decimals(decimal revenue, decimal total, decimal expected)
    {
        Assert.Equal(expected, AdminSalesAnalyticsService.ComputePercent(revenue, total));
    }

    [Theory]
    [InlineData(null, "2026-06-30", "required")]
    [InlineData("2026-06-01", null, "required")]
    [InlineData(null, null, "required")]
    [InlineData("", "", "required")]
    [InlineData("   ", "   ", "required")]
    [InlineData("garbage", "2026-06-30", "startDate")]
    [InlineData("2026-06-01", "30/06/2026", "endDate")]
    [InlineData("2026-07-01", "2026-06-30", "after")]
    public void DateRange_Validation_Reports_Problems(string? start, string? end, string expectedFragment)
    {
        var error = new SalesAnalyticsDateRangeRequest(start, end).TryParse(out _, out _);

        Assert.NotNull(error);
        Assert.Contains(expectedFragment, error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Valid_DateRange_Parses()
    {
        var error = new SalesAnalyticsDateRangeRequest("2026-06-01", "2026-06-30").TryParse(out var start, out var end);

        Assert.Null(error);
        Assert.Equal(new DateOnly(2026, 6, 1), start);
        Assert.Equal(new DateOnly(2026, 6, 30), end);
    }

    [Fact]
    public async Task Category_With_Zero_Revenue_But_Positive_Sales_Count_Gets_Zero_Percent()
    {
        var result = await RunAsync(
            new AdminCategorySalesRow("CATS", "Cats", 100m, 5),
            new AdminCategorySalesRow("FREE", "Free Gift", 0m, 2));

        Assert.Equal(100m, result.TotalRevenue);
        Assert.Equal(7, result.TotalSalesCount);
        Assert.Equal(2, result.Categories.Count);
        
        var freeCategory = result.Categories.Single(c => c.CategoryId == "FREE");
        Assert.Equal(0m, freeCategory.Revenue);
        Assert.Equal(0m, freeCategory.RevenuePercent);
        Assert.Equal(2, freeCategory.SalesCount);
    }

    [Fact]
    public async Task Multiple_Categories_With_Total_Revenue_Zero_Produces_Zero_Percent()
    {
        var result = await RunAsync(
            new AdminCategorySalesRow("CATS", "Cats", 0m, 5),
            new AdminCategorySalesRow("DOGS", "Dogs", 0m, 2));

        Assert.Equal(0m, result.TotalRevenue);
        Assert.Equal(7, result.TotalSalesCount);
        Assert.Equal(2, result.Categories.Count);
        Assert.All(result.Categories, c => Assert.Equal(0m, c.RevenuePercent));
    }
}
