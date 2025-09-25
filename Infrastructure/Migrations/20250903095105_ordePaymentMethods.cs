using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ordePaymentMethods : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrderWayPointsStatus",
                table: "NA_OrderWayPoint",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PackImagePath",
                table: "NA_OrderWayPoint",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OrderNumber",
                table: "NA_Order",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "NA_PaymentMethod",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArabicName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EnglishName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NA_PaymentMethod", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NA_OrderPaymentMethod",
                columns: table => new
                {
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    PaymentMethodId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OrderPaymentStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NA_OrderPaymentMethod", x => new { x.OrderId, x.PaymentMethodId });
                    table.ForeignKey(
                        name: "FK_NA_OrderPaymentMethod_NA_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "NA_Order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NA_OrderPaymentMethod_NA_PaymentMethod_PaymentMethodId",
                        column: x => x.PaymentMethodId,
                        principalTable: "NA_PaymentMethod",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NA_OrderPaymentMethod_PaymentMethodId",
                table: "NA_OrderPaymentMethod",
                column: "PaymentMethodId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NA_OrderPaymentMethod");

            migrationBuilder.DropTable(
                name: "NA_PaymentMethod");

            migrationBuilder.DropColumn(
                name: "OrderWayPointsStatus",
                table: "NA_OrderWayPoint");

            migrationBuilder.DropColumn(
                name: "PackImagePath",
                table: "NA_OrderWayPoint");

            migrationBuilder.DropColumn(
                name: "OrderNumber",
                table: "NA_Order");
        }
    }
}
