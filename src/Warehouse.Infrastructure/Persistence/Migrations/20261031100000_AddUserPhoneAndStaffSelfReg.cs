using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Warehouse.Infrastructure.Persistence;

#nullable disable

namespace Warehouse.Infrastructure.Persistence.Migrations;

[DbContext(typeof(WarehouseDbContext))]
[Migration("20261031100000_AddUserPhoneAndStaffSelfReg")]
public partial class AddUserPhoneAndStaffSelfReg : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "allow_staff_self_registration",
            table: "tenants",
            type: "boolean",
            nullable: false,
            defaultValue: true);

        migrationBuilder.AddColumn<string>(
            name: "phone_number",
            table: "users",
            type: "character varying(32)",
            maxLength: 32,
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "ix_users_tenant_id_phone_number",
            table: "users",
            columns: new[] { "tenant_id", "phone_number" },
            unique: true,
            filter: "phone_number IS NOT NULL");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "ix_users_tenant_id_phone_number",
            table: "users");

        migrationBuilder.DropColumn(
            name: "phone_number",
            table: "users");

        migrationBuilder.DropColumn(
            name: "allow_staff_self_registration",
            table: "tenants");
    }
}
