using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehouse.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddProductCatalog : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "units_of_measure",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                is_system = table.Column<bool>(type: "boolean", nullable: false),
                is_active = table.Column<bool>(type: "boolean", nullable: false),
                created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                row_version = table.Column<int>(type: "integer", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_units_of_measure", x => x.id);
                table.ForeignKey(
                    name: "fk_units_of_measure_tenants_tenant_id",
                    column: x => x.tenant_id,
                    principalTable: "tenants",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "product_categories",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                parent_category_id = table.Column<Guid>(type: "uuid", nullable: true),
                is_active = table.Column<bool>(type: "boolean", nullable: false),
                created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                row_version = table.Column<int>(type: "integer", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_product_categories", x => x.id);
                table.ForeignKey(
                    name: "fk_product_categories_product_categories_parent_category_id",
                    column: x => x.parent_category_id,
                    principalTable: "product_categories",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_product_categories_tenants_tenant_id",
                    column: x => x.tenant_id,
                    principalTable: "tenants",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "products",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                sku = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                brand = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                category_id = table.Column<Guid>(type: "uuid", nullable: true),
                unit_of_measure_id = table.Column<Guid>(type: "uuid", nullable: false),
                purchase_price = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                sale_price = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                is_taxable = table.Column<bool>(type: "boolean", nullable: false),
                is_active = table.Column<bool>(type: "boolean", nullable: false),
                track_inventory = table.Column<bool>(type: "boolean", nullable: false),
                minimum_stock = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                maximum_stock = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                reorder_point = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                row_version = table.Column<int>(type: "integer", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_products", x => x.id);
                table.CheckConstraint("ck_products_maximum_stock_non_negative", "maximum_stock IS NULL OR maximum_stock >= 0");
                table.CheckConstraint("ck_products_minimum_stock_non_negative", "minimum_stock IS NULL OR minimum_stock >= 0");
                table.CheckConstraint("ck_products_purchase_price_non_negative", "purchase_price >= 0");
                table.CheckConstraint("ck_products_reorder_point_non_negative", "reorder_point IS NULL OR reorder_point >= 0");
                table.CheckConstraint("ck_products_sale_price_non_negative", "sale_price >= 0");
                table.ForeignKey(
                    name: "fk_products_product_categories_category_id",
                    column: x => x.category_id,
                    principalTable: "product_categories",
                    principalColumn: "id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "fk_products_tenants_tenant_id",
                    column: x => x.tenant_id,
                    principalTable: "tenants",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_products_units_of_measure_unit_of_measure_id",
                    column: x => x.unit_of_measure_id,
                    principalTable: "units_of_measure",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "product_barcodes",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                product_id = table.Column<Guid>(type: "uuid", nullable: false),
                barcode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                is_primary = table.Column<bool>(type: "boolean", nullable: false),
                created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_product_barcodes", x => x.id);
                table.ForeignKey(
                    name: "fk_product_barcodes_products_product_id",
                    column: x => x.product_id,
                    principalTable: "products",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_product_barcodes_tenants_tenant_id",
                    column: x => x.tenant_id,
                    principalTable: "tenants",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(name: "ix_product_barcodes_product_id", table: "product_barcodes", column: "product_id");
        migrationBuilder.CreateIndex(name: "ix_product_barcodes_tenant_id", table: "product_barcodes", column: "tenant_id");
        migrationBuilder.CreateIndex(name: "ix_product_barcodes_tenant_id_barcode", table: "product_barcodes", columns: new[] { "tenant_id", "barcode" }, unique: true);
        migrationBuilder.CreateIndex(name: "ix_product_barcodes_tenant_id_product_id", table: "product_barcodes", columns: new[] { "tenant_id", "product_id" });

        migrationBuilder.CreateIndex(name: "ix_product_categories_parent_category_id", table: "product_categories", column: "parent_category_id");
        migrationBuilder.CreateIndex(name: "ix_product_categories_tenant_id", table: "product_categories", column: "tenant_id");
        migrationBuilder.CreateIndex(name: "ix_product_categories_tenant_id_code", table: "product_categories", columns: new[] { "tenant_id", "code" }, unique: true);
        migrationBuilder.CreateIndex(name: "ix_product_categories_tenant_id_is_active", table: "product_categories", columns: new[] { "tenant_id", "is_active" });

        migrationBuilder.CreateIndex(name: "ix_products_category_id", table: "products", column: "category_id");
        migrationBuilder.CreateIndex(name: "ix_products_tenant_id", table: "products", column: "tenant_id");
        migrationBuilder.CreateIndex(name: "ix_products_tenant_id_category_id", table: "products", columns: new[] { "tenant_id", "category_id" });
        migrationBuilder.CreateIndex(name: "ix_products_tenant_id_is_active", table: "products", columns: new[] { "tenant_id", "is_active" });
        migrationBuilder.CreateIndex(name: "ix_products_tenant_id_name", table: "products", columns: new[] { "tenant_id", "name" });
        migrationBuilder.CreateIndex(name: "ix_products_tenant_id_sku", table: "products", columns: new[] { "tenant_id", "sku" }, unique: true);
        migrationBuilder.CreateIndex(name: "ix_products_unit_of_measure_id", table: "products", column: "unit_of_measure_id");

        migrationBuilder.CreateIndex(name: "ix_units_of_measure_tenant_id", table: "units_of_measure", column: "tenant_id");
        migrationBuilder.CreateIndex(name: "ix_units_of_measure_tenant_id_code", table: "units_of_measure", columns: new[] { "tenant_id", "code" }, unique: true);
        migrationBuilder.CreateIndex(name: "ix_units_of_measure_tenant_id_is_active", table: "units_of_measure", columns: new[] { "tenant_id", "is_active" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "product_barcodes");
        migrationBuilder.DropTable(name: "products");
        migrationBuilder.DropTable(name: "product_categories");
        migrationBuilder.DropTable(name: "units_of_measure");
    }
}
