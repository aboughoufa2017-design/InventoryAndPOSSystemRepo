using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehouse.Infrastructure.Persistence.Migrations;

public partial class AddSyncChangeLog : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "tenant_change_counters",
            columns: table => new
            {
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                last_change_version = table.Column<long>(type: "bigint", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_tenant_change_counters", x => x.tenant_id);
                table.ForeignKey(
                    name: "fk_tenant_change_counters_tenants_tenant_id",
                    column: x => x.tenant_id,
                    principalTable: "tenants",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "change_log",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                change_version = table.Column<long>(type: "bigint", nullable: false),
                entity_type = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                operation = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                payload_json = table.Column<string>(type: "jsonb", nullable: false),
                occurred_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                device_id = table.Column<Guid>(type: "uuid", nullable: true),
                user_id = table.Column<Guid>(type: "uuid", nullable: true),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_change_log", x => x.id);
                table.CheckConstraint("ck_change_log_change_version_positive", "change_version > 0");
                table.CheckConstraint("ck_change_log_operation", "operation IN ('Upsert', 'Deactivate')");
                table.ForeignKey(name: "fk_change_log_devices_device_id", column: x => x.device_id, principalTable: "devices", principalColumn: "id", onDelete: ReferentialAction.SetNull);
                table.ForeignKey(name: "fk_change_log_tenants_tenant_id", column: x => x.tenant_id, principalTable: "tenants", principalColumn: "id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey(name: "fk_change_log_users_user_id", column: x => x.user_id, principalTable: "users", principalColumn: "id", onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateIndex(name: "ix_change_log_device_id", table: "change_log", column: "device_id");
        migrationBuilder.CreateIndex(name: "ix_change_log_tenant_id", table: "change_log", column: "tenant_id");
        migrationBuilder.CreateIndex(name: "ix_change_log_tenant_id_change_version", table: "change_log", columns: new[] { "tenant_id", "change_version" }, unique: true);
        migrationBuilder.CreateIndex(name: "ix_change_log_tenant_id_entity_type_entity_id", table: "change_log", columns: new[] { "tenant_id", "entity_type", "entity_id" });
        migrationBuilder.CreateIndex(name: "ix_change_log_user_id", table: "change_log", column: "user_id");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "change_log");
        migrationBuilder.DropTable(name: "tenant_change_counters");
    }
}
