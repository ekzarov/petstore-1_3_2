using Petstore.Data.Entities;

namespace Petstore.Cart;

public interface ICartRepository
{
    Task<IReadOnlyList<CartLineEntity>> GetLinesAsync(string cartKey, CancellationToken cancellationToken);

    Task AddItemAsync(string cartKey, string itemId, CancellationToken cancellationToken);

    Task<bool> SetQuantityAsync(string cartKey, string itemId, int quantity, CancellationToken cancellationToken);

    Task<bool> RemoveLineAsync(string cartKey, string itemId, CancellationToken cancellationToken);

    Task EmptyAsync(string cartKey, CancellationToken cancellationToken);

    Task MergeAsync(string anonymousKey, string userKey, CancellationToken cancellationToken);
}
