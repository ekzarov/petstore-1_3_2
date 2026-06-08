using Microsoft.EntityFrameworkCore;
using Petstore.Data;
using Petstore.Data.Entities;

namespace Petstore.Catalog;

public sealed class CatalogSeeder(PetstoreCatalogContext context)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await UpsertCategoryAsync(new CategoryEntity { Id = "FISH", Name = "Fish" }, cancellationToken);
        await UpsertCategoryAsync(new CategoryEntity { Id = "DOGS", Name = "Dogs" }, cancellationToken);
        await UpsertCategoryAsync(new CategoryEntity { Id = "REPTILES", Name = "Reptiles" }, cancellationToken);
        await UpsertCategoryAsync(new CategoryEntity { Id = "CATS", Name = "Cats" }, cancellationToken);
        await UpsertCategoryAsync(new CategoryEntity { Id = "BIRDS", Name = "Birds" }, cancellationToken);

        await UpsertProductAsync(
            new ProductEntity
            {
                Id = "FI-SW-01",
                CategoryId = "FISH",
                Name = "Angelfish",
                Description = "Salt Water fish from Australia"
            },
            cancellationToken);

        await UpsertProductAsync(
            new ProductEntity
            {
                Id = "FI-FW-02",
                CategoryId = "FISH",
                Name = "Goldfish",
                Description = "Fresh Water fish from China"
            },
            cancellationToken);

        await UpsertItemAsync(
            new ItemEntity
            {
                Id = "EST-1",
                ProductId = "FI-SW-01",
                Name = "Large Angelfish",
                Attributes = ["Large", "Cuddly"],
                Description = "Fresh Water fish from Japan",
                Price = 16.50m,
                Currency = "USD"
            },
            cancellationToken);

        await UpsertItemAsync(
            new ItemEntity
            {
                Id = "EST-2",
                ProductId = "FI-SW-01",
                Name = "Small Angelfish",
                Attributes = ["Small"],
                Description = "Fresh Water fish from Japan",
                Price = 16.50m,
                Currency = "USD"
            },
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task UpsertCategoryAsync(CategoryEntity seed, CancellationToken cancellationToken)
    {
        var existing = await context.Categories.FindAsync([seed.Id], cancellationToken);
        if (existing is null)
        {
            context.Categories.Add(seed);
            return;
        }

        existing.Name = seed.Name;
        existing.Description = seed.Description;
    }

    private async Task UpsertProductAsync(ProductEntity seed, CancellationToken cancellationToken)
    {
        var existing = await context.Products.FindAsync([seed.Id], cancellationToken);
        if (existing is null)
        {
            context.Products.Add(seed);
            return;
        }

        existing.CategoryId = seed.CategoryId;
        existing.Name = seed.Name;
        existing.Description = seed.Description;
    }

    private async Task UpsertItemAsync(ItemEntity seed, CancellationToken cancellationToken)
    {
        var existing = await context.Items.FindAsync([seed.Id], cancellationToken);
        if (existing is null)
        {
            context.Items.Add(seed);
            return;
        }

        existing.ProductId = seed.ProductId;
        existing.Name = seed.Name;
        existing.Attributes = seed.Attributes;
        existing.Description = seed.Description;
        existing.Price = seed.Price;
        existing.Currency = seed.Currency;
    }
}
