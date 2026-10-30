using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehouse.Infrastructure.Persistence.Migrations;

public partial class AddSales : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "customers",
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
                table.PrimaryKey("pk_customers", x => x.id);
                table.ForeignKey(name: "fk_customers_tenants_tenant_id", column: x => x.tenant_id, principalTable: "tenants", principalColumn: "id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "sales_orders",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                customer_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                table.PrimaryKey("pk_sales_orders", x => x.id);
                table.CheckConstraint("ck_sales_orders_status", "status IN ('Draft', 'Confirmed', 'PartiallyFulfilled', 'Fulfilled', 'Closed', 'Cancelled')");
                table.CheckConstraint("ck_sales_orders_payment_status", "payment_status IN ('Unpaid', 'PartiallyPaid', 'Paid')");
                table.CheckConstraint("ck_sales_orders_paid_non_negative", "paid_amount >= 0");
                table.CheckConstraint("ck_sales_orders_totals_non_negative", "subtotal >= 0 AND tax_total >= 0 AND grand_total >= 0");
                table.ForeignKey(name: "fk_sales_orders_customers_customer_id", column: x => x.customer_id, principalTable: "customers", principalColumn: "id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey(name: "fk_sales_orders_tenants_tenant_id", column: x => x.tenant_id, principalTable: "tenants", principalColumn: "id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey(name: "fk_sales_orders_users_created_by_user_id", column: x => x.created_by_user_id, principalTable: "users", principalColumn: "id", onDelete: ReferentialAction.SetNull);
                table.ForeignKey(name: "fk_sales_orders_warehouse_locations_location_id", column: x => x.location_id, principalTable: "warehouse_locations", principalColumn: "id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey(name: "fk_sales_orders_warehouses_warehouse_id", column: x => x.warehouse_id, principalTable: "warehouses", principalColumn: "id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "sales_order_lines",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                sales_order_id = table.Column<Guid>(type: "uuid", nullable: false),
                product_id = table.Column<Guid>(type: "uuid", nullable: false),
                quantity_ordered = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                quantity_shipped = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
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
                table.PrimaryKey("pk_sales_order_lines", x => x.id);
                table.CheckConstraint("ck_sales_order_lines_qty_ordered_positive", "quantity_ordered > 0");
                table.CheckConstraint("ck_sales_order_lines_qty_shipped_non_negative", "quantity_shipped >= 0");
                table.CheckConstraint("ck_sales_order_lines_qty_returned_non_negative", "quantity_returned >= 0");
                table.CheckConstraint("ck_sales_order_lines_shipped_not_over_ordered", "quantity_shipped <= quantity_ordered");
                table.CheckConstraint("ck_sales_order_lines_returned_not_over_shipped", "quantity_returned <= quantity_shipped");
                table.CheckConstraint("ck_sales_order_lines_unit_price_non_negative", "unit_price >= 0");
                table.CheckConstraint("ck_sales_order_lines_tax_rate_non_negative", "tax_rate >= 0");
                table.ForeignKey(name: "fk_sales_order_lines_products_product_id", column: x => x.product_id, principalTable: "products", principalColumn: "id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey(name: "fk_sales_order_lines_sales_orders_sales_order_id", column: x => x.sales_order_id, principalTable: "sales_orders", principalColumn: "id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey(name: "fk_sales_order_lines_tenants_tenant_id", column: x => x.tenant_id, principalTable: "tenants", principalColumn: "id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "sales_operations",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                sales_order_id = table.Column<Guid>(type: "uuid", nullable: false),
                action = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                result_payload_json = table.Column<string>(type: "jsonb", nullable: false),
                processed_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_sales_operations", x => x.id);
                table.ForeignKey(name: "fk_sales_operations_sales_orders_sales_order_id", column: x => x.sales_order_id, principalTable: "sales_orders", principalColumn: "id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey(name: "fk_sales_operations_tenants_tenant_id", column: x => x.tenant_id, principalTable: "tenants", principalColumn: "id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "sales_shipments",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                sales_order_id = table.Column<Guid>(type: "uuid", nullable: false),
                warehouse_id = table.Column<Guid>(type: "uuid", nullable: false),
                location_id = table.Column<Guid>(type: "uuid", nullable: false),
                notes = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                operation_id = table.Column<Guid>(type: "uuid", nullable: true),
                shipped_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_sales_shipments", x => x.id);
                table.ForeignKey(name: "fk_sales_shipments_sales_orders_sales_order_id", column: x => x.sales_order_id, principalTable: "sales_orders", principalColumn: "id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey(name: "fk_sales_shipments_tenants_tenant_id", column: x => x.tenant_id, principalTable: "tenants", principalColumn: "id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey(name: "fk_sales_shipments_users_created_by_user_id", column: x => x.created_by_user_id, principalTable: "users", principalColumn: "id", onDelete: ReferentialAction.SetNull);
                table.ForeignKey(name: "fk_sales_shipments_warehouse_locations_location_id", column: x => x.location_id, principalTable: "warehouse_locations", principalColumn: "id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey(name: "fk_sales_shipments_warehouses_warehouse_id", column: x => x.warehouse_id, principalTable: "warehouses", principalColumn: "id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "sales_returns",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                sales_order_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                table.PrimaryKey("pk_sales_returns", x => x.id);
                table.ForeignKey(name: "fk_sales_returns_sales_orders_sales_order_id", column: x => x.sales_order_id, principalTable: "sales_orders", principalColumn: "id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey(name: "fk_sales_returns_tenants_tenant_id", column: x => x.tenant_id, principalTable: "tenants", principalColumn: "id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey(name: "fk_sales_returns_users_created_by_user_id", column: x => x.created_by_user_id, principalTable: "users", principalColumn: "id", onDelete: ReferentialAction.SetNull);
                table.ForeignKey(name: "fk_sales_returns_warehouse_locations_location_id", column: x => x.location_id, principalTable: "warehouse_locations", principalColumn: "id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey(name: "fk_sales_returns_warehouses_warehouse_id", column: x => x.warehouse_id, principalTable: "warehouses", principalColumn: "id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "sales_shipment_lines",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                sales_shipment_id = table.Column<Guid>(type: "uuid", nullable: false),
                sales_order_line_id = table.Column<Guid>(type: "uuid", nullable: false),
                product_id = table.Column<Guid>(type: "uuid", nullable: false),
                quantity = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_sales_shipment_lines", x => x.id);
                table.CheckConstraint("ck_sales_shipment_lines_quantity_positive", "quantity > 0");
                table.ForeignKey(name: "fk_sales_shipment_lines_products_product_id", column: x => x.product_id, principalTable: "products", principalColumn: "id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey(name: "fk_sales_shipment_lines_sales_order_lines_sales_order_line_id", column: x => x.sales_order_line_id, principalTable: "sales_order_lines", principalColumn: "id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey(name: "fk_sales_shipment_lines_sales_shipments_sales_shipment_id", column: x => x.sales_shipment_id, principalTable: "sales_shipments", principalColumn: "id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey(name: "fk_sales_shipment_lines_tenants_tenant_id", column: x => x.tenant_id, principalTable: "tenants", principalColumn: "id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "sales_return_lines",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                sales_return_id = table.Column<Guid>(type: "uuid", nullable: false),
                sales_order_line_id = table.Column<Guid>(type: "uuid", nullable: false),
                product_id = table.Column<Guid>(type: "uuid", nullable: false),
                quantity = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_sales_return_lines", x => x.id);
                table.CheckConstraint("ck_sales_return_lines_quantity_positive", "quantity > 0");
                table.ForeignKey(name: "fk_sales_return_lines_products_product_id", column: x => x.product_id, principalTable: "products", principalColumn: "id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey(name: "fk_sales_return_lines_sales_order_lines_sales_order_line_id", column: x => x.sales_order_line_id, principalTable: "sales_order_lines", principalColumn: "id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey(name: "fk_sales_return_lines_sales_returns_sales_return_id", column: x => x.sales_return_id, principalTable: "sales_returns", principalColumn: "id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey(name: "fk_sales_return_lines_tenants_tenant_id", column: x => x.tenant_id, principalTable: "tenants", principalColumn: "id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(name: "ix_sales_operations_sales_order_id", table: "sales_operations", column: "sales_order_id");
        migrationBuilder.CreateIndex(name: "ix_sales_operations_tenant_id", table: "sales_operations", column: "tenant_id");
        migrationBuilder.CreateIndex(name: "ix_sales_operations_tenant_id_sales_order_id", table: "sales_operations", columns: new[] { "tenant_id", "sales_order_id" });

        migrationBuilder.CreateIndex(name: "ix_sales_order_lines_product_id", table: "sales_order_lines", column: "product_id");
        migrationBuilder.CreateIndex(name: "ix_sales_order_lines_sales_order_id_product_id", table: "sales_order_lines", columns: new[] { "sales_order_id", "product_id" }, unique: true);
        migrationBuilder.CreateIndex(name: "ix_sales_order_lines_tenant_id", table: "sales_order_lines", column: "tenant_id");

        migrationBuilder.CreateIndex(name: "ix_sales_orders_created_by_user_id", table: "sales_orders", column: "created_by_user_id");
        migrationBuilder.CreateIndex(name: "ix_sales_orders_location_id", table: "sales_orders", column: "location_id");
        migrationBuilder.CreateIndex(name: "ix_sales_orders_customer_id", table: "sales_orders", column: "customer_id");
        migrationBuilder.CreateIndex(name: "ix_sales_orders_tenant_id", table: "sales_orders", column: "tenant_id");
        migrationBuilder.CreateIndex(name: "ix_sales_orders_tenant_id_status", table: "sales_orders", columns: new[] { "tenant_id", "status" });
        migrationBuilder.CreateIndex(name: "ix_sales_orders_tenant_id_customer_id", table: "sales_orders", columns: new[] { "tenant_id", "customer_id" });
        migrationBuilder.CreateIndex(name: "ix_sales_orders_warehouse_id", table: "sales_orders", column: "warehouse_id");

        migrationBuilder.CreateIndex(name: "ix_sales_shipment_lines_product_id", table: "sales_shipment_lines", column: "product_id");
        migrationBuilder.CreateIndex(name: "ix_sales_shipment_lines_sales_order_line_id", table: "sales_shipment_lines", column: "sales_order_line_id");
        migrationBuilder.CreateIndex(name: "ix_sales_shipment_lines_sales_shipment_id_sales_order_line_id", table: "sales_shipment_lines", columns: new[] { "sales_shipment_id", "sales_order_line_id" }, unique: true);
        migrationBuilder.CreateIndex(name: "ix_sales_shipment_lines_tenant_id", table: "sales_shipment_lines", column: "tenant_id");

        migrationBuilder.CreateIndex(name: "ix_sales_shipments_created_by_user_id", table: "sales_shipments", column: "created_by_user_id");
        migrationBuilder.CreateIndex(name: "ix_sales_shipments_location_id", table: "sales_shipments", column: "location_id");
        migrationBuilder.CreateIndex(name: "ix_sales_shipments_operation_id", table: "sales_shipments", column: "operation_id", unique: true);
        migrationBuilder.CreateIndex(name: "ix_sales_shipments_sales_order_id", table: "sales_shipments", column: "sales_order_id");
        migrationBuilder.CreateIndex(name: "ix_sales_shipments_tenant_id", table: "sales_shipments", column: "tenant_id");
        migrationBuilder.CreateIndex(name: "ix_sales_shipments_tenant_id_sales_order_id", table: "sales_shipments", columns: new[] { "tenant_id", "sales_order_id" });
        migrationBuilder.CreateIndex(name: "ix_sales_shipments_warehouse_id", table: "sales_shipments", column: "warehouse_id");

        migrationBuilder.CreateIndex(name: "ix_sales_return_lines_product_id", table: "sales_return_lines", column: "product_id");
        migrationBuilder.CreateIndex(name: "ix_sales_return_lines_sales_order_line_id", table: "sales_return_lines", column: "sales_order_line_id");
        migrationBuilder.CreateIndex(name: "ix_sales_return_lines_sales_return_id_sales_order_line_id", table: "sales_return_lines", columns: new[] { "sales_return_id", "sales_order_line_id" }, unique: true);
        migrationBuilder.CreateIndex(name: "ix_sales_return_lines_tenant_id", table: "sales_return_lines", column: "tenant_id");

        migrationBuilder.CreateIndex(name: "ix_sales_returns_created_by_user_id", table: "sales_returns", column: "created_by_user_id");
        migrationBuilder.CreateIndex(name: "ix_sales_returns_location_id", table: "sales_returns", column: "location_id");
        migrationBuilder.CreateIndex(name: "ix_sales_returns_operation_id", table: "sales_returns", column: "operation_id", unique: true);
        migrationBuilder.CreateIndex(name: "ix_sales_returns_sales_order_id", table: "sales_returns", column: "sales_order_id");
        migrationBuilder.CreateIndex(name: "ix_sales_returns_tenant_id", table: "sales_returns", column: "tenant_id");
        migrationBuilder.CreateIndex(name: "ix_sales_returns_tenant_id_sales_order_id", table: "sales_returns", columns: new[] { "tenant_id", "sales_order_id" });
        migrationBuilder.CreateIndex(name: "ix_sales_returns_warehouse_id", table: "sales_returns", column: "warehouse_id");

        migrationBuilder.CreateIndex(name: "ix_customers_tenant_id", table: "customers", column: "tenant_id");
        migrationBuilder.CreateIndex(name: "ix_customers_tenant_id_code", table: "customers", columns: new[] { "tenant_id", "code" }, unique: true);
        migrationBuilder.CreateIndex(name: "ix_customers_tenant_id_is_active", table: "customers", columns: new[] { "tenant_id", "is_active" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "sales_operations");
        migrationBuilder.DropTable(name: "sales_shipment_lines");
        migrationBuilder.DropTable(name: "sales_return_lines");
        migrationBuilder.DropTable(name: "sales_shipments");
        migrationBuilder.DropTable(name: "sales_returns");
        migrationBuilder.DropTable(name: "sales_order_lines");
        migrationBuilder.DropTable(name: "sales_orders");
        migrationBuilder.DropTable(name: "customers");
    }
}
