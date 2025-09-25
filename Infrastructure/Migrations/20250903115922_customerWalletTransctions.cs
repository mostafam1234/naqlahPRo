using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class customerWalletTransctions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PickedUpDate",
                table: "NA_OrderWayPoint",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeliveryManId",
                table: "NA_Order",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "NA_OrderStatusHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    OrderStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NA_OrderStatusHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NA_OrderStatusHistory_NA_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "NA_Order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NA_WalletTransctions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArabicDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EnglishDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Withdraw = table.Column<bool>(type: "bit", nullable: false),
                    OrderId = table.Column<int>(type: "int", nullable: true),
                    CustomerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NA_WalletTransctions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NA_WalletTransctions_NA_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "NA_Customer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NA_OrderStatusHistory_OrderId",
                table: "NA_OrderStatusHistory",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_NA_WalletTransctions_CustomerId",
                table: "NA_WalletTransctions",
                column: "CustomerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NA_OrderStatusHistory");

            migrationBuilder.DropTable(
                name: "NA_WalletTransctions");

            migrationBuilder.DropColumn(
                name: "PickedUpDate",
                table: "NA_OrderWayPoint");

            migrationBuilder.DropColumn(
                name: "DeliveryManId",
                table: "NA_Order");
        }
    }
}
