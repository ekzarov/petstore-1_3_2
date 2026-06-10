namespace Petstore.Cart;

public sealed record CartIdentity(string? Key, string? AnonymousKey)
{
    public bool HasIdentity => Key is not null;
}
