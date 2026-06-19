using Microsoft.Extensions.Configuration;
using Petstore.Accounts;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Petstore.Tests;

public sealed class JwtTokenServiceTests
{
    private static IConfiguration CreateConfiguration(string? key, string? issuer, string? expiryHours = null)
    {
        var inMemorySettings = new Dictionary<string, string?>();
        if (key is not null) inMemorySettings["Jwt:Key"] = key;
        if (issuer is not null) inMemorySettings["Jwt:Issuer"] = issuer;
        if (expiryHours is not null) inMemorySettings["Jwt:ExpiryHours"] = expiryHours;

        return new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
    }

    [Fact]
    public void IssueToken_Throws_If_Key_Is_Missing()
    {
        var config = CreateConfiguration(null, "test-issuer");
        var service = new JwtTokenService(config);

        Assert.Throws<InvalidOperationException>(() => service.IssueToken("user1", "customer"));
    }

    [Fact]
    public void IssueToken_Throws_If_Issuer_Is_Missing()
    {
        var config = CreateConfiguration("super-secret-key-that-is-long-enough-for-sha256", null);
        var service = new JwtTokenService(config);

        Assert.Throws<InvalidOperationException>(() => service.IssueToken("user1", "customer"));
    }

    [Fact]
    public void IssueToken_Generates_Valid_JwtToken_With_Correct_Claims_And_Expiry()
    {
        // Secret key must be at least 256 bits (32 bytes)
        var key = "super-secret-key-that-is-long-enough-for-sha256!!";
        var issuer = "test-issuer";
        var config = CreateConfiguration(key, issuer, "2");
        var service = new JwtTokenService(config);

        var (tokenString, expiresAt) = service.IssueToken("user123", "admin");

        Assert.False(string.IsNullOrWhiteSpace(tokenString));
        
        // Assert expiresAt is roughly 2 hours from now
        var diff = expiresAt - DateTime.UtcNow;
        Assert.True(diff.TotalHours > 1.9 && diff.TotalHours <= 2.1);

        // Read and parse token
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(tokenString);

        Assert.Equal(issuer, token.Issuer);
        
        // Assert claims
        var subClaim = token.Claims.Single(c => c.Type == JwtRegisteredClaimNames.Sub).Value;
        var roleClaim = token.Claims.Single(c => c.Type == ClaimTypes.Role).Value;

        Assert.Equal("user123", subClaim);
        Assert.Equal("admin", roleClaim);
    }
}
