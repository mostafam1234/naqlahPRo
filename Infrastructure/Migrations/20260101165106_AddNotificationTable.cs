using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NA_Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArabicTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    EnglishTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ArabicMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    EnglishMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    OrderId = table.Column<int>(type: "int", nullable: true),
                    NotificationType = table.Column<int>(type: "int", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NA_Notifications", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NA_Notifications_CreationDate",
                table: "NA_Notifications",
                column: "CreationDate");

            migrationBuilder.CreateIndex(
                name: "IX_NA_Notifications_IsRead",
                table: "NA_Notifications",
                column: "IsRead");

            migrationBuilder.CreateIndex(
                name: "IX_NA_Notifications_OrderId",
                table: "NA_Notifications",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_NA_Notifications_UserId",
                table: "NA_Notifications",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NA_Notifications");
        }
    }
}
