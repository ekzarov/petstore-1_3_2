namespace Petstore.Tests;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class PetstoreCatalogDatabaseCollection : ICollectionFixture<PetstoreCatalogTestsFixture>
{
    public const string Name = "PetstoreCatalogDatabase";
}
