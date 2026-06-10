using System.Net.Http.Headers;
using Petstore.Models;

namespace Petstore.Tests;

[Collection(PetstoreCatalogDatabaseCollection.Name)]
[Trait("Category", DatabaseIntegrationTestCategory.Name)]
public sealed class CartMergeTests(PetstoreCatalogTestsFixture fixture) : DatabaseIntegrationTestBase(fixture)
{
    private const string CartHeader = "X-Cart-Id";

    private static async Task<string> SignInAsync(HttpClient client, string userId, string password)
    {
        var response = await client.PostAsJsonAsync("/api/auth/signin", new SignInRequestDto(userId, password));
        return (await response.Content.ReadFromJsonAsync<SignInResponseDto>())!.Token;
    }

    [Fact]
    public async Task Anonymous_Cart_Merges_Into_User_Cart_On_Sign_In()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        var cartId = Guid.NewGuid().ToString();

        // Build an anonymous cart.
        using (var anonymous = factory.CreateClient())
        {
            anonymous.DefaultRequestHeaders.Add(CartHeader, cartId);
            await anonymous.PostAsJsonAsync("/api/cart/items", new AddCartItemRequestDto("EST-1"));
            await anonymous.PostAsJsonAsync("/api/cart/items", new AddCartItemRequestDto("EST-1"));
        }

        // Authenticated request still carrying the anonymous header triggers the merge.
        using var authenticated = factory.CreateClient();
        authenticated.DefaultRequestHeaders.Add(CartHeader, cartId);
        var token = await SignInAsync(authenticated, "j2ee", "j2ee");
        authenticated.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var merged = await authenticated.GetFromJsonAsync<CartDto>("/api/cart");
        Assert.NotNull(merged);
        var line = Assert.Single(merged.Lines);
        Assert.Equal("EST-1", line.ItemId);
        Assert.Equal(2, line.Quantity);

        // The anonymous cart is gone: the same header without auth now yields an empty cart.
        using var anonymousAgain = factory.CreateClient();
        anonymousAgain.DefaultRequestHeaders.Add(CartHeader, cartId);
        var leftover = await anonymousAgain.GetFromJsonAsync<CartDto>("/api/cart");
        Assert.NotNull(leftover);
        Assert.Empty(leftover.Lines);
    }

    [Fact]
    public async Task Merge_Sums_Quantities_With_Existing_User_Lines_And_Runs_Once()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        var cartId = Guid.NewGuid().ToString();

        // The signed-in user already has EST-1 x1.
        using var authenticated = factory.CreateClient();
        var token = await SignInAsync(authenticated, "shopper", "j2ee");
        authenticated.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        await authenticated.PostAsJsonAsync("/api/cart/items", new AddCartItemRequestDto("EST-1"));

        // An anonymous cart holds EST-1 x1 and EST-2 x1.
        using (var anonymous = factory.CreateClient())
        {
            anonymous.DefaultRequestHeaders.Add(CartHeader, cartId);
            await anonymous.PostAsJsonAsync("/api/cart/items", new AddCartItemRequestDto("EST-1"));
            await anonymous.PostAsJsonAsync("/api/cart/items", new AddCartItemRequestDto("EST-2"));
        }

        // First authenticated call with the header merges; repeating it must not double quantities.
        authenticated.DefaultRequestHeaders.Add(CartHeader, cartId);
        var first = await authenticated.GetFromJsonAsync<CartDto>("/api/cart");
        var second = await authenticated.GetFromJsonAsync<CartDto>("/api/cart");

        Assert.NotNull(first);
        Assert.NotNull(second);
        Assert.Equal(2, second.Lines.Count);
        Assert.Equal(2, second.Lines.Single(line => line.ItemId == "EST-1").Quantity);
        Assert.Equal(1, second.Lines.Single(line => line.ItemId == "EST-2").Quantity);
        Assert.Equal(first.Total, second.Total);
    }

    [Fact]
    public async Task User_Cart_Follows_Identity_Without_Header()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);

        using var session1 = factory.CreateClient();
        var token = await SignInAsync(session1, "j2ee", "j2ee");
        session1.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        await session1.PostAsJsonAsync("/api/cart/items", new AddCartItemRequestDto("EST-2"));

        // A fresh client (new browser/session) with only the token sees the same cart.
        using var session2 = factory.CreateClient();
        session2.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var cart = await session2.GetFromJsonAsync<CartDto>("/api/cart");

        Assert.NotNull(cart);
        Assert.Equal("EST-2", Assert.Single(cart.Lines).ItemId);
    }
}
