using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Petstore.Data.Entities;

namespace Petstore.Data.Configurations;

public sealed class ShipmentLineEntityConfiguration : IEntityTypeConfiguration<ShipmentLineEntity>
{
    public void Configure(EntityTypeBuilder<ShipmentLineEntity> entity)
    {
        entity.ToTable("ShipmentLines");
        entity.HasKey(line => new { line.ShipmentId, line.ItemId });
        entity.Property(line => line.ItemId).HasMaxLength(CatalogModelConstants.Lengths.Id);
    }
}
