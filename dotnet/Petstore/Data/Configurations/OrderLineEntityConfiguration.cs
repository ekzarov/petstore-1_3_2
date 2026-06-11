using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Petstore.Data.Entities;

namespace Petstore.Data.Configurations;

public sealed class OrderLineEntityConfiguration : IEntityTypeConfiguration<OrderLineEntity>
{
    public void Configure(EntityTypeBuilder<OrderLineEntity> entity)
    {
        entity.ToTable(OrderModelConstants.Tables.OrderLines);
        entity.HasKey(line => new { line.OrderId, line.ItemId });
        entity.Property(line => line.ItemId).HasMaxLength(CatalogModelConstants.Lengths.Id);
        entity.Property(line => line.Name).HasMaxLength(CatalogModelConstants.Lengths.Name).IsRequired();
        entity.Property(line => line.Currency).HasMaxLength(CatalogModelConstants.Lengths.Currency).IsRequired();
        entity.Property(line => line.UnitPrice)
            .HasPrecision(CatalogModelConstants.Precision.Price, CatalogModelConstants.Precision.PriceScale);
    }
}
