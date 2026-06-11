using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class orderRating : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                table: "NA_Order",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "DiscountCodeId",
                table: "NA_Order",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiscountCodeName",
                table: "NA_Order",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "NA_OrderRating",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    DeliveryManId = table.Column<int>(type: "int", nullable: true),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NA_OrderRating", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NA_OrderRating_NA_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "NA_Customer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NA_OrderRating_NA_DeliveryMan_DeliveryManId",
                        column: x => x.DeliveryManId,
                        principalTable: "NA_DeliveryMan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NA_OrderRating_NA_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "NA_Order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NA_OrderRating_CustomerId",
                table: "NA_OrderRating",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_NA_OrderRating_DeliveryManId",
                table: "NA_OrderRating",
                column: "DeliveryManId");

            migrationBuilder.CreateIndex(
                name: "IX_NA_OrderRating_OrderId_CustomerId",
                table: "NA_OrderRating",
                columns: new[] { "OrderId", "CustomerId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NA_OrderRating");

            migrationBuilder.DropColumn(
                name: "DiscountAmount",
                table: "NA_Order");

            migrationBuilder.DropColumn(
                name: "DiscountCodeId",
                table: "NA_Order");

            migrationBuilder.DropColumn(
                name: "DiscountCodeName",
                table: "NA_Order");
        }
    }
}
