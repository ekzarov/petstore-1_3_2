using Petstore.Accounts;

namespace Petstore.Tests;

public sealed class Pbkdf2PasswordHasherTests
{
    private readonly Pbkdf2PasswordHasher hasher = new();

    [Fact]
    public void Hash_Then_Verify_Succeeds_For_Correct_Password()
    {
        var (hash, salt) = hasher.Hash("j2ee");

        Assert.True(hasher.Verify("j2ee", hash, salt));
    }

    [Fact]
    public void Verify_Fails_For_Wrong_Password()
    {
        var (hash, salt) = hasher.Hash("j2ee");

        Assert.False(hasher.Verify("wrong", hash, salt));
    }

    [Fact]
    public void Hash_Produces_Distinct_Salts_And_Hashes_For_Same_Password()
    {
        var (hash1, salt1) = hasher.Hash("j2ee");
        var (hash2, salt2) = hasher.Hash("j2ee");

        Assert.NotEqual(salt1, salt2);
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void Hash_Produces_Expected_Sizes()
    {
        var (hash, salt) = hasher.Hash("j2ee");

        Assert.Equal(32, hash.Length);
        Assert.Equal(16, salt.Length);
    }
}
