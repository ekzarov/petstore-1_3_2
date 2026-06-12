using Microsoft.EntityFrameworkCore;
using Petstore.Accounts;

namespace Petstore.Tests;

[Collection(PetstoreCatalogDatabaseCollection.Name)]
[Trait("Category", DatabaseIntegrationTestCategory.Name)]
public sealed class AccountSeederTests(PetstoreCatalogTestsFixture fixture) : DatabaseIntegrationTestBase(fixture)
{
    [Fact]
    public async Task Seeder_Creates_Parity_Users_And_Is_Idempotent()
    {
        var hasher = new Pbkdf2PasswordHasher();

        await using (var context = Fixture.CreateContext())
        {
            await AccountSeeder.SeedAsync(context, hasher);
            await AccountSeeder.SeedAsync(context, hasher);
        }

        await using (var context = Fixture.CreateContext())
        {
            var users = await context.Users.AsNoTracking().OrderBy(user => user.UserId).ToListAsync();

            Assert.Equal(["admin", "j2ee", "j2ee-ja", "shopper", "supplier"], users.Select(user => user.UserId));
            Assert.Equal("admin", users.Single(user => user.UserId == "admin").Role);
            Assert.Equal("supplier", users.Single(user => user.UserId == "supplier").Role);
            Assert.All(
                users.Where(user => user.UserId is not "admin" and not "supplier"),
                user => Assert.Equal("customer", user.Role));

            var j2ee = users.Single(user => user.UserId == "j2ee");
            Assert.True(hasher.Verify("j2ee", j2ee.PasswordHash, j2ee.PasswordSalt));
        }
    }
}
