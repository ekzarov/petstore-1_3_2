using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Petstore.Data.Entities;

namespace Petstore.Data.Configurations;

public sealed class SupplierInventoryEntityConfiguration : IEntityTypeConfiguration<SupplierInventoryEntity>
{
    public void Configure(EntityTypeBuilder<SupplierInventoryEntity> entity)
    {
        entity.ToTable("SupplierInventory");
        entity.HasKey(inventory => inventory.ItemId);
        entity.Property(inventory => inventory.ItemId).HasMaxLength(CatalogModelConstants.Lengths.Id);
    }
}
