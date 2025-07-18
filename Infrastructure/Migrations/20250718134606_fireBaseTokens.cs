using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class fireBaseTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AndriodDevice",
                table: "NA_DeliveryMan",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IosDevice",
                table: "NA_DeliveryMan",
                type: "nvarchar(max)",
                nullable: false,    
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AndriodDevice",
                table: "NA_DeliveryMan");

            migrationBuilder.DropColumn(
                name: "IosDevice",
                table: "NA_DeliveryMan");
        }
    }
}
