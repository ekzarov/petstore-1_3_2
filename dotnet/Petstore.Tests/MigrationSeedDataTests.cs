using Microsoft.EntityFrameworkCore;
using Petstore.Accounts;

namespace Petstore.Tests;

[Collection(PetstoreCatalogDatabaseCollection.Name)]
[Trait("Category", DatabaseIntegrationTestCategory.Name)]
public sealed class MigrationSeedDataTests(PetstoreCatalogTestsFixture fixture) : DatabaseIntegrationTestBase(fixture)
{
    [Fact]
    public async Task Migrations_Create_Demo_Users_Contacts_And_Inventory()
    {
        await using var context = Fixture.CreateContext();
        var hasher = new Pbkdf2PasswordHasher();

        var users = await context.Users.AsNoTracking().OrderBy(user => user.UserId).ToListAsync();
        Assert.Equal(["admin", "j2ee", "j2ee-ja", "shopper", "supplier"], users.Select(user => user.UserId));
        Assert.Equal("admin", users.Single(user => user.UserId == "admin").Role);
        Assert.Equal("supplier", users.Single(user => user.UserId == "supplier").Role);
        Assert.All(
            users.Where(user => user.UserId is not "admin" and not "supplier"),
            user => Assert.Equal("customer", user.Role));

        Assert.True(hasher.Verify("j2ee", users.Single(user => user.UserId == "j2ee").PasswordHash, users.Single(user => user.UserId == "j2ee").PasswordSalt));
        Assert.True(hasher.Verify("admin", users.Single(user => user.UserId == "admin").PasswordHash, users.Single(user => user.UserId == "admin").PasswordSalt));
        Assert.True(hasher.Verify("supplier", users.Single(user => user.UserId == "supplier").PasswordHash, users.Single(user => user.UserId == "supplier").PasswordSalt));

        var contacts = await context.CustomerContacts.AsNoTracking().OrderBy(contact => contact.UserId).ToListAsync();
        Assert.Equal(["admin", "j2ee", "supplier"], contacts.Select(contact => contact.UserId));
        Assert.Equal("admin@petstore.example", contacts.Single(contact => contact.UserId == "admin").Email);
        Assert.Equal("j2ee@petstore.example", contacts.Single(contact => contact.UserId == "j2ee").Email);
        Assert.Equal("supplier@petstore.example", contacts.Single(contact => contact.UserId == "supplier").Email);

        var itemIds = await context.Items.AsNoTracking().Select(item => item.Id).OrderBy(id => id).ToListAsync();
        var inventory = await context.SupplierInventory.AsNoTracking().OrderBy(row => row.ItemId).ToListAsync();
        Assert.Equal(itemIds, inventory.Select(row => row.ItemId));
        Assert.All(inventory, row => Assert.Equal(100, row.QuantityOnHand));
    }
}
