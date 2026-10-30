using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehouse.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
[DbContext(typeof(WarehouseDbContext))]
[Migration("20261030190000_AddSelfCheckout")]
public partial class AddSelfCheckout : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "self_checkout_sessions",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                warehouse_id = table.Column<Guid>(type: "uuid", nullable: false),
                code = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                access_token = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                is_guest = table.Column<bool>(type: "boolean", nullable: false),
                status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                currency_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                subtotal = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                tax_total = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                grand_total = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                audit_required = table.Column<bool>(type: "boolean", nullable: false),
                audit_item_count = table.Column<int>(type: "integer", nullable: false),
                audit_target_barcodes_json = table.Column<string>(type: "text", nullable: true),
                audit_completed = table.Column<bool>(type: "boolean", nullable: false),
                expires_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_self_checkout_sessions", x => x.id);
                table.ForeignKey(
                    name: "fk_self_checkout_sessions_tenants_tenant_id",
                    column: x => x.tenant_id,
                    principalTable: "tenants",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "self_checkout_lines",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                session_id = table.Column<Guid>(type: "uuid", nullable: false),
                product_id = table.Column<Guid>(type: "uuid", nullable: false),
                sku_snapshot = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                name_snapshot = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                barcode_snapshot = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                quantity = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                unit_price = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                line_total = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_self_checkout_lines", x => x.id);
                table.ForeignKey(
                    name: "fk_self_checkout_lines_self_checkout_sessions_session_id",
                    column: x => x.session_id,
                    principalTable: "self_checkout_sessions",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_self_checkout_lines_tenants_tenant_id",
                    column: x => x.tenant_id,
                    principalTable: "tenants",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "ix_self_checkout_lines_session_id",
            table: "self_checkout_lines",
            column: "session_id");

        migrationBuilder.CreateIndex(
            name: "ix_self_checkout_lines_tenant_id_session_id",
            table: "self_checkout_lines",
            columns: new[] { "tenant_id", "session_id" });

        migrationBuilder.CreateIndex(
            name: "ix_self_checkout_sessions_access_token",
            table: "self_checkout_sessions",
            column: "access_token",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_self_checkout_sessions_tenant_id_code",
            table: "self_checkout_sessions",
            columns: new[] { "tenant_id", "code" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_self_checkout_sessions_tenant_id_status_expires_at_utc",
            table: "self_checkout_sessions",
            columns: new[] { "tenant_id", "status", "expires_at_utc" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "self_checkout_lines");
        migrationBuilder.DropTable(name: "self_checkout_sessions");
    }
}
