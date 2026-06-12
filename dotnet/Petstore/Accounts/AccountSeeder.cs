using Microsoft.EntityFrameworkCore;
using Petstore.Data;
using Petstore.Data.Entities;

namespace Petstore.Accounts;

public static class AccountSeeder
{
    private static readonly (string UserId, string Password, string Role)[] SeedUsers =
    [
        ("j2ee", "j2ee", AccountModelConstants.Roles.Customer),
        ("j2ee-ja", "j2ee", AccountModelConstants.Roles.Customer),
        ("shopper", "j2ee", AccountModelConstants.Roles.Customer),
        ("admin", "admin", AccountModelConstants.Roles.Admin),
        ("supplier", "supplier", AccountModelConstants.Roles.Supplier)
    ];

    public static async Task SeedAsync(PetstoreCatalogContext context, IPasswordHasher passwordHasher, CancellationToken cancellationToken = default)
    {
        foreach (var (userId, password, role) in SeedUsers)
        {
            var exists = await context.Users.AnyAsync(user => user.UserId == userId, cancellationToken);
            if (exists)
            {
                continue;
            }

            var (hash, salt) = passwordHasher.Hash(password);
            context.Users.Add(new UserEntity
            {
                UserId = userId,
                PasswordHash = hash,
                PasswordSalt = salt,
                Role = role,
                CreatedAt = DateTime.UtcNow
            });
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
