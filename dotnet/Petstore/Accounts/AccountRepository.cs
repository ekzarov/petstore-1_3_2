using Microsoft.EntityFrameworkCore;
using Petstore.Data;
using Petstore.Data.Entities;

namespace Petstore.Accounts;

public sealed class AccountRepository(PetstoreCatalogContext context) : IAccountRepository
{
    public Task<UserEntity?> FindUserAsync(string userId, CancellationToken cancellationToken)
    {
        return context.Users.AsNoTracking().SingleOrDefaultAsync(user => user.UserId == userId, cancellationToken);
    }

    public Task<bool> UserExistsAsync(string userId, CancellationToken cancellationToken)
    {
        return context.Users.AnyAsync(user => user.UserId == userId, cancellationToken);
    }

    public async Task CreateAccountAsync(UserEntity user, CancellationToken cancellationToken)
    {
        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task<CustomerContactEntity?> GetContactAsync(string userId, CancellationToken cancellationToken)
    {
        return context.CustomerContacts.AsNoTracking().SingleOrDefaultAsync(contact => contact.UserId == userId, cancellationToken);
    }

    public async Task UpdateContactAsync(CustomerContactEntity contact, CancellationToken cancellationToken)
    {
        var existing = await context.CustomerContacts.SingleOrDefaultAsync(c => c.UserId == contact.UserId, cancellationToken);
        if (existing is null)
        {
            context.CustomerContacts.Add(contact);
        }
        else
        {
            context.Entry(existing).CurrentValues.SetValues(contact);
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
