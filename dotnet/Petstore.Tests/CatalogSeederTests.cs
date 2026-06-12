using Microsoft.EntityFrameworkCore;
using Petstore.Data.Entities;

namespace Petstore.Tests;

[Collection(PetstoreCatalogDatabaseCollection.Name)]
[Trait("Category", DatabaseIntegrationTestCategory.Name)]
public sealed class CatalogSeederTests(PetstoreCatalogTestsFixture fixture) : DatabaseIntegrationTestBase(fixture)
{
    [Fact]
    public async Task MigrationSeedData_Contains_Unique_Legacy_Catalog_Ids()
    {
        await using var context = Fixture.CreateContext();

        var categoryIds = await context.Categories.Select(category => category.Id).ToListAsync();
        var productIds = await context.Products.Select(product => product.Id).ToListAsync();
        var itemIds = await context.Items.Select(item => item.Id).ToListAsync();

        Assert.Equal(5, categoryIds.Count);
        Assert.Equal(categoryIds.Count, categoryIds.Distinct().Count());
        Assert.Contains("FISH", categoryIds);
        Assert.Contains("DOGS", categoryIds);
        Assert.Contains("REPTILES", categoryIds);
        Assert.Contains("CATS", categoryIds);
        Assert.Contains("BIRDS", categoryIds);

        Assert.Equal(16, productIds.Count);
        Assert.Equal(productIds.Count, productIds.Distinct().Count());
        Assert.Contains("FI-SW-01", productIds);
        Assert.Contains("FI-FW-02", productIds);
        Assert.Contains("K9-BD-01", productIds);
        Assert.Contains("RP-LI-02", productIds);
        Assert.Contains("FL-DSH-01", productIds);
        Assert.Contains("AV-CB-01", productIds);

        Assert.Equal(28, itemIds.Count);
        Assert.Equal(itemIds.Count, itemIds.Distinct().Count());
        Assert.Contains("EST-1", itemIds);
        Assert.Contains("EST-2", itemIds);
        Assert.Contains("EST-6", itemIds);
        Assert.Contains("EST-13", itemIds);
        Assert.Contains("EST-14", itemIds);
        Assert.Contains("EST-18", itemIds);
    }

    [Fact]
    public async Task MigrationSeedData_Preserves_Fk_Relationships_And_Representative_Values()
    {
        await using var context = Fixture.CreateContext();

        var angelfish = await context.Products.SingleAsync(product => product.Id == "FI-SW-01");
        var largeAngelfish = await context.Items.SingleAsync(item => item.Id == "EST-1");

        Assert.Equal("FISH", angelfish.CategoryId);
        Assert.Equal("Angelfish", angelfish.Name);
        Assert.Equal("Salt Water fish from Australia", angelfish.Description);

        Assert.Equal("FI-SW-01", largeAngelfish.ProductId);
        Assert.Equal("Large Angelfish", largeAngelfish.Name);
        Assert.Equal(["Large", "Cuddly"], largeAngelfish.Attributes);
        Assert.Equal(16.50m, largeAngelfish.Price);
        Assert.Equal("USD", largeAngelfish.Currency);

        var bulldog = await context.Products.SingleAsync(product => product.Id == "K9-BD-01");
        var maleBulldog = await context.Items.SingleAsync(item => item.Id == "EST-6");

        Assert.Equal("DOGS", bulldog.CategoryId);
        Assert.Equal("Bulldog", bulldog.Name);
        Assert.Equal("Friendly dog from England", bulldog.Description);

        Assert.Equal("K9-BD-01", maleBulldog.ProductId);
        Assert.Equal("Male Adult Bulldog", maleBulldog.Name);
        Assert.Equal(["Male Adult"], maleBulldog.Attributes);
        Assert.Equal(18.50m, maleBulldog.Price);
        Assert.Equal("USD", maleBulldog.Currency);
    }

    [Fact]
    public async Task MigrationSeedData_Is_Idempotent_When_Migrations_Are_Applied_Again()
    {
        await Fixture.ApplyMigrationsAsync();

        await using var context = Fixture.CreateContext();

        Assert.Equal(5, await context.Categories.CountAsync());
        Assert.Equal(16, await context.Products.CountAsync());
        Assert.Equal(28, await context.Items.CountAsync());
    }

    [Fact]
    public async Task Database_Enforces_Product_Category_Foreign_Key()
    {
        await using var context = Fixture.CreateContext();

        context.Products.Add(new ProductEntity
        {
            Id = "BROKEN-PRODUCT",
            CategoryId = "UNKNOWN",
            Name = "Broken product"
        });

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }
}
