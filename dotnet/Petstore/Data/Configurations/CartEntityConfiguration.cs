using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Petstore.Data.Entities;

namespace Petstore.Data.Configurations;

public sealed class CartEntityConfiguration : IEntityTypeConfiguration<CartEntity>
{
    public void Configure(EntityTypeBuilder<CartEntity> entity)
    {
        entity.ToTable(CartModelConstants.Tables.Carts);
        entity.HasKey(cart => cart.CartKey);
        entity.Property(cart => cart.CartKey).HasMaxLength(CartModelConstants.Lengths.CartKey);

        entity.HasMany(cart => cart.Lines)
            .WithOne(line => line.Cart)
            .HasForeignKey(line => line.CartKey)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
