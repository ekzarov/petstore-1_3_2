using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Petstore.Data.Entities;

namespace Petstore.Data.Configurations;

public sealed class ShipmentEntityConfiguration : IEntityTypeConfiguration<ShipmentEntity>
{
    public void Configure(EntityTypeBuilder<ShipmentEntity> entity)
    {
        entity.ToTable("Shipments");
        entity.HasKey(shipment => shipment.Id);
        entity.HasIndex(shipment => shipment.OrderId);

        entity.HasMany(shipment => shipment.Lines)
            .WithOne(line => line.Shipment)
            .HasForeignKey(line => line.ShipmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
