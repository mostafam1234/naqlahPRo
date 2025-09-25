using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class addStateColumnToDeliveryManTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DeliveryState",
                table: "NA_DeliveryMan",
                type: "int",
                nullable: false,
                defaultValue: 1);
        }
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveryState",
                table: "NA_DeliveryMan");
        }
    }
}
