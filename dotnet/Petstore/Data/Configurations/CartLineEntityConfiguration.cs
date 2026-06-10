using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Petstore.Data.Entities;

namespace Petstore.Data.Configurations;

public sealed class CartLineEntityConfiguration : IEntityTypeConfiguration<CartLineEntity>
{
    public void Configure(EntityTypeBuilder<CartLineEntity> entity)
    {
        entity.ToTable(CartModelConstants.Tables.CartLines);
        entity.HasKey(line => new { line.CartKey, line.ItemId });
        entity.Property(line => line.CartKey).HasMaxLength(CartModelConstants.Lengths.CartKey);
        entity.Property(line => line.ItemId).HasMaxLength(CatalogModelConstants.Lengths.Id);
    }
}
