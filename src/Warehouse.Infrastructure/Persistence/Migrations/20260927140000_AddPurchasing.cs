using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehouse.Infrastructure.Persistence.Migrations;

public partial class AddPurchasing : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "suppliers",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                contact_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                phone = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                notes = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                is_active = table.Column<bool>(type: "boolean", nullable: false),
                created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                row_version = table.Column<int>(type: "integer", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_suppliers", x => x.id);
                table.ForeignKey(name: "fk_suppliers_tenants_tenant_id", column: x => x.tenant_id, principalTable: "tenants", principalColumn: "id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "purchase_orders",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                supplier_id = table.Column<Guid>(type: "uuid", nullable: false),
                warehouse_id = table.Column<Guid>(type: "uuid", nullable: false),
                location_id = table.Column<Guid>(type: "uuid", nullable: false),
                status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                payment_status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                reference = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                notes = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                subtotal = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                tax_total = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                grand_total = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                paid_amount = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                confirmed_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                closed_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                row_version = table.Column<int>(type: "integer", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_purchase_orders", x => x.id);
                table.CheckConstraint("ck_purchase_orders_status", "status IN ('Draft', 'Confirmed', 'PartiallyReceived', 'FullyReceived', 'Closed', 'Cancelled')");
                table.CheckConstraint("ck_purchase_orders_payment_status", "payment_status IN ('Unpaid', 'PartiallyPaid', 'Paid')");
                table.CheckConstraint("ck_purchase_orders_paid_non_negative", "paid_amount >= 0");
                table.CheckConstraint("ck_purchase_orders_totals_non_negative", "subtotal >= 0 AND tax_total >= 0 AND grand_total >= 0");
                table.ForeignKey(name: "fk_purchase_orders_suppliers_supplier_id", column: x => x.supplier_id, principalTable: "suppliers", principalColumn: "id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey(name: "fk_purchase_orders_tenants_tenant_id", column: x => x.tenant_id, principalTable: "tenants", principalColumn: "id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey(name: "fk_purchase_orders_users_created_by_user_id", column: x => x.created_by_user_id, principalTable: "users", principalColumn: "id", onDelete: ReferentialAction.SetNull);
                table.ForeignKey(name: "fk_purchase_orders_warehouse_locations_location_id", column: x => x.location_id, principalTable: "warehouse_locations", principalColumn: "id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey(name: "fk_purchase_orders_warehouses_warehouse_id", column: x => x.warehouse_id, principalTable: "warehouses", principalColumn: "id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "purchase_order_lines",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                purchase_order_id = table.Column<Guid>(type: "uuid", nullable: false),
                product_id = table.Column<Guid>(type: "uuid", nullable: false),
                quantity_ordered = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                quantity_received = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                quantity_returned = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                unit_price = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                tax_rate = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: false),
                line_net = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                line_tax = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                line_total = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                notes = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_purchase_order_lines", x => x.id);
                table.CheckConstraint("ck_purchase_order_lines_qty_ordered_positive", "quantity_ordered > 0");
                table.CheckConstraint("ck_purchase_order_lines_qty_received_non_negative", "quantity_received >= 0");
                table.CheckConstraint("ck_purchase_order_lines_qty_returned_non_negative", "quantity_returned >= 0");
                table.CheckConstraint("ck_purchase_order_lines_received_not_over_ordered", "quantity_received <= quantity_ordered");
                table.CheckConstraint("ck_purchase_order_lines_returned_not_over_received", "quantity_returned <= quantity_received");
                table.CheckConstraint("ck_purchase_order_lines_unit_price_non_negative", "unit_price >= 0");
                table.CheckConstraint("ck_purchase_order_lines_tax_rate_non_negative", "tax_rate >= 0");
                table.ForeignKey(name: "fk_purchase_order_lines_products_product_id", column: x => x.product_id, principalTable: "products", principalColumn: "id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey(name: "fk_purchase_order_lines_purchase_orders_purchase_order_id", column: x => x.purchase_order_id, principalTable: "purchase_orders", principalColumn: "id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey(name: "fk_purchase_order_lines_tenants_tenant_id", column: x => x.tenant_id, principalTable: "tenants", principalColumn: "id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "purchase_operations",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                purchase_order_id = table.Column<Guid>(type: "uuid", nullable: false),
                action = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                result_payload_json = table.Column<string>(type: "jsonb", nullable: false),
                processed_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_purchase_operations", x => x.id);
                table.ForeignKey(name: "fk_purchase_operations_purchase_orders_purchase_order_id", column: x => x.purchase_order_id, principalTable: "purchase_orders", principalColumn: "id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey(name: "fk_purchase_operations_tenants_tenant_id", column: x => x.tenant_id, principalTable: "tenants", principalColumn: "id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "purchase_receipts",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                purchase_order_id = table.Column<Guid>(type: "uuid", nullable: false),
                warehouse_id = table.Column<Guid>(type: "uuid", nullable: false),
                location_id = table.Column<Guid>(type: "uuid", nullable: false),
                notes = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                operation_id = table.Column<Guid>(type: "uuid", nullable: true),
                received_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_purchase_receipts", x => x.id);
                table.ForeignKey(name: "fk_purchase_receipts_purchase_orders_purchase_order_id", column: x => x.purchase_order_id, principalTable: "purchase_orders", principalColumn: "id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey(name: "fk_purchase_receipts_tenants_tenant_id", column: x => x.tenant_id, principalTable: "tenants", principalColumn: "id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey(name: "fk_purchase_receipts_users_created_by_user_id", column: x => x.created_by_user_id, principalTable: "users", principalColumn: "id", onDelete: ReferentialAction.SetNull);
                table.ForeignKey(name: "fk_purchase_receipts_warehouse_locations_location_id", column: x => x.location_id, principalTable: "warehouse_locations", principalColumn: "id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey(name: "fk_purchase_receipts_warehouses_warehouse_id", column: x => x.warehouse_id, principalTable: "warehouses", principalColumn: "id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "purchase_returns",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                purchase_order_id = table.Column<Guid>(type: "uuid", nullable: false),
                warehouse_id = table.Column<Guid>(type: "uuid", nullable: false),
                location_id = table.Column<Guid>(type: "uuid", nullable: false),
                notes = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                operation_id = table.Column<Guid>(type: "uuid", nullable: true),
                returned_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_purchase_returns", x => x.id);
                table.ForeignKey(name: "fk_purchase_returns_purchase_orders_purchase_order_id", column: x => x.purchase_order_id, principalTable: "purchase_orders", principalColumn: "id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey(name: "fk_purchase_returns_tenants_tenant_id", column: x => x.tenant_id, principalTable: "tenants", principalColumn: "id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey(name: "fk_purchase_returns_users_created_by_user_id", column: x => x.created_by_user_id, principalTable: "users", principalColumn: "id", onDelete: ReferentialAction.SetNull);
                table.ForeignKey(name: "fk_purchase_returns_warehouse_locations_location_id", column: x => x.location_id, principalTable: "warehouse_locations", principalColumn: "id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey(name: "fk_purchase_returns_warehouses_warehouse_id", column: x => x.warehouse_id, principalTable: "warehouses", principalColumn: "id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "purchase_receipt_lines",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                purchase_receipt_id = table.Column<Guid>(type: "uuid", nullable: false),
                purchase_order_line_id = table.Column<Guid>(type: "uuid", nullable: false),
                product_id = table.Column<Guid>(type: "uuid", nullable: false),
                quantity = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_purchase_receipt_lines", x => x.id);
                table.CheckConstraint("ck_purchase_receipt_lines_quantity_positive", "quantity > 0");
                table.ForeignKey(name: "fk_purchase_receipt_lines_products_product_id", column: x => x.product_id, principalTable: "products", principalColumn: "id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey(name: "fk_purchase_receipt_lines_purchase_order_lines_purchase_order_line_id", column: x => x.purchase_order_line_id, principalTable: "purchase_order_lines", principalColumn: "id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey(name: "fk_purchase_receipt_lines_purchase_receipts_purchase_receipt_id", column: x => x.purchase_receipt_id, principalTable: "purchase_receipts", principalColumn: "id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey(name: "fk_purchase_receipt_lines_tenants_tenant_id", column: x => x.tenant_id, principalTable: "tenants", principalColumn: "id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "purchase_return_lines",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                purchase_return_id = table.Column<Guid>(type: "uuid", nullable: false),
                purchase_order_line_id = table.Column<Guid>(type: "uuid", nullable: false),
                product_id = table.Column<Guid>(type: "uuid", nullable: false),
                quantity = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_purchase_return_lines", x => x.id);
                table.CheckConstraint("ck_purchase_return_lines_quantity_positive", "quantity > 0");
                table.ForeignKey(name: "fk_purchase_return_lines_products_product_id", column: x => x.product_id, principalTable: "products", principalColumn: "id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey(name: "fk_purchase_return_lines_purchase_order_lines_purchase_order_line_id", column: x => x.purchase_order_line_id, principalTable: "purchase_order_lines", principalColumn: "id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey(name: "fk_purchase_return_lines_purchase_returns_purchase_return_id", column: x => x.purchase_return_id, principalTable: "purchase_returns", principalColumn: "id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey(name: "fk_purchase_return_lines_tenants_tenant_id", column: x => x.tenant_id, principalTable: "tenants", principalColumn: "id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(name: "ix_purchase_operations_purchase_order_id", table: "purchase_operations", column: "purchase_order_id");
        migrationBuilder.CreateIndex(name: "ix_purchase_operations_tenant_id", table: "purchase_operations", column: "tenant_id");
        migrationBuilder.CreateIndex(name: "ix_purchase_operations_tenant_id_purchase_order_id", table: "purchase_operations", columns: new[] { "tenant_id", "purchase_order_id" });

        migrationBuilder.CreateIndex(name: "ix_purchase_order_lines_product_id", table: "purchase_order_lines", column: "product_id");
        migrationBuilder.CreateIndex(name: "ix_purchase_order_lines_purchase_order_id_product_id", table: "purchase_order_lines", columns: new[] { "purchase_order_id", "product_id" }, unique: true);
        migrationBuilder.CreateIndex(name: "ix_purchase_order_lines_tenant_id", table: "purchase_order_lines", column: "tenant_id");

        migrationBuilder.CreateIndex(name: "ix_purchase_orders_created_by_user_id", table: "purchase_orders", column: "created_by_user_id");
        migrationBuilder.CreateIndex(name: "ix_purchase_orders_location_id", table: "purchase_orders", column: "location_id");
        migrationBuilder.CreateIndex(name: "ix_purchase_orders_supplier_id", table: "purchase_orders", column: "supplier_id");
        migrationBuilder.CreateIndex(name: "ix_purchase_orders_tenant_id", table: "purchase_orders", column: "tenant_id");
        migrationBuilder.CreateIndex(name: "ix_purchase_orders_tenant_id_status", table: "purchase_orders", columns: new[] { "tenant_id", "status" });
        migrationBuilder.CreateIndex(name: "ix_purchase_orders_tenant_id_supplier_id", table: "purchase_orders", columns: new[] { "tenant_id", "supplier_id" });
        migrationBuilder.CreateIndex(name: "ix_purchase_orders_warehouse_id", table: "purchase_orders", column: "warehouse_id");

        migrationBuilder.CreateIndex(name: "ix_purchase_receipt_lines_product_id", table: "purchase_receipt_lines", column: "product_id");
        migrationBuilder.CreateIndex(name: "ix_purchase_receipt_lines_purchase_order_line_id", table: "purchase_receipt_lines", column: "purchase_order_line_id");
        migrationBuilder.CreateIndex(name: "ix_purchase_receipt_lines_purchase_receipt_id_purchase_order_line_id", table: "purchase_receipt_lines", columns: new[] { "purchase_receipt_id", "purchase_order_line_id" }, unique: true);
        migrationBuilder.CreateIndex(name: "ix_purchase_receipt_lines_tenant_id", table: "purchase_receipt_lines", column: "tenant_id");

        migrationBuilder.CreateIndex(name: "ix_purchase_receipts_created_by_user_id", table: "purchase_receipts", column: "created_by_user_id");
        migrationBuilder.CreateIndex(name: "ix_purchase_receipts_location_id", table: "purchase_receipts", column: "location_id");
        migrationBuilder.CreateIndex(name: "ix_purchase_receipts_operation_id", table: "purchase_receipts", column: "operation_id", unique: true);
        migrationBuilder.CreateIndex(name: "ix_purchase_receipts_purchase_order_id", table: "purchase_receipts", column: "purchase_order_id");
        migrationBuilder.CreateIndex(name: "ix_purchase_receipts_tenant_id", table: "purchase_receipts", column: "tenant_id");
        migrationBuilder.CreateIndex(name: "ix_purchase_receipts_tenant_id_purchase_order_id", table: "purchase_receipts", columns: new[] { "tenant_id", "purchase_order_id" });
        migrationBuilder.CreateIndex(name: "ix_purchase_receipts_warehouse_id", table: "purchase_receipts", column: "warehouse_id");

        migrationBuilder.CreateIndex(name: "ix_purchase_return_lines_product_id", table: "purchase_return_lines", column: "product_id");
        migrationBuilder.CreateIndex(name: "ix_purchase_return_lines_purchase_order_line_id", table: "purchase_return_lines", column: "purchase_order_line_id");
        migrationBuilder.CreateIndex(name: "ix_purchase_return_lines_purchase_return_id_purchase_order_line_id", table: "purchase_return_lines", columns: new[] { "purchase_return_id", "purchase_order_line_id" }, unique: true);
        migrationBuilder.CreateIndex(name: "ix_purchase_return_lines_tenant_id", table: "purchase_return_lines", column: "tenant_id");

        migrationBuilder.CreateIndex(name: "ix_purchase_returns_created_by_user_id", table: "purchase_returns", column: "created_by_user_id");
        migrationBuilder.CreateIndex(name: "ix_purchase_returns_location_id", table: "purchase_returns", column: "location_id");
        migrationBuilder.CreateIndex(name: "ix_purchase_returns_operation_id", table: "purchase_returns", column: "operation_id", unique: true);
        migrationBuilder.CreateIndex(name: "ix_purchase_returns_purchase_order_id", table: "purchase_returns", column: "purchase_order_id");
        migrationBuilder.CreateIndex(name: "ix_purchase_returns_tenant_id", table: "purchase_returns", column: "tenant_id");
        migrationBuilder.CreateIndex(name: "ix_purchase_returns_tenant_id_purchase_order_id", table: "purchase_returns", columns: new[] { "tenant_id", "purchase_order_id" });
        migrationBuilder.CreateIndex(name: "ix_purchase_returns_warehouse_id", table: "purchase_returns", column: "warehouse_id");

        migrationBuilder.CreateIndex(name: "ix_suppliers_tenant_id", table: "suppliers", column: "tenant_id");
        migrationBuilder.CreateIndex(name: "ix_suppliers_tenant_id_code", table: "suppliers", columns: new[] { "tenant_id", "code" }, unique: true);
        migrationBuilder.CreateIndex(name: "ix_suppliers_tenant_id_is_active", table: "suppliers", columns: new[] { "tenant_id", "is_active" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "purchase_operations");
        migrationBuilder.DropTable(name: "purchase_receipt_lines");
        migrationBuilder.DropTable(name: "purchase_return_lines");
        migrationBuilder.DropTable(name: "purchase_receipts");
        migrationBuilder.DropTable(name: "purchase_returns");
        migrationBuilder.DropTable(name: "purchase_order_lines");
        migrationBuilder.DropTable(name: "purchase_orders");
        migrationBuilder.DropTable(name: "suppliers");
    }
}
