using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Warehouse.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPosModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "warehouse_id",
                table: "devices",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "cash_shifts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    warehouse_id = table.Column<Guid>(type: "uuid", nullable: false),
                    device_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cashier_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    opening_float = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                    closing_counted_cash = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: true),
                    opened_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    closed_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    row_version = table.Column<int>(type: "integer", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cash_shifts", x => x.id);
                    table.ForeignKey(
                        name: "fk_cash_shifts_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "pos_inventory_conflicts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    warehouse_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_command_id = table.Column<Guid>(type: "uuid", nullable: true),
                    pos_sale_id = table.Column<Guid>(type: "uuid", nullable: true),
                    resulting_on_hand = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    message = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    resolved_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pos_inventory_conflicts", x => x.id);
                    table.ForeignKey(
                        name: "fk_pos_inventory_conflicts_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "pos_sales",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    warehouse_id = table.Column<Guid>(type: "uuid", nullable: false),
                    location_id = table.Column<Guid>(type: "uuid", nullable: false),
                    device_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cashier_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cash_shift_id = table.Column<Guid>(type: "uuid", nullable: true),
                    client_command_id = table.Column<Guid>(type: "uuid", nullable: false),
                    receipt_number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    currency_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    subtotal = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                    discount_total = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                    tax_total = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                    grand_total = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                    sold_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    voided_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    row_version = table.Column<int>(type: "integer", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pos_sales", x => x.id);
                    table.ForeignKey(
                        name: "fk_pos_sales_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "price_lists",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    currency_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_price_lists", x => x.id);
                    table.ForeignKey(
                        name: "fk_price_lists_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "restaurant_tables",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    warehouse_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_restaurant_tables", x => x.id);
                    table.ForeignKey(
                        name: "fk_restaurant_tables_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "cash_movements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cash_shift_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                    reason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cash_movements", x => x.id);
                    table.ForeignKey(
                        name: "fk_cash_movements_cash_shifts_cash_shift_id",
                        column: x => x.cash_shift_id,
                        principalTable: "cash_shifts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_cash_movements_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "pos_payments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pos_sale_id = table.Column<Guid>(type: "uuid", nullable: false),
                    method = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    reference = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pos_payments", x => x.id);
                    table.ForeignKey(
                        name: "fk_pos_payments_pos_sales_pos_sale_id",
                        column: x => x.pos_sale_id,
                        principalTable: "pos_sales",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_pos_payments_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "pos_sale_lines",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pos_sale_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sku_snapshot = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    name_snapshot = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                    discount_amount = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                    tax_amount = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                    line_total = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pos_sale_lines", x => x.id);
                    table.ForeignKey(
                        name: "fk_pos_sale_lines_pos_sales_pos_sale_id",
                        column: x => x.pos_sale_id,
                        principalTable: "pos_sales",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_pos_sale_lines_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "product_prices",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    price_list_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                    min_quantity = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_prices", x => x.id);
                    table.ForeignKey(
                        name: "fk_product_prices_price_lists_price_list_id",
                        column: x => x.price_list_id,
                        principalTable: "price_lists",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_product_prices_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "restaurant_open_orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    warehouse_id = table.Column<Guid>(type: "uuid", nullable: false),
                    table_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    opened_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    closed_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    pos_sale_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_restaurant_open_orders", x => x.id);
                    table.ForeignKey(
                        name: "fk_restaurant_open_orders_restaurant_tables_table_id",
                        column: x => x.table_id,
                        principalTable: "restaurant_tables",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_restaurant_open_orders_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "permissions",
                columns: new[] { "id", "code", "description", "name" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111116"), "pos.sale", null, "POS checkout" },
                    { new Guid("11111111-1111-1111-1111-111111111117"), "pos.void", null, "Void POS sales" },
                    { new Guid("11111111-1111-1111-1111-111111111118"), "pos.refund", null, "Refund POS sales" },
                    { new Guid("11111111-1111-1111-1111-111111111119"), "pos.shift", null, "POS cash shifts" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_devices_tenant_id_warehouse_id",
                table: "devices",
                columns: new[] { "tenant_id", "warehouse_id" });

            migrationBuilder.CreateIndex(
                name: "ix_devices_warehouse_id",
                table: "devices",
                column: "warehouse_id");

            migrationBuilder.CreateIndex(
                name: "ix_cash_movements_cash_shift_id",
                table: "cash_movements",
                column: "cash_shift_id");

            migrationBuilder.CreateIndex(
                name: "ix_cash_movements_tenant_id_cash_shift_id",
                table: "cash_movements",
                columns: new[] { "tenant_id", "cash_shift_id" });

            migrationBuilder.CreateIndex(
                name: "ix_cash_shifts_tenant_id_warehouse_id_status",
                table: "cash_shifts",
                columns: new[] { "tenant_id", "warehouse_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_pos_inventory_conflicts_tenant_id_status",
                table: "pos_inventory_conflicts",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_pos_payments_pos_sale_id",
                table: "pos_payments",
                column: "pos_sale_id");

            migrationBuilder.CreateIndex(
                name: "ix_pos_payments_tenant_id_pos_sale_id",
                table: "pos_payments",
                columns: new[] { "tenant_id", "pos_sale_id" });

            migrationBuilder.CreateIndex(
                name: "ix_pos_sale_lines_pos_sale_id",
                table: "pos_sale_lines",
                column: "pos_sale_id");

            migrationBuilder.CreateIndex(
                name: "ix_pos_sale_lines_tenant_id_pos_sale_id",
                table: "pos_sale_lines",
                columns: new[] { "tenant_id", "pos_sale_id" });

            migrationBuilder.CreateIndex(
                name: "ix_pos_sales_tenant_id_client_command_id",
                table: "pos_sales",
                columns: new[] { "tenant_id", "client_command_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_pos_sales_tenant_id_receipt_number",
                table: "pos_sales",
                columns: new[] { "tenant_id", "receipt_number" });

            migrationBuilder.CreateIndex(
                name: "ix_pos_sales_tenant_id_warehouse_id_sold_at_utc",
                table: "pos_sales",
                columns: new[] { "tenant_id", "warehouse_id", "sold_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_price_lists_tenant_id_code",
                table: "price_lists",
                columns: new[] { "tenant_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_product_prices_price_list_id",
                table: "product_prices",
                column: "price_list_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_prices_tenant_id_price_list_id_product_id_min_quant",
                table: "product_prices",
                columns: new[] { "tenant_id", "price_list_id", "product_id", "min_quantity" });

            migrationBuilder.CreateIndex(
                name: "ix_restaurant_open_orders_table_id",
                table: "restaurant_open_orders",
                column: "table_id");

            migrationBuilder.CreateIndex(
                name: "ix_restaurant_open_orders_tenant_id_warehouse_id_status",
                table: "restaurant_open_orders",
                columns: new[] { "tenant_id", "warehouse_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_restaurant_tables_tenant_id_warehouse_id_code",
                table: "restaurant_tables",
                columns: new[] { "tenant_id", "warehouse_id", "code" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_devices_warehouses_warehouse_id",
                table: "devices",
                column: "warehouse_id",
                principalTable: "warehouses",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_devices_warehouses_warehouse_id",
                table: "devices");

            migrationBuilder.DropTable(
                name: "cash_movements");

            migrationBuilder.DropTable(
                name: "pos_inventory_conflicts");

            migrationBuilder.DropTable(
                name: "pos_payments");

            migrationBuilder.DropTable(
                name: "pos_sale_lines");

            migrationBuilder.DropTable(
                name: "product_prices");

            migrationBuilder.DropTable(
                name: "restaurant_open_orders");

            migrationBuilder.DropTable(
                name: "cash_shifts");

            migrationBuilder.DropTable(
                name: "pos_sales");

            migrationBuilder.DropTable(
                name: "price_lists");

            migrationBuilder.DropTable(
                name: "restaurant_tables");

            migrationBuilder.DropIndex(
                name: "ix_devices_tenant_id_warehouse_id",
                table: "devices");

            migrationBuilder.DropIndex(
                name: "ix_devices_warehouse_id",
                table: "devices");

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111116"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111117"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111118"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111119"));

            migrationBuilder.DropColumn(
                name: "warehouse_id",
                table: "devices");
        }
    }
}
