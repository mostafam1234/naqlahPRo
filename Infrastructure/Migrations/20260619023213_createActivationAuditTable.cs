using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class createActivationAuditTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NA_DeliveryManActiveHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeliveryManId = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NA_DeliveryManActiveHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NA_DeliveryManActiveHistory_NA_DeliveryMan_DeliveryManId",
                        column: x => x.DeliveryManId,
                        principalTable: "NA_DeliveryMan",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_NA_DeliveryManActiveHistory_NA_User_ChangedByUserId",
                        column: x => x.ChangedByUserId,
                        principalTable: "NA_User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_NA_DeliveryManActiveHistory_ChangedAt",
                table: "NA_DeliveryManActiveHistory",
                column: "ChangedAt");

            migrationBuilder.CreateIndex(
                name: "IX_NA_DeliveryManActiveHistory_ChangedByUserId",
                table: "NA_DeliveryManActiveHistory",
                column: "ChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_NA_DeliveryManActiveHistory_DeliveryManId",
                table: "NA_DeliveryManActiveHistory",
                column: "DeliveryManId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NA_DeliveryManActiveHistory");
        }
    }
}
