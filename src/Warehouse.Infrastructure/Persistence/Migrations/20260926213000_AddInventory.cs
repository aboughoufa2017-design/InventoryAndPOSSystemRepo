using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehouse.Infrastructure.Persistence.Migrations;

public partial class AddInventory : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "stock_balances",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                product_id = table.Column<Guid>(type: "uuid", nullable: false),
                warehouse_id = table.Column<Guid>(type: "uuid", nullable: false),
                location_id = table.Column<Guid>(type: "uuid", nullable: false),
                on_hand = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                reserved = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                row_version = table.Column<int>(type: "integer", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_stock_balances", x => x.id);
                table.ForeignKey(name: "fk_stock_balances_products_product_id", column: x => x.product_id, principalTable: "products", principalColumn: "id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey(name: "fk_stock_balances_tenants_tenant_id", column: x => x.tenant_id, principalTable: "tenants", principalColumn: "id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey(name: "fk_stock_balances_warehouse_locations_location_id", column: x => x.location_id, principalTable: "warehouse_locations", principalColumn: "id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey(name: "fk_stock_balances_warehouses_warehouse_id", column: x => x.warehouse_id, principalTable: "warehouses", principalColumn: "id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "stock_adjustments",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                warehouse_id = table.Column<Guid>(type: "uuid", nullable: false),
                kind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                reason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                posted_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                row_version = table.Column<int>(type: "integer", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_stock_adjustments", x => x.id);
                table.CheckConstraint("ck_stock_adjustments_kind", "kind IN ('OpeningBalance', 'Correction')");
                table.CheckConstraint("ck_stock_adjustments_status", "status IN ('Draft', 'Approved', 'Posted', 'Cancelled')");
                table.ForeignKey(name: "fk_stock_adjustments_tenants_tenant_id", column: x => x.tenant_id, principalTable: "tenants", principalColumn: "id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey(name: "fk_stock_adjustments_users_created_by_user_id", column: x => x.created_by_user_id, principalTable: "users", principalColumn: "id", onDelete: ReferentialAction.SetNull);
                table.ForeignKey(name: "fk_stock_adjustments_warehouses_warehouse_id", column: x => x.warehouse_id, principalTable: "warehouses", principalColumn: "id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "stock_movements",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                product_id = table.Column<Guid>(type: "uuid", nullable: false),
                warehouse_id = table.Column<Guid>(type: "uuid", nullable: false),
                location_id = table.Column<Guid>(type: "uuid", nullable: false),
                quantity_change = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                movement_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                source_document_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                source_document_id = table.Column<Guid>(type: "uuid", nullable: true),
                transfer_line_id = table.Column<Guid>(type: "uuid", nullable: true),
                created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                notes = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                occurred_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_stock_movements", x => x.id);
                table.ForeignKey(name: "fk_stock_movements_products_product_id", column: x => x.product_id, principalTable: "products", principalColumn: "id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey(name: "fk_stock_movements_tenants_tenant_id", column: x => x.tenant_id, principalTable: "tenants", principalColumn: "id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey(name: "fk_stock_movements_users_created_by_user_id", column: x => x.created_by_user_id, principalTable: "users", principalColumn: "id", onDelete: ReferentialAction.SetNull);
                table.ForeignKey(name: "fk_stock_movements_warehouse_locations_location_id", column: x => x.location_id, principalTable: "warehouse_locations", principalColumn: "id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey(name: "fk_stock_movements_warehouses_warehouse_id", column: x => x.warehouse_id, principalTable: "warehouses", principalColumn: "id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "stock_adjustment_lines",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                stock_adjustment_id = table.Column<Guid>(type: "uuid", nullable: false),
                product_id = table.Column<Guid>(type: "uuid", nullable: false),
                location_id = table.Column<Guid>(type: "uuid", nullable: false),
                quantity_change = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                notes = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_stock_adjustment_lines", x => x.id);
                table.CheckConstraint("ck_stock_adjustment_lines_quantity_change_nonzero", "quantity_change <> 0");
                table.ForeignKey(name: "fk_stock_adjustment_lines_products_product_id", column: x => x.product_id, principalTable: "products", principalColumn: "id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey(name: "fk_stock_adjustment_lines_stock_adjustments_stock_adjustment_id", column: x => x.stock_adjustment_id, principalTable: "stock_adjustments", principalColumn: "id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey(name: "fk_stock_adjustment_lines_tenants_tenant_id", column: x => x.tenant_id, principalTable: "tenants", principalColumn: "id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey(name: "fk_stock_adjustment_lines_warehouse_locations_location_id", column: x => x.location_id, principalTable: "warehouse_locations", principalColumn: "id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(name: "ix_stock_adjustment_lines_product_id", table: "stock_adjustment_lines", column: "product_id");
        migrationBuilder.CreateIndex(name: "ix_stock_adjustment_lines_location_id", table: "stock_adjustment_lines", column: "location_id");
        migrationBuilder.CreateIndex(name: "ix_stock_adjustment_lines_stock_adjustment_id_product_id_locatio", table: "stock_adjustment_lines", columns: new[] { "stock_adjustment_id", "product_id", "location_id" }, unique: true);
        migrationBuilder.CreateIndex(name: "ix_stock_adjustment_lines_tenant_id", table: "stock_adjustment_lines", column: "tenant_id");

        migrationBuilder.CreateIndex(name: "ix_stock_adjustments_created_by_user_id", table: "stock_adjustments", column: "created_by_user_id");
        migrationBuilder.CreateIndex(name: "ix_stock_adjustments_tenant_id", table: "stock_adjustments", column: "tenant_id");
        migrationBuilder.CreateIndex(name: "ix_stock_adjustments_tenant_id_warehouse_id_status", table: "stock_adjustments", columns: new[] { "tenant_id", "warehouse_id", "status" });
        migrationBuilder.CreateIndex(name: "ix_stock_adjustments_warehouse_id", table: "stock_adjustments", column: "warehouse_id");

        migrationBuilder.CreateIndex(name: "ix_stock_balances_location_id", table: "stock_balances", column: "location_id");
        migrationBuilder.CreateIndex(name: "ix_stock_balances_product_id", table: "stock_balances", column: "product_id");
        migrationBuilder.CreateIndex(name: "ix_stock_balances_tenant_id", table: "stock_balances", column: "tenant_id");
        migrationBuilder.CreateIndex(name: "ix_stock_balances_tenant_id_product_id_warehouse_id_location_id", table: "stock_balances", columns: new[] { "tenant_id", "product_id", "warehouse_id", "location_id" }, unique: true);
        migrationBuilder.CreateIndex(name: "ix_stock_balances_tenant_id_warehouse_id_product_id", table: "stock_balances", columns: new[] { "tenant_id", "warehouse_id", "product_id" });
        migrationBuilder.CreateIndex(name: "ix_stock_balances_warehouse_id", table: "stock_balances", column: "warehouse_id");

        migrationBuilder.CreateIndex(name: "ix_stock_movements_created_by_user_id", table: "stock_movements", column: "created_by_user_id");
        migrationBuilder.CreateIndex(name: "ix_stock_movements_location_id", table: "stock_movements", column: "location_id");
        migrationBuilder.CreateIndex(name: "ix_stock_movements_product_id", table: "stock_movements", column: "product_id");
        migrationBuilder.CreateIndex(name: "ix_stock_movements_tenant_id", table: "stock_movements", column: "tenant_id");
        migrationBuilder.CreateIndex(name: "ix_stock_movements_tenant_id_product_id_occurred_at_utc", table: "stock_movements", columns: new[] { "tenant_id", "product_id", "occurred_at_utc" });
        migrationBuilder.CreateIndex(name: "ix_stock_movements_tenant_id_source_document_type_source_documen", table: "stock_movements", columns: new[] { "tenant_id", "source_document_type", "source_document_id" });
        migrationBuilder.CreateIndex(name: "ix_stock_movements_tenant_id_warehouse_id_location_id", table: "stock_movements", columns: new[] { "tenant_id", "warehouse_id", "location_id" });
        migrationBuilder.CreateIndex(name: "ix_stock_movements_transfer_line_id", table: "stock_movements", column: "transfer_line_id");
        migrationBuilder.CreateIndex(name: "ix_stock_movements_warehouse_id", table: "stock_movements", column: "warehouse_id");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "stock_adjustment_lines");
        migrationBuilder.DropTable(name: "stock_movements");
        migrationBuilder.DropTable(name: "stock_balances");
        migrationBuilder.DropTable(name: "stock_adjustments");
    }
}
