using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Petstore.Data.Entities;

namespace Petstore.Data.Configurations;

public sealed class CategoryEntityConfiguration : IEntityTypeConfiguration<CategoryEntity>
{
    public void Configure(EntityTypeBuilder<CategoryEntity> entity)
    {
        entity.ToTable(CatalogModelConstants.Tables.Categories);
        entity.HasKey(category => category.Id);
        entity.Property(category => category.Id).HasMaxLength(CatalogModelConstants.Lengths.Id);
        entity.Property(category => category.Name).HasMaxLength(CatalogModelConstants.Lengths.Name).IsRequired();
        entity.Property(category => category.Description).HasMaxLength(CatalogModelConstants.Lengths.Description);

        entity.HasMany(category => category.Products)
            .WithOne(product => product.Category)
            .HasForeignKey(product => product.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
