using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class deliveryAssitant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NA_AssistanWork",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArabicName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EnglishName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NA_AssistanWork", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NA_Assistant",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssistanWorkId = table.Column<int>(type: "int", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdentityNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeliveryManId = table.Column<int>(type: "int", nullable: true),
                    FrontIdentityImagePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BackIdentityImagePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdentityExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NA_Assistant", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NA_Assistant_NA_AssistanWork_AssistanWorkId",
                        column: x => x.AssistanWorkId,
                        principalTable: "NA_AssistanWork",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NA_Assistant_NA_DeliveryMan_DeliveryManId",
                        column: x => x.DeliveryManId,
                        principalTable: "NA_DeliveryMan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NA_Assistant_AssistanWorkId",
                table: "NA_Assistant",
                column: "AssistanWorkId");

            migrationBuilder.CreateIndex(
                name: "IX_NA_Assistant_DeliveryManId",
                table: "NA_Assistant",
                column: "DeliveryManId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NA_Assistant");

            migrationBuilder.DropTable(
                name: "NA_AssistanWork");
        }
    }
}
