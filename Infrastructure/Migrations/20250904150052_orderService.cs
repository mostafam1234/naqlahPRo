using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class orderService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NA_OrderPackage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArabicDescripton = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EnglishDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MinWeightInKiloGram = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxWeightInKiloGram = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NA_OrderPackage", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NA_OrderServices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    WorkId = table.Column<int>(type: "int", nullable: false),
                    ArabicName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EnglishName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NA_OrderServices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NA_OrderServices_NA_AssistanWork_WorkId",
                        column: x => x.WorkId,
                        principalTable: "NA_AssistanWork",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NA_OrderServices_NA_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "NA_Order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NA_OrderServices_OrderId",
                table: "NA_OrderServices",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_NA_OrderServices_WorkId",
                table: "NA_OrderServices",
                column: "WorkId");

            migrationBuilder.AddForeignKey(
                name: "FK_NA_Order_NA_OrderPackage_OrderPackageId",
                table: "NA_Order",
                column: "OrderPackageId",
                principalTable: "NA_OrderPackage",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NA_Order_NA_OrderPackage_OrderPackageId",
                table: "NA_Order");

            migrationBuilder.DropTable(
                name: "NA_OrderPackage");

            migrationBuilder.DropTable(
                name: "NA_OrderServices");
        }
    }
}
