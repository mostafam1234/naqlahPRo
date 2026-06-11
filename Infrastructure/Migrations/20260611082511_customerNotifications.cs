using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class customerNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsScheduled",
                table: "NA_Notifications",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduledDate",
                table: "NA_Notifications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "NA_CustomerNotification",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NotificationId = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    ReadDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NA_CustomerNotification", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NA_CustomerNotification_NA_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "NA_Customer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NA_CustomerNotification_NA_Notifications_NotificationId",
                        column: x => x.NotificationId,
                        principalTable: "NA_Notifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NA_CustomerNotification_CustomerId",
                table: "NA_CustomerNotification",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_NA_CustomerNotification_CustomerId_IsRead",
                table: "NA_CustomerNotification",
                columns: new[] { "CustomerId", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_NA_CustomerNotification_NotificationId",
                table: "NA_CustomerNotification",
                column: "NotificationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NA_CustomerNotification");

            migrationBuilder.DropColumn(
                name: "IsScheduled",
                table: "NA_Notifications");

            migrationBuilder.DropColumn(
                name: "ScheduledDate",
                table: "NA_Notifications");
        }
    }
}
