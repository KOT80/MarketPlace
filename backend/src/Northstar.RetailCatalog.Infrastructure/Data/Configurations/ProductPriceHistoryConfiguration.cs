using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Northstar.RetailCatalog.Domain.Entities;

namespace Northstar.RetailCatalog.Infrastructure.Data.Configurations;

public class ProductPriceHistoryConfiguration : IEntityTypeConfiguration<ProductPriceHistory>
{
    public void Configure(EntityTypeBuilder<ProductPriceHistory> builder)
    {
        builder.ToTable("product_price_history", t => t.HasCheckConstraint(
            "ck_product_price_history_prices_nonneg",
            "old_price >= 0 AND new_price >= 0"));

        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id).HasColumnName("id");

        builder.Property(h => h.ProductId)
            .HasColumnName("product_id")
            .IsRequired();

        builder.Property(h => h.OldPrice)
            .HasColumnName("old_price")
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(h => h.NewPrice)
            .HasColumnName("new_price")
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(h => h.ChangedAtUtc)
            .HasColumnName("changed_at_utc")
            .IsRequired();

        builder.HasIndex(h => new { h.ProductId, h.ChangedAtUtc })
            .HasDatabaseName("ix_product_price_history_product_id_changed_at_utc");

        builder.HasOne(h => h.Product)
            .WithMany(p => p.PriceHistory)
            .HasForeignKey(h => h.ProductId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_product_price_history_product_id");
    }
}
