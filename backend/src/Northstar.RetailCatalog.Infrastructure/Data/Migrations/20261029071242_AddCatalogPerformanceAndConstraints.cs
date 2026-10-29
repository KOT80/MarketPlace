using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Northstar.RetailCatalog.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalogPerformanceAndConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,");

            migrationBuilder.CreateIndex(
                name: "ix_products_created_at_utc_id",
                table: "products",
                columns: new[] { "created_at_utc", "id" });

            migrationBuilder.CreateIndex(
                name: "ix_products_name_id",
                table: "products",
                columns: new[] { "name", "id" });

            migrationBuilder.CreateIndex(
                name: "ix_products_name_trgm",
                table: "products",
                column: "name")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_products_price_id",
                table: "products",
                columns: new[] { "price", "id" });

            migrationBuilder.Sql(
                "CREATE INDEX ix_products_sku_trgm ON products USING gin (sku gin_trgm_ops);");

            migrationBuilder.AddCheckConstraint(
                name: "ck_products_price_range",
                table: "products",
                sql: "price >= 0 AND price <= 999999999999.99");

            migrationBuilder.AddCheckConstraint(
                name: "ck_product_price_history_prices_nonneg",
                table: "product_price_history",
                sql: "old_price >= 0 AND new_price >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_products_created_at_utc_id",
                table: "products");

            migrationBuilder.DropIndex(
                name: "ix_products_name_id",
                table: "products");

            migrationBuilder.DropIndex(
                name: "ix_products_name_trgm",
                table: "products");

            migrationBuilder.DropIndex(
                name: "ix_products_price_id",
                table: "products");

            migrationBuilder.Sql("DROP INDEX IF EXISTS ix_products_sku_trgm;");

            migrationBuilder.DropCheckConstraint(
                name: "ck_products_price_range",
                table: "products");

            migrationBuilder.DropCheckConstraint(
                name: "ck_product_price_history_prices_nonneg",
                table: "product_price_history");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:pg_trgm", ",,");
        }
    }
}
