using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OrderPackConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrderPackageId",
                table: "NA_Order",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "NA_VehicleTypeCategory",
                columns: table => new
                {
                    VehicleTypeId = table.Column<int>(type: "int", nullable: false),
                    MainCategoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NA_VehicleTypeCategory", x => new { x.VehicleTypeId, x.MainCategoryId });
                    table.ForeignKey(
                        name: "FK_NA_VehicleTypeCategory_NA_MainCategory_MainCategoryId",
                        column: x => x.MainCategoryId,
                        principalTable: "NA_MainCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NA_VehicleTypeCategory_NA_VehicleType_VehicleTypeId",
                        column: x => x.VehicleTypeId,
                        principalTable: "NA_VehicleType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NA_Order_OrderPackageId",
                table: "NA_Order",
                column: "OrderPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_NA_VehicleTypeCategory_MainCategoryId",
                table: "NA_VehicleTypeCategory",
                column: "MainCategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NA_VehicleTypeCategory");

            migrationBuilder.DropIndex(
                name: "IX_NA_Order_OrderPackageId",
                table: "NA_Order");

            migrationBuilder.DropColumn(
                name: "OrderPackageId",
                table: "NA_Order");
        }
    }
}
