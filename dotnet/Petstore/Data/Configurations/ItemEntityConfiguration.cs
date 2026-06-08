using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Petstore.Data.Entities;

namespace Petstore.Data.Configurations;

public sealed class ItemEntityConfiguration : IEntityTypeConfiguration<ItemEntity>
{
    public void Configure(EntityTypeBuilder<ItemEntity> entity)
    {
        var attributesComparer = new ValueComparer<List<string>>(
            (left, right) => left != null && right != null && left.SequenceEqual(right),
            value => value.Aggregate(0, (hash, item) => HashCode.Combine(hash, item.GetHashCode())),
            value => value.ToList());

        entity.ToTable(CatalogModelConstants.Tables.Items);
        entity.HasKey(item => item.Id);
        entity.Property(item => item.Id).HasMaxLength(CatalogModelConstants.Lengths.Id);
        entity.Property(item => item.ProductId).HasMaxLength(CatalogModelConstants.Lengths.Id).IsRequired();
        entity.Property(item => item.Name).HasMaxLength(CatalogModelConstants.Lengths.Name).IsRequired();
        entity.Property(item => item.Description).HasMaxLength(CatalogModelConstants.Lengths.Description);
        entity.Property(item => item.Price)
            .HasPrecision(CatalogModelConstants.Precision.Price, CatalogModelConstants.Precision.PriceScale);
        entity.Property(item => item.Currency).HasMaxLength(CatalogModelConstants.Lengths.Currency).IsRequired();
        entity.Property(item => item.Attributes)
            .HasConversion(
                value => JsonSerializer.Serialize(value, (JsonSerializerOptions?)null),
                value => JsonSerializer.Deserialize<List<string>>(value, (JsonSerializerOptions?)null) ?? new List<string>())
            .Metadata.SetValueComparer(attributesComparer);
    }
}
