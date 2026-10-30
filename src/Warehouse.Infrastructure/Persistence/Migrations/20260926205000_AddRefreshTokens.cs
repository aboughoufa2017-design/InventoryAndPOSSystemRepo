using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehouse.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddRefreshTokens : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "refresh_tokens",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                device_id = table.Column<Guid>(type: "uuid", nullable: true),
                family_id = table.Column<Guid>(type: "uuid", nullable: false),
                token_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                expires_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                revoked_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                replaced_by_token_id = table.Column<Guid>(type: "uuid", nullable: true),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_refresh_tokens", x => x.id);
                table.ForeignKey(
                    name: "fk_refresh_tokens_devices_device_id",
                    column: x => x.device_id,
                    principalTable: "devices",
                    principalColumn: "id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "fk_refresh_tokens_refresh_tokens_replaced_by_token_id",
                    column: x => x.replaced_by_token_id,
                    principalTable: "refresh_tokens",
                    principalColumn: "id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "fk_refresh_tokens_tenants_tenant_id",
                    column: x => x.tenant_id,
                    principalTable: "tenants",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_refresh_tokens_users_user_id",
                    column: x => x.user_id,
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_refresh_tokens_device_id",
            table: "refresh_tokens",
            column: "device_id");

        migrationBuilder.CreateIndex(
            name: "ix_refresh_tokens_family_id",
            table: "refresh_tokens",
            column: "family_id");

        migrationBuilder.CreateIndex(
            name: "ix_refresh_tokens_replaced_by_token_id",
            table: "refresh_tokens",
            column: "replaced_by_token_id");

        migrationBuilder.CreateIndex(
            name: "ix_refresh_tokens_tenant_id",
            table: "refresh_tokens",
            column: "tenant_id");

        migrationBuilder.CreateIndex(
            name: "ix_refresh_tokens_tenant_id_user_id",
            table: "refresh_tokens",
            columns: new[] { "tenant_id", "user_id" });

        migrationBuilder.CreateIndex(
            name: "ix_refresh_tokens_token_hash",
            table: "refresh_tokens",
            column: "token_hash",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_refresh_tokens_user_id",
            table: "refresh_tokens",
            column: "user_id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "refresh_tokens");
    }
}
