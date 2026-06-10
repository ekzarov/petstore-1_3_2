using Petstore.Data.Entities;

namespace Petstore.Accounts;

public interface IAccountRepository
{
    Task<UserEntity?> FindUserAsync(string userId, CancellationToken cancellationToken);

    Task<bool> UserExistsAsync(string userId, CancellationToken cancellationToken);

    Task CreateAccountAsync(UserEntity user, CancellationToken cancellationToken);

    Task<CustomerContactEntity?> GetContactAsync(string userId, CancellationToken cancellationToken);

    Task UpdateContactAsync(CustomerContactEntity contact, CancellationToken cancellationToken);
}
