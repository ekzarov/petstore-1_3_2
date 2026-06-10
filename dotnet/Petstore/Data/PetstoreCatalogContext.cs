using Microsoft.EntityFrameworkCore;
using Petstore.Catalog;
using Petstore.Data.Entities;

namespace Petstore.Data;

public sealed class PetstoreCatalogContext(DbContextOptions<PetstoreCatalogContext> options) : DbContext(options)
{
    public DbSet<CategoryEntity> Categories => Set<CategoryEntity>();

    public DbSet<ProductEntity> Products => Set<ProductEntity>();

    public DbSet<ItemEntity> Items => Set<ItemEntity>();

    public DbSet<UserEntity> Users => Set<UserEntity>();

    public DbSet<CustomerContactEntity> CustomerContacts => Set<CustomerContactEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PetstoreCatalogContext).Assembly);
        CatalogSeeder.Seed(modelBuilder);
    }
}
