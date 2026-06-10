using System.Net.Http.Headers;
using Petstore.Models;

namespace Petstore.Tests;

[Collection(PetstoreCatalogDatabaseCollection.Name)]
[Trait("Category", DatabaseIntegrationTestCategory.Name)]
public sealed class AuthApiContractTests(PetstoreCatalogTestsFixture fixture) : DatabaseIntegrationTestBase(fixture)
{
    [Fact]
    public async Task SignIn_With_Seeded_User_Returns_Token()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/signin", new SignInRequestDto("j2ee", "j2ee"));
        var body = await response.Content.ReadFromJsonAsync<SignInResponseDto>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body.Token));
        Assert.Equal("j2ee", body.UserId);
        Assert.Equal("customer", body.Role);
        Assert.True(body.ExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task SignIn_Wrong_Password_And_Unknown_User_Are_Indistinguishable()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var client = factory.CreateClient();

        var wrongPassword = await client.PostAsJsonAsync("/api/auth/signin", new SignInRequestDto("j2ee", "nope"));
        var unknownUser = await client.PostAsJsonAsync("/api/auth/signin", new SignInRequestDto("no-such-user", "nope"));

        Assert.Equal(HttpStatusCode.Unauthorized, wrongPassword.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, unknownUser.StatusCode);

        var wrongBody = await wrongPassword.Content.ReadFromJsonAsync<ApiErrorDto>();
        var unknownBody = await unknownUser.Content.ReadFromJsonAsync<ApiErrorDto>();
        Assert.NotNull(wrongBody);
        Assert.NotNull(unknownBody);
        Assert.Equal(wrongBody, unknownBody);
    }

    [Fact]
    public async Task Token_Grants_Access_To_Authenticated_Endpoint()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var client = factory.CreateClient();

        var signIn = await client.PostAsJsonAsync("/api/auth/signin", new SignInRequestDto("j2ee", "j2ee"));
        var token = (await signIn.Content.ReadFromJsonAsync<SignInResponseDto>())!.Token;

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var account = await client.GetAsync("/api/account");

        Assert.Equal(HttpStatusCode.OK, account.StatusCode);
    }

    [Fact]
    public async Task Authenticated_Endpoint_Rejects_Anonymous_Caller()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/account");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Catalog_Endpoints_Stay_Anonymous()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/catalog/categories");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
