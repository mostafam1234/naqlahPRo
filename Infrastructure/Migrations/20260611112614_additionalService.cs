using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class additionalService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NA_AdditionalService",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArabicName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    EnglishName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NA_AdditionalService", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NA_OrderAdditionalServices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    AdditionalServiceId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NA_OrderAdditionalServices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NA_OrderAdditionalServices_NA_AdditionalService_AdditionalServiceId",
                        column: x => x.AdditionalServiceId,
                        principalTable: "NA_AdditionalService",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NA_OrderAdditionalServices_NA_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "NA_Order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NA_OrderAdditionalServices_AdditionalServiceId",
                table: "NA_OrderAdditionalServices",
                column: "AdditionalServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_NA_OrderAdditionalServices_OrderId",
                table: "NA_OrderAdditionalServices",
                column: "OrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NA_OrderAdditionalServices");

            migrationBuilder.DropTable(
                name: "NA_AdditionalService");
        }
    }
}
