using Microsoft.EntityFrameworkCore;
using Petstore.Data.Entities;

namespace Petstore.Catalog;

public static class CatalogSeeder
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CategoryEntity>().HasData(
            new CategoryEntity { Id = "FISH", Name = "Fish" },
            new CategoryEntity { Id = "DOGS", Name = "Dogs" },
            new CategoryEntity { Id = "REPTILES", Name = "Reptiles" },
            new CategoryEntity { Id = "CATS", Name = "Cats" },
            new CategoryEntity { Id = "BIRDS", Name = "Birds" });

        modelBuilder.Entity<ProductEntity>().HasData(
            new ProductEntity
            {
                Id = "FI-SW-01",
                CategoryId = "FISH",
                Name = "Angelfish",
                Description = "Salt Water fish from Australia"
            },
            new ProductEntity
            {
                Id = "FI-FW-02",
                CategoryId = "FISH",
                Name = "Goldfish",
                Description = "Fresh Water fish from China"
            });

        modelBuilder.Entity<ItemEntity>().HasData(
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
            new ItemEntity
            {
                Id = "EST-2",
                ProductId = "FI-SW-01",
                Name = "Small Angelfish",
                Attributes = ["Small"],
                Description = "Fresh Water fish from Japan",
                Price = 16.50m,
                Currency = "USD"
            });
    }
}
