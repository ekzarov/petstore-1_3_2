namespace Petstore.Tests;

public abstract class DatabaseIntegrationTestBase(PetstoreCatalogTestsFixture fixture) : IAsyncLifetime
{
    protected PetstoreCatalogTestsFixture Fixture { get; } = fixture;

    public async Task InitializeAsync()
    {
        await Fixture.ResetDatabaseAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}
