using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Petstore.Data.Entities;

namespace Petstore.Data.Configurations;

public sealed class OrderEntityConfiguration : IEntityTypeConfiguration<OrderEntity>
{
    public void Configure(EntityTypeBuilder<OrderEntity> entity)
    {
        entity.ToTable(OrderModelConstants.Tables.Orders);
        entity.HasKey(order => order.Id);
        entity.Property(order => order.Id).ValueGeneratedOnAdd();
        entity.Property(order => order.UserId).HasMaxLength(AccountModelConstants.Lengths.UserId).IsRequired();
        entity.Property(order => order.Status).HasMaxLength(OrderModelConstants.Lengths.Status).IsRequired();
        entity.Property(order => order.Currency).HasMaxLength(CatalogModelConstants.Lengths.Currency).IsRequired();
        entity.Property(order => order.Total)
            .HasPrecision(CatalogModelConstants.Precision.Price, CatalogModelConstants.Precision.PriceScale);
        entity.HasIndex(order => order.UserId);
        entity.HasIndex(order => order.Status);

        entity.OwnsOne(order => order.ShippingContact, ConfigureContact);
        entity.OwnsOne(order => order.BillingContact, ConfigureContact);

        entity.HasMany(order => order.Lines)
            .WithOne(line => line.Order)
            .HasForeignKey(line => line.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureContact(OwnedNavigationBuilder<OrderEntity, OrderContactBlock> contact)
    {
        contact.Property(c => c.FamilyName).HasMaxLength(AccountModelConstants.Lengths.Name).IsRequired();
        contact.Property(c => c.GivenName).HasMaxLength(AccountModelConstants.Lengths.Name).IsRequired();
        contact.Property(c => c.Street1).HasMaxLength(AccountModelConstants.Lengths.Street).IsRequired();
        contact.Property(c => c.Street2).HasMaxLength(AccountModelConstants.Lengths.Street);
        contact.Property(c => c.City).HasMaxLength(AccountModelConstants.Lengths.City).IsRequired();
        contact.Property(c => c.State).HasMaxLength(AccountModelConstants.Lengths.State).IsRequired();
        contact.Property(c => c.Zip).HasMaxLength(AccountModelConstants.Lengths.Zip).IsRequired();
        contact.Property(c => c.Country).HasMaxLength(AccountModelConstants.Lengths.Country).IsRequired();
        contact.Property(c => c.Email).HasMaxLength(AccountModelConstants.Lengths.Email).IsRequired();
        contact.Property(c => c.Phone).HasMaxLength(AccountModelConstants.Lengths.Phone).IsRequired();
    }
}
