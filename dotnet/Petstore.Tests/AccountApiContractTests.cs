using System.Net.Http.Headers;
using Petstore.Models;

namespace Petstore.Tests;

[Collection(PetstoreCatalogDatabaseCollection.Name)]
[Trait("Category", DatabaseIntegrationTestCategory.Name)]
public sealed class AccountApiContractTests(PetstoreCatalogTestsFixture fixture) : DatabaseIntegrationTestBase(fixture)
{
    private static readonly ContactInfoDto ValidContact = new(
        "Doe", "Jane", "1 Main Street", null, "Springfield", "IL", "62701", "USA", "jane@example.com", "555-0100");

    [Fact]
    public async Task Register_Creates_Account_That_Can_Sign_In()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var client = factory.CreateClient();

        var register = await client.PostAsJsonAsync(
            "/api/account",
            new RegisterRequestDto("jane", "secret-1", ValidContact));
        var created = await register.Content.ReadFromJsonAsync<AccountDto>();

        Assert.Equal(HttpStatusCode.Created, register.StatusCode);
        Assert.NotNull(created);
        Assert.Equal("jane", created.UserId);
        Assert.Equal(ValidContact, created.Contact);

        var signIn = await client.PostAsJsonAsync("/api/auth/signin", new SignInRequestDto("jane", "secret-1"));
        Assert.Equal(HttpStatusCode.OK, signIn.StatusCode);
    }

    [Fact]
    public async Task Register_Duplicate_UserId_Returns_Conflict()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/account",
            new RegisterRequestDto("j2ee", "another-password", ValidContact));
        var error = await response.Content.ReadFromJsonAsync<ApiErrorDto>();

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.NotNull(error);
        Assert.Equal("account.duplicate_user", error.Code);

        // Existing account is unchanged: original password still works.
        var signIn = await client.PostAsJsonAsync("/api/auth/signin", new SignInRequestDto("j2ee", "j2ee"));
        Assert.Equal(HttpStatusCode.OK, signIn.StatusCode);
    }

    [Fact]
    public async Task Register_Missing_Fields_Names_Them()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/account",
            new RegisterRequestDto(null, null, ValidContact with { Email = "" }));
        var error = await response.Content.ReadFromJsonAsync<ApiErrorDto>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(error);
        Assert.Equal("account.validation", error.Code);
        Assert.Contains("userId", error.Message);
        Assert.Contains("password", error.Message);
        Assert.Contains("contact.email", error.Message);
    }

    [Fact]
    public async Task Register_Response_Never_Contains_Password_Material()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/account",
            new RegisterRequestDto("nopass", "super-secret-value", ValidContact));
        var raw = await response.Content.ReadAsStringAsync();

        Assert.DoesNotContain("super-secret-value", raw);
        Assert.DoesNotContain("password", raw, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash", raw, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Account_Read_And_Update_Round_Trip()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var client = factory.CreateClient();

        await client.PostAsJsonAsync("/api/account", new RegisterRequestDto("roundtrip", "pw-123456", ValidContact));
        var signIn = await client.PostAsJsonAsync("/api/auth/signin", new SignInRequestDto("roundtrip", "pw-123456"));
        var token = (await signIn.Content.ReadFromJsonAsync<SignInResponseDto>())!.Token;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var initial = await client.GetFromJsonAsync<AccountDto>("/api/account");
        Assert.NotNull(initial);
        Assert.Equal(ValidContact, initial.Contact);

        var updatedContact = ValidContact with { Street1 = "42 New Street", City = "Shelbyville" };
        var update = await client.PutAsJsonAsync("/api/account/contact", updatedContact);
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);

        var reloaded = await client.GetFromJsonAsync<AccountDto>("/api/account");
        Assert.NotNull(reloaded);
        Assert.Equal(updatedContact, reloaded.Contact);
    }

    [Fact]
    public async Task Update_Contact_Validates_Fields()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var client = factory.CreateClient();

        var signIn = await client.PostAsJsonAsync("/api/auth/signin", new SignInRequestDto("j2ee", "j2ee"));
        var token = (await signIn.Content.ReadFromJsonAsync<SignInResponseDto>())!.Token;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PutAsJsonAsync("/api/account/contact", ValidContact with { City = " " });
        var error = await response.Content.ReadFromJsonAsync<ApiErrorDto>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(error);
        Assert.Contains("contact.city", error.Message);
    }

    [Fact]
    public async Task Update_Contact_Rejects_Anonymous()
    {
        using var factory = new CatalogApiFactory(Fixture.ConnectionString);
        using var client = factory.CreateClient();

        var response = await client.PutAsJsonAsync("/api/account/contact", ValidContact);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
