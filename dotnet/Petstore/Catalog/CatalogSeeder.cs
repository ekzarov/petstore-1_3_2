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
                Id = "FI-SW-02",
                CategoryId = "FISH",
                Name = "Tiger Shark",
                Description = "Salt Water fish from Australia"
            },
            new ProductEntity
            {
                Id = "FI-FW-01",
                CategoryId = "FISH",
                Name = "Koi",
                Description = "Fresh Water fish from Japan"
            },
            new ProductEntity
            {
                Id = "FI-FW-02",
                CategoryId = "FISH",
                Name = "Goldfish",
                Description = "Fresh Water fish from China"
            },
            new ProductEntity
            {
                Id = "K9-BD-01",
                CategoryId = "DOGS",
                Name = "Bulldog",
                Description = "Friendly dog from England"
            },
            new ProductEntity
            {
                Id = "K9-PO-02",
                CategoryId = "DOGS",
                Name = "Poodle",
                Description = "Cute dog from France"
            },
            new ProductEntity
            {
                Id = "K9-DL-01",
                CategoryId = "DOGS",
                Name = "Dalmation",
                Description = "Great dog for a Fire Station"
            },
            new ProductEntity
            {
                Id = "K9-RT-01",
                CategoryId = "DOGS",
                Name = "Golden Retriever",
                Description = "Great family dog"
            },
            new ProductEntity
            {
                Id = "K9-RT-02",
                CategoryId = "DOGS",
                Name = "Labrador Retriever",
                Description = "Great hunting dog"
            },
            new ProductEntity
            {
                Id = "K9-CW-01",
                CategoryId = "DOGS",
                Name = "Chihuahua",
                Description = "Great companion dog"
            },
            new ProductEntity
            {
                Id = "RP-SN-01",
                CategoryId = "REPTILES",
                Name = "Rattlesnake",
                Description = "Doubles as a watch dog"
            },
            new ProductEntity
            {
                Id = "RP-LI-02",
                CategoryId = "REPTILES",
                Name = "Iguana",
                Description = "Friendly green friend"
            },
            new ProductEntity
            {
                Id = "FL-DSH-01",
                CategoryId = "CATS",
                Name = "Manx",
                Description = "Great for reducing mouse populations"
            },
            new ProductEntity
            {
                Id = "FL-DLH-02",
                CategoryId = "CATS",
                Name = "Persian",
                Description = "Friendly house cat, doubles as a princess"
            },
            new ProductEntity
            {
                Id = "AV-CB-01",
                CategoryId = "BIRDS",
                Name = "Amazon Parrot",
                Description = "Great companion for up to 75 years"
            },
            new ProductEntity
            {
                Id = "AV-SB-02",
                CategoryId = "BIRDS",
                Name = "Finch",
                Description = "Great stress reliever"
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
            },
            new ItemEntity
            {
                Id = "EST-3",
                ProductId = "FI-SW-02",
                Name = "Toothless Tiger Shark",
                Attributes = ["Toothless", "Mean"],
                Description = "Salt Water fish from Australia",
                Price = 18.50m,
                Currency = "USD"
            },
            new ItemEntity
            {
                Id = "EST-4",
                ProductId = "FI-FW-01",
                Name = "Spotted Koi",
                Attributes = ["Spotted"],
                Description = "Fresh Water fish from Japan",
                Price = 18.50m,
                Currency = "USD"
            },
            new ItemEntity
            {
                Id = "EST-5",
                ProductId = "FI-FW-01",
                Name = "Spotless Koi",
                Attributes = ["Spotless"],
                Description = "Fresh Water fish from Japan",
                Price = 18.50m,
                Currency = "USD"
            },
            new ItemEntity
            {
                Id = "EST-6",
                ProductId = "K9-BD-01",
                Name = "Male Adult Bulldog",
                Attributes = ["Male Adult"],
                Description = "Friendly dog from England",
                Price = 18.50m,
                Currency = "USD"
            },
            new ItemEntity
            {
                Id = "EST-7",
                ProductId = "K9-BD-01",
                Name = "Female Puppy Bulldog",
                Attributes = ["Female Puppy"],
                Description = "Friendly dog from England",
                Price = 18.50m,
                Currency = "USD"
            },
            new ItemEntity
            {
                Id = "EST-8",
                ProductId = "K9-PO-02",
                Name = "Male Puppy Poodle",
                Attributes = ["Male Puppy"],
                Description = "Cute dog from France",
                Price = 18.50m,
                Currency = "USD"
            },
            new ItemEntity
            {
                Id = "EST-9",
                ProductId = "K9-DL-01",
                Name = "Spotless Male Puppy Dalmation",
                Attributes = ["Spotless Male Puppy"],
                Description = "Great dog for a Fire Station",
                Price = 18.50m,
                Currency = "USD"
            },
            new ItemEntity
            {
                Id = "EST-10",
                ProductId = "K9-DL-01",
                Name = "Spotted Adult Female Dalmation",
                Attributes = ["Spotted Adult Female"],
                Description = "Great dog for a Fire Station",
                Price = 18.50m,
                Currency = "USD"
            },
            new ItemEntity
            {
                Id = "EST-11",
                ProductId = "RP-SN-01",
                Name = "Venomless Rattlesnake",
                Attributes = ["Venomless"],
                Description = "More Bark than bite",
                Price = 18.50m,
                Currency = "USD"
            },
            new ItemEntity
            {
                Id = "EST-12",
                ProductId = "RP-SN-01",
                Name = "Rattleless Rattlesnake",
                Attributes = ["Rattleless"],
                Description = "Doubles as a watch dog",
                Price = 18.50m,
                Currency = "USD"
            },
            new ItemEntity
            {
                Id = "EST-13",
                ProductId = "RP-LI-02",
                Name = "Green Adult Iguana",
                Attributes = ["Green Adult"],
                Description = "Friendly green friend",
                Price = 12.50m,
                Currency = "USD"
            },
            new ItemEntity
            {
                Id = "EST-14",
                ProductId = "FL-DSH-01",
                Name = "Tailless Manx",
                Attributes = ["Tailless"],
                Description = "Great for reducing mouse populations",
                Price = 58.50m,
                Currency = "USD"
            },
            new ItemEntity
            {
                Id = "EST-15",
                ProductId = "FL-DSH-01",
                Name = "With tail Manx",
                Attributes = ["With tail"],
                Description = "Great for reducing mouse populations",
                Price = 23.50m,
                Currency = "USD"
            },
            new ItemEntity
            {
                Id = "EST-16",
                ProductId = "FL-DLH-02",
                Name = "Adult Female Persian",
                Attributes = ["Adult Female"],
                Description = "Friendly house cat, doubles as a princess",
                Price = 93.50m,
                Currency = "USD"
            },
            new ItemEntity
            {
                Id = "EST-17",
                ProductId = "FL-DLH-02",
                Name = "Adult Male Persian",
                Attributes = ["Adult Male"],
                Description = "Friendly house cat, doubles as a prince",
                Price = 93.50m,
                Currency = "USD"
            },
            new ItemEntity
            {
                Id = "EST-18",
                ProductId = "AV-CB-01",
                Name = "Adult Male Amazon Parrot",
                Attributes = ["Adult Male"],
                Description = "Great companion for up to 75 years",
                Price = 193.50m,
                Currency = "USD"
            },
            new ItemEntity
            {
                Id = "EST-19",
                ProductId = "AV-SB-02",
                Name = "Adult Male Finch",
                Attributes = ["Adult Male"],
                Description = "Great stress reliever",
                Price = 15.50m,
                Currency = "USD"
            },
            new ItemEntity
            {
                Id = "EST-20",
                ProductId = "FI-FW-02",
                Name = "Adult Male Goldfish",
                Attributes = ["Adult Male"],
                Description = "Fresh Water fish from China",
                Price = 5.50m,
                Currency = "USD"
            },
            new ItemEntity
            {
                Id = "EST-21",
                ProductId = "FI-FW-02",
                Name = "Adult Female Goldfish",
                Attributes = ["Adult Female"],
                Description = "Fresh Water fish from China",
                Price = 5.29m,
                Currency = "USD"
            },
            new ItemEntity
            {
                Id = "EST-22",
                ProductId = "K9-RT-02",
                Name = "Adult Male Labrador Retriever",
                Attributes = ["Adult Male"],
                Description = "Great hunting dog",
                Price = 135.50m,
                Currency = "USD"
            },
            new ItemEntity
            {
                Id = "EST-23",
                ProductId = "K9-RT-02",
                Name = "Adult Female Labrador Retriever",
                Attributes = ["Adult Female"],
                Description = "Great hunting dog",
                Price = 145.49m,
                Currency = "USD"
            },
            new ItemEntity
            {
                Id = "EST-24",
                ProductId = "K9-RT-02",
                Name = "Male Puppy Labrador Retriever",
                Attributes = ["Male Puppy"],
                Description = "Great addition to a family",
                Price = 255.50m,
                Currency = "USD"
            },
            new ItemEntity
            {
                Id = "EST-25",
                ProductId = "K9-RT-02",
                Name = "Female Puppy Labrador Retriever",
                Attributes = ["Female Puppy"],
                Description = "Great hunting dog",
                Price = 325.29m,
                Currency = "USD"
            },
            new ItemEntity
            {
                Id = "EST-26",
                ProductId = "K9-CW-01",
                Name = "Adult Male Chihuahua",
                Attributes = ["Adult Male"],
                Description = "Little yapper",
                Price = 125.50m,
                Currency = "USD"
            },
            new ItemEntity
            {
                Id = "EST-27",
                ProductId = "K9-CW-01",
                Name = "Adult Female Chihuahua",
                Attributes = ["Adult Female"],
                Description = "Great companion dog",
                Price = 155.29m,
                Currency = "USD"
            },
            new ItemEntity
            {
                Id = "EST-28",
                ProductId = "K9-RT-01",
                Name = "Adult Female Golden Retriever",
                Attributes = ["Adult Female"],
                Description = "Great family dog",
                Price = 155.29m,
                Currency = "USD"
            });
    }
}
