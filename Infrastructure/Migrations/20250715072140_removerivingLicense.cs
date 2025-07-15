using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class removerivingLicense : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FrontDrivingLicenseImage",
                table: "NA_DeliveryMan");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FrontDrivingLicenseImage",
                table: "NA_DeliveryMan",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
