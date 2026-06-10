using System.Security.Claims;

namespace Petstore.Cart;

public static class CartIdentityResolver
{
    public const string CartIdHeader = "X-Cart-Id";

    public static CartIdentity Resolve(HttpContext httpContext)
    {
        var header = httpContext.Request.Headers[CartIdHeader].FirstOrDefault();
        var anonymousKey = string.IsNullOrWhiteSpace(header) ? null : $"anon:{header}";

        var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrWhiteSpace(userId))
        {
            // Authenticated: the cart follows the user. A still-present anonymous
            // key is reported so the repository can merge it in once.
            return new CartIdentity($"user:{userId}", anonymousKey);
        }

        return new CartIdentity(anonymousKey, null);
    }
}
