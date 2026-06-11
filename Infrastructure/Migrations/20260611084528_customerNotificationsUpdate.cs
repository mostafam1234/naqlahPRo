using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class customerNotificationsUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAdminBroadcast",
                table: "NA_Notifications",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsProcessed",
                table: "NA_Notifications",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TargetAll",
                table: "NA_Notifications",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "TargetCustomerType",
                table: "NA_Notifications",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_NA_Notifications_IsAdminBroadcast_IsScheduled_IsProcessed",
                table: "NA_Notifications",
                columns: new[] { "IsAdminBroadcast", "IsScheduled", "IsProcessed" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_NA_Notifications_IsAdminBroadcast_IsScheduled_IsProcessed",
                table: "NA_Notifications");

            migrationBuilder.DropColumn(
                name: "IsAdminBroadcast",
                table: "NA_Notifications");

            migrationBuilder.DropColumn(
                name: "IsProcessed",
                table: "NA_Notifications");

            migrationBuilder.DropColumn(
                name: "TargetAll",
                table: "NA_Notifications");

            migrationBuilder.DropColumn(
                name: "TargetCustomerType",
                table: "NA_Notifications");
        }
    }
}
