using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Petstore.Data.Entities;

namespace Petstore.Data.Configurations;

public sealed class UserEntityConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> entity)
    {
        entity.ToTable(AccountModelConstants.Tables.Users);
        entity.HasKey(user => user.UserId);
        entity.Property(user => user.UserId).HasMaxLength(AccountModelConstants.Lengths.UserId);
        entity.Property(user => user.PasswordHash).HasMaxLength(AccountModelConstants.Lengths.PasswordHash).IsRequired();
        entity.Property(user => user.PasswordSalt).HasMaxLength(AccountModelConstants.Lengths.PasswordSalt).IsRequired();
        entity.Property(user => user.Role).HasMaxLength(AccountModelConstants.Lengths.Role).IsRequired();

        entity.HasOne(user => user.Contact)
            .WithOne(contact => contact.User)
            .HasForeignKey<CustomerContactEntity>(contact => contact.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
