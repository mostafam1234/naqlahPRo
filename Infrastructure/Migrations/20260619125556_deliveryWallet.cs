using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class deliveryWallet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ClientApprovedPickup",
                table: "NA_Order",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DriverConfirmedGoingToPickup",
                table: "NA_Order",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PickupReminderSentAt",
                table: "NA_Order",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VehicleOwnerName",
                table: "NA_DeliveryVehicle",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRead",
                table: "NA_CaptainNotifications",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "OrderId",
                table: "NA_CaptainNotifications",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReadDate",
                table: "NA_CaptainNotifications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "NA_CaptainNotifications",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "NA_DeliveryManWalletTransaction",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeliveryManId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsCredit = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    DescriptionAr = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OrderId = table.Column<int>(type: "int", nullable: true),
                    BankAccountId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NA_DeliveryManWalletTransaction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NA_DeliveryManWalletTransaction_NA_DeliveryMan_DeliveryManId",
                        column: x => x.DeliveryManId,
                        principalTable: "NA_DeliveryMan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NA_DeliveryManWalletTransaction_CreatedAt",
                table: "NA_DeliveryManWalletTransaction",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_NA_DeliveryManWalletTransaction_DeliveryManId",
                table: "NA_DeliveryManWalletTransaction",
                column: "DeliveryManId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NA_DeliveryManWalletTransaction");

            migrationBuilder.DropColumn(
                name: "ClientApprovedPickup",
                table: "NA_Order");

            migrationBuilder.DropColumn(
                name: "DriverConfirmedGoingToPickup",
                table: "NA_Order");

            migrationBuilder.DropColumn(
                name: "PickupReminderSentAt",
                table: "NA_Order");

            migrationBuilder.DropColumn(
                name: "VehicleOwnerName",
                table: "NA_DeliveryVehicle");

            migrationBuilder.DropColumn(
                name: "IsRead",
                table: "NA_CaptainNotifications");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "NA_CaptainNotifications");

            migrationBuilder.DropColumn(
                name: "ReadDate",
                table: "NA_CaptainNotifications");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "NA_CaptainNotifications");
        }
    }
}
