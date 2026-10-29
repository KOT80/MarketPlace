using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Northstar.RetailCatalog.Domain.Entities;

namespace Northstar.RetailCatalog.Infrastructure.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products", t => t.HasCheckConstraint(
            "ck_products_price_range",
            "price >= 0 AND price <= 999999999999.99"));

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");

        builder.Property(p => p.Sku)
            .HasColumnName("sku")
            .HasMaxLength(64)
            .IsRequired();

        builder.HasIndex(p => p.Sku)
            .IsUnique()
            .HasDatabaseName("ux_products_sku");

        builder.Property(p => p.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasColumnName("description")
            .HasMaxLength(4000);

        builder.Property(p => p.CategoryId)
            .HasColumnName("category_id")
            .IsRequired();

        builder.Property(p => p.Price)
            .HasColumnName("price")
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(p => p.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(p => p.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(p => p.UpdatedAtUtc)
            .HasColumnName("updated_at_utc");

        builder.HasIndex(p => new { p.CategoryId, p.IsActive })
            .HasDatabaseName("ix_products_category_id_is_active");

        builder.HasIndex(p => new { p.CreatedAtUtc, p.Id })
            .HasDatabaseName("ix_products_created_at_utc_id");

        builder.HasIndex(p => new { p.Price, p.Id })
            .HasDatabaseName("ix_products_price_id");

        builder.HasIndex(p => new { p.Name, p.Id })
            .HasDatabaseName("ix_products_name_id");

        builder.HasIndex(p => p.Name)
            .HasDatabaseName("ix_products_name_trgm")
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");

        builder.HasOne(p => p.Category)
            .WithMany()
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_products_category_id");

        builder.Property<uint>("Version").IsRowVersion();
    }
}
