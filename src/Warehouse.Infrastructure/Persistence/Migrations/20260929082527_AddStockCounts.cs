using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehouse.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStockCounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "stock_counts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    warehouse_id = table.Column<Guid>(type: "uuid", nullable: false),
                    location_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    notes = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    posted_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    posted_adjustment_id = table.Column<Guid>(type: "uuid", nullable: true),
                    row_version = table.Column<int>(type: "integer", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stock_counts", x => x.id);
                    table.CheckConstraint("ck_stock_counts_status", "status IN ('Draft', 'Counted', 'Posted', 'Cancelled')");
                    table.ForeignKey(
                        name: "fk_stock_counts_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_stock_counts_warehouse_locations_location_id",
                        column: x => x.location_id,
                        principalTable: "warehouse_locations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_stock_counts_warehouses_warehouse_id",
                        column: x => x.warehouse_id,
                        principalTable: "warehouses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "stock_count_lines",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    stock_count_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    system_quantity = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    counted_quantity = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    notes = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stock_count_lines", x => x.id);
                    table.ForeignKey(
                        name: "fk_stock_count_lines_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_stock_count_lines_stock_counts_stock_count_id",
                        column: x => x.stock_count_id,
                        principalTable: "stock_counts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_stock_count_lines_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_stock_count_lines_product_id",
                table: "stock_count_lines",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_stock_count_lines_stock_count_id_product_id",
                table: "stock_count_lines",
                columns: new[] { "stock_count_id", "product_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_stock_count_lines_tenant_id",
                table: "stock_count_lines",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_stock_counts_location_id",
                table: "stock_counts",
                column: "location_id");

            migrationBuilder.CreateIndex(
                name: "ix_stock_counts_tenant_id",
                table: "stock_counts",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_stock_counts_tenant_id_warehouse_id_status",
                table: "stock_counts",
                columns: new[] { "tenant_id", "warehouse_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_stock_counts_warehouse_id",
                table: "stock_counts",
                column: "warehouse_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "stock_count_lines");

            migrationBuilder.DropTable(
                name: "stock_counts");
        }
    }
}
