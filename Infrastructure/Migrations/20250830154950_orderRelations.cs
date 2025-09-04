using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class orderRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryManLocation_NA_DeliveryMan_DeliveryManId",
                table: "DeliveryManLocation");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DeliveryManLocation",
                table: "DeliveryManLocation");

            migrationBuilder.RenameTable(
                name: "DeliveryManLocation",
                newName: "DeliveryManLocations");

            migrationBuilder.RenameIndex(
                name: "IX_DeliveryManLocation_DeliveryManId",
                table: "DeliveryManLocations",
                newName: "IX_DeliveryManLocations_DeliveryManId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DeliveryManLocations",
                table: "DeliveryManLocations",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "NA_MainCategory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArabicName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EnglishName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NA_MainCategory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NA_Order",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    OrderType = table.Column<int>(type: "int", nullable: false),
                    OrderStatus = table.Column<int>(type: "int", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NA_Order", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NA_Region",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArabicName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EnglishName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NA_Region", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NA_OrderDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    MainCategoryId = table.Column<int>(type: "int", nullable: false),
                    ArabicCategoryName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EnglishCategoryName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NA_OrderDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NA_OrderDetails_NA_MainCategory_MainCategoryId",
                        column: x => x.MainCategoryId,
                        principalTable: "NA_MainCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NA_OrderDetails_NA_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "NA_Order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NA_City",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArabicName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RegionId = table.Column<int>(type: "int", nullable: false),
                    EnglishName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NA_City", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NA_City_NA_Region_RegionId",
                        column: x => x.RegionId,
                        principalTable: "NA_Region",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NA_Neighborhood",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArabicName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CityId = table.Column<int>(type: "int", nullable: false),
                    EnglishName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NA_Neighborhood", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NA_Neighborhood_NA_City_CityId",
                        column: x => x.CityId,
                        principalTable: "NA_City",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NA_OrderWayPoint",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    longitude = table.Column<double>(type: "float", nullable: false),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    RegionId = table.Column<int>(type: "int", nullable: false),
                    CityId = table.Column<int>(type: "int", nullable: false),
                    NeighborhoodId = table.Column<int>(type: "int", nullable: false),
                    IsOrgin = table.Column<bool>(type: "bit", nullable: false),
                    IsDestination = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NA_OrderWayPoint", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NA_OrderWayPoint_NA_City_CityId",
                        column: x => x.CityId,
                        principalTable: "NA_City",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NA_OrderWayPoint_NA_Neighborhood_NeighborhoodId",
                        column: x => x.NeighborhoodId,
                        principalTable: "NA_Neighborhood",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NA_OrderWayPoint_NA_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "NA_Order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NA_OrderWayPoint_NA_Region_RegionId",
                        column: x => x.RegionId,
                        principalTable: "NA_Region",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NA_City_RegionId",
                table: "NA_City",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_NA_Neighborhood_CityId",
                table: "NA_Neighborhood",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_NA_OrderDetails_MainCategoryId",
                table: "NA_OrderDetails",
                column: "MainCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_NA_OrderDetails_OrderId",
                table: "NA_OrderDetails",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_NA_OrderWayPoint_CityId",
                table: "NA_OrderWayPoint",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_NA_OrderWayPoint_NeighborhoodId",
                table: "NA_OrderWayPoint",
                column: "NeighborhoodId");

            migrationBuilder.CreateIndex(
                name: "IX_NA_OrderWayPoint_OrderId",
                table: "NA_OrderWayPoint",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_NA_OrderWayPoint_RegionId",
                table: "NA_OrderWayPoint",
                column: "RegionId");

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryManLocations_NA_DeliveryMan_DeliveryManId",
                table: "DeliveryManLocations",
                column: "DeliveryManId",
                principalTable: "NA_DeliveryMan",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryManLocations_NA_DeliveryMan_DeliveryManId",
                table: "DeliveryManLocations");

            migrationBuilder.DropTable(
                name: "NA_OrderDetails");

            migrationBuilder.DropTable(
                name: "NA_OrderWayPoint");

            migrationBuilder.DropTable(
                name: "NA_MainCategory");

            migrationBuilder.DropTable(
                name: "NA_Neighborhood");

            migrationBuilder.DropTable(
                name: "NA_Order");

            migrationBuilder.DropTable(
                name: "NA_City");

            migrationBuilder.DropTable(
                name: "NA_Region");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DeliveryManLocations",
                table: "DeliveryManLocations");

            migrationBuilder.RenameTable(
                name: "DeliveryManLocations",
                newName: "DeliveryManLocation");

            migrationBuilder.RenameIndex(
                name: "IX_DeliveryManLocations_DeliveryManId",
                table: "DeliveryManLocation",
                newName: "IX_DeliveryManLocation_DeliveryManId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DeliveryManLocation",
                table: "DeliveryManLocation",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryManLocation_NA_DeliveryMan_DeliveryManId",
                table: "DeliveryManLocation",
                column: "DeliveryManId",
                principalTable: "NA_DeliveryMan",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
