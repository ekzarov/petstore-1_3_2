using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Petstore.Data.Entities;

namespace Petstore.Data.Configurations;

public sealed class CustomerContactEntityConfiguration : IEntityTypeConfiguration<CustomerContactEntity>
{
    public void Configure(EntityTypeBuilder<CustomerContactEntity> entity)
    {
        entity.ToTable(AccountModelConstants.Tables.CustomerContacts);
        entity.HasKey(contact => contact.UserId);
        entity.Property(contact => contact.UserId).HasMaxLength(AccountModelConstants.Lengths.UserId);
        entity.Property(contact => contact.FamilyName).HasMaxLength(AccountModelConstants.Lengths.Name).IsRequired();
        entity.Property(contact => contact.GivenName).HasMaxLength(AccountModelConstants.Lengths.Name).IsRequired();
        entity.Property(contact => contact.Street1).HasMaxLength(AccountModelConstants.Lengths.Street).IsRequired();
        entity.Property(contact => contact.Street2).HasMaxLength(AccountModelConstants.Lengths.Street);
        entity.Property(contact => contact.City).HasMaxLength(AccountModelConstants.Lengths.City).IsRequired();
        entity.Property(contact => contact.State).HasMaxLength(AccountModelConstants.Lengths.State).IsRequired();
        entity.Property(contact => contact.Zip).HasMaxLength(AccountModelConstants.Lengths.Zip).IsRequired();
        entity.Property(contact => contact.Country).HasMaxLength(AccountModelConstants.Lengths.Country).IsRequired();
        entity.Property(contact => contact.Email).HasMaxLength(AccountModelConstants.Lengths.Email).IsRequired();
        entity.Property(contact => contact.Phone).HasMaxLength(AccountModelConstants.Lengths.Phone).IsRequired();
    }
}
