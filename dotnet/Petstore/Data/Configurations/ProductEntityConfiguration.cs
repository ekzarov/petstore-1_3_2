using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Petstore.Data.Entities;

namespace Petstore.Data.Configurations;

public sealed class ProductEntityConfiguration : IEntityTypeConfiguration<ProductEntity>
{
    public void Configure(EntityTypeBuilder<ProductEntity> entity)
    {
        entity.ToTable(CatalogModelConstants.Tables.Products);
        entity.HasKey(product => product.Id);
        entity.Property(product => product.Id).HasMaxLength(CatalogModelConstants.Lengths.Id);
        entity.Property(product => product.CategoryId).HasMaxLength(CatalogModelConstants.Lengths.Id).IsRequired();
        entity.Property(product => product.Name).HasMaxLength(CatalogModelConstants.Lengths.Name).IsRequired();
        entity.Property(product => product.Description).HasMaxLength(CatalogModelConstants.Lengths.Description);

        entity.HasMany(product => product.Items)
            .WithOne(item => item.Product)
            .HasForeignKey(item => item.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
