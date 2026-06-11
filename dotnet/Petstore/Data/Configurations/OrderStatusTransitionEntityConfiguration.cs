using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Petstore.Data.Entities;

namespace Petstore.Data.Configurations;

public sealed class OrderStatusTransitionEntityConfiguration : IEntityTypeConfiguration<OrderStatusTransitionEntity>
{
    public void Configure(EntityTypeBuilder<OrderStatusTransitionEntity> entity)
    {
        entity.ToTable("OrderStatusTransitions");
        entity.HasKey(transition => transition.Id);
        entity.Property(transition => transition.FromStatus).HasMaxLength(OrderModelConstants.Lengths.Status).IsRequired();
        entity.Property(transition => transition.ToStatus).HasMaxLength(OrderModelConstants.Lengths.Status).IsRequired();
        entity.Property(transition => transition.Actor).HasMaxLength(AccountModelConstants.Lengths.UserId).IsRequired();
        entity.HasIndex(transition => transition.OrderId);
    }
}
