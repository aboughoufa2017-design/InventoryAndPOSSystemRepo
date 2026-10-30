using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehouse.Infrastructure.Persistence.Migrations;

public partial class AddTransfers : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "transfers",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                source_warehouse_id = table.Column<Guid>(type: "uuid", nullable: false),
                destination_warehouse_id = table.Column<Guid>(type: "uuid", nullable: false),
                source_location_id = table.Column<Guid>(type: "uuid", nullable: false),
                destination_location_id = table.Column<Guid>(type: "uuid", nullable: false),
                status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                notes = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                dispatch_operation_id = table.Column<Guid>(type: "uuid", nullable: true),
                created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                dispatched_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                completed_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                row_version = table.Column<int>(type: "integer", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_transfers", x => x.id);
                table.CheckConstraint("ck_transfers_status", "status IN ('Draft', 'Dispatched', 'PartiallyReceived', 'Completed', 'Cancelled')");
                table.CheckConstraint("ck_transfers_warehouses_differ", "source_warehouse_id <> destination_warehouse_id");
                table.ForeignKey(name: "fk_transfers_tenants_tenant_id", column: x => x.tenant_id, principalTable: "tenants", principalColumn: "id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey(name: "fk_transfers_users_created_by_user_id", column: x => x.created_by_user_id, principalTable: "users", principalColumn: "id", onDelete: ReferentialAction.SetNull);
                table.ForeignKey(name: "fk_transfers_warehouse_locations_destination_location_id", column: x => x.destination_location_id, principalTable: "warehouse_locations", principalColumn: "id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey(name: "fk_transfers_warehouse_locations_source_location_id", column: x => x.source_location_id, principalTable: "warehouse_locations", principalColumn: "id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey(name: "fk_transfers_warehouses_destination_warehouse_id", column: x => x.destination_warehouse_id, principalTable: "warehouses", principalColumn: "id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey(name: "fk_transfers_warehouses_source_warehouse_id", column: x => x.source_warehouse_id, principalTable: "warehouses", principalColumn: "id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "transfer_lines",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                transfer_id = table.Column<Guid>(type: "uuid", nullable: false),
                product_id = table.Column<Guid>(type: "uuid", nullable: false),
                quantity_requested = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                quantity_received = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                notes = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_transfer_lines", x => x.id);
                table.CheckConstraint("ck_transfer_lines_quantity_requested_positive", "quantity_requested > 0");
                table.CheckConstraint("ck_transfer_lines_quantity_received_non_negative", "quantity_received >= 0");
                table.CheckConstraint("ck_transfer_lines_received_not_over_requested", "quantity_received <= quantity_requested");
                table.ForeignKey(name: "fk_transfer_lines_products_product_id", column: x => x.product_id, principalTable: "products", principalColumn: "id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey(name: "fk_transfer_lines_tenants_tenant_id", column: x => x.tenant_id, principalTable: "tenants", principalColumn: "id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey(name: "fk_transfer_lines_transfers_transfer_id", column: x => x.transfer_id, principalTable: "transfers", principalColumn: "id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "transfer_operations",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                transfer_id = table.Column<Guid>(type: "uuid", nullable: false),
                action = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                result_payload_json = table.Column<string>(type: "jsonb", nullable: false),
                processed_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_transfer_operations", x => x.id);
                table.ForeignKey(name: "fk_transfer_operations_tenants_tenant_id", column: x => x.tenant_id, principalTable: "tenants", principalColumn: "id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey(name: "fk_transfer_operations_transfers_transfer_id", column: x => x.transfer_id, principalTable: "transfers", principalColumn: "id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(name: "ix_transfer_lines_product_id", table: "transfer_lines", column: "product_id");
        migrationBuilder.CreateIndex(name: "ix_transfer_lines_tenant_id", table: "transfer_lines", column: "tenant_id");
        migrationBuilder.CreateIndex(name: "ix_transfer_lines_transfer_id_product_id", table: "transfer_lines", columns: new[] { "transfer_id", "product_id" }, unique: true);

        migrationBuilder.CreateIndex(name: "ix_transfer_operations_tenant_id", table: "transfer_operations", column: "tenant_id");
        migrationBuilder.CreateIndex(name: "ix_transfer_operations_tenant_id_transfer_id", table: "transfer_operations", columns: new[] { "tenant_id", "transfer_id" });
        migrationBuilder.CreateIndex(name: "ix_transfer_operations_transfer_id", table: "transfer_operations", column: "transfer_id");

        migrationBuilder.CreateIndex(name: "ix_transfers_created_by_user_id", table: "transfers", column: "created_by_user_id");
        migrationBuilder.CreateIndex(name: "ix_transfers_destination_location_id", table: "transfers", column: "destination_location_id");
        migrationBuilder.CreateIndex(name: "ix_transfers_destination_warehouse_id", table: "transfers", column: "destination_warehouse_id");
        migrationBuilder.CreateIndex(name: "ix_transfers_dispatch_operation_id", table: "transfers", column: "dispatch_operation_id", unique: true);
        migrationBuilder.CreateIndex(name: "ix_transfers_source_location_id", table: "transfers", column: "source_location_id");
        migrationBuilder.CreateIndex(name: "ix_transfers_source_warehouse_id", table: "transfers", column: "source_warehouse_id");
        migrationBuilder.CreateIndex(name: "ix_transfers_tenant_id", table: "transfers", column: "tenant_id");
        migrationBuilder.CreateIndex(name: "ix_transfers_tenant_id_destination_warehouse_id", table: "transfers", columns: new[] { "tenant_id", "destination_warehouse_id" });
        migrationBuilder.CreateIndex(name: "ix_transfers_tenant_id_source_warehouse_id", table: "transfers", columns: new[] { "tenant_id", "source_warehouse_id" });
        migrationBuilder.CreateIndex(name: "ix_transfers_tenant_id_status", table: "transfers", columns: new[] { "tenant_id", "status" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "transfer_lines");
        migrationBuilder.DropTable(name: "transfer_operations");
        migrationBuilder.DropTable(name: "transfers");
    }
}
