using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class systemCOnfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Cost",
                table: "NA_VehicleType",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Cost",
                table: "NA_AssistanWork",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "NA_SystemCOnfiguration",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BaseKm = table.Column<int>(type: "int", nullable: false),
                    BaseKmRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ExtraKmRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BaseHours = table.Column<int>(type: "int", nullable: false),
                    BaseHourRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ExtraHourRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VatRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ServiceFess = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NA_SystemCOnfiguration", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NA_SystemCOnfiguration");

            migrationBuilder.DropColumn(
                name: "Cost",
                table: "NA_VehicleType");

            migrationBuilder.DropColumn(
                name: "Cost",
                table: "NA_AssistanWork");
        }
    }
}
