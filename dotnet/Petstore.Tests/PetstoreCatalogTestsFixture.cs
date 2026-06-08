using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Petstore.Data;

namespace Petstore.Tests;

public sealed class PetstoreCatalogTestsFixture
{
    private readonly DbContextOptions<PetstoreCatalogContext> options;

    public PetstoreCatalogTestsFixture()
    {
        options = new DbContextOptionsBuilder<PetstoreCatalogContext>()
            .UseSqlServer(GetConnectionString())
            .Options;
    }

    public PetstoreCatalogContext CreateContext()
    {
        return new PetstoreCatalogContext(options);
    }

    public async Task ResetDatabaseAsync()
    {
        await using var context = CreateContext();

        await context.Database.EnsureDeletedAsync();
        await context.Database.MigrateAsync();
    }

    public async Task ApplyMigrationsAsync()
    {
        await using var context = CreateContext();

        await context.Database.MigrateAsync();
    }

    private static string GetConnectionString()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        using var stream = File.OpenRead(path);
        using var document = JsonDocument.Parse(stream);

        return document.RootElement
            .GetProperty("ConnectionStrings")
            .GetProperty("PetstoreCatalogTests")
            .GetString()
            ?? throw new InvalidOperationException("Connection string 'PetstoreCatalogTests' is not configured.");
    }
}
