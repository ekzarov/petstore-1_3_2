using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Petstore.Tests;

internal sealed class CatalogApiFactory(string connectionString) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:PetstoreCatalog"] = connectionString
            });
        });
    }
}
