using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehouse.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddSaaSPlatform : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "plans",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                is_active = table.Column<bool>(type: "boolean", nullable: false),
                max_users = table.Column<int>(type: "integer", nullable: true),
                max_warehouses = table.Column<int>(type: "integer", nullable: true),
                max_devices = table.Column<int>(type: "integer", nullable: true),
                max_products = table.Column<int>(type: "integer", nullable: true),
                feature_codes = table.Column<List<string>>(type: "character varying(64)[]", nullable: false),
                created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_plans", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "platform_operators",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                password_hash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                display_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                is_active = table.Column<bool>(type: "boolean", nullable: false),
                created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_platform_operators", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "tenant_subscriptions",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                trial_ends_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                current_period_start_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                current_period_end_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                external_reference = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                row_version = table.Column<int>(type: "integer", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_tenant_subscriptions", x => x.id);
                table.CheckConstraint("ck_tenant_subscriptions_status", "status IN ('Trial', 'Active', 'PastDue', 'Cancelled')");
                table.ForeignKey(
                    name: "fk_tenant_subscriptions_plans_plan_id",
                    column: x => x.plan_id,
                    principalTable: "plans",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "ix_plans_code",
            table: "plans",
            column: "code",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_platform_operators_email",
            table: "platform_operators",
            column: "email",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_tenant_subscriptions_plan_id",
            table: "tenant_subscriptions",
            column: "plan_id");

        migrationBuilder.CreateIndex(
            name: "ix_tenant_subscriptions_tenant_id",
            table: "tenant_subscriptions",
            column: "tenant_id",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_tenant_subscriptions_tenant_id_status",
            table: "tenant_subscriptions",
            columns: new[] { "tenant_id", "status" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "tenant_subscriptions");
        migrationBuilder.DropTable(name: "platform_operators");
        migrationBuilder.DropTable(name: "plans");
    }
}
