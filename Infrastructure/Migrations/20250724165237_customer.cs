using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class customer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Active",
                table: "NA_DeliveryMan",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "DeliveryManLocation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeliveryManId = table.Column<int>(type: "int", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryManLocation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeliveryManLocation_NA_DeliveryMan_DeliveryManId",
                        column: x => x.DeliveryManId,
                        principalTable: "NA_DeliveryMan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NA_Customer",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomerType = table.Column<int>(type: "int", nullable: false),
                    AndriodDevice = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IosDevice = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NA_Customer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NA_Customer_NA_User_UserId",
                        column: x => x.UserId,
                        principalTable: "NA_User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NA_EstablishMent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MobileNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RecoredImagePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TaxRegistrationNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TaxRegistrationImagePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NA_EstablishMent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NA_EstablishMent_NA_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "NA_Customer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NA_Individual",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<int>(type: "int", nullable: false),
                    MobileNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdentityNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FrontIdentityImagePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BackIdentityImagePath = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NA_Individual", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NA_Individual_NA_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "NA_Customer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EstablishMentRepresentitive",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EstablishmentId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FrontIdentityNumberImagePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BackIdentityNumberImagePath = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstablishMentRepresentitive", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EstablishMentRepresentitive_NA_EstablishMent_EstablishmentId",
                        column: x => x.EstablishmentId,
                        principalTable: "NA_EstablishMent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryManLocation_DeliveryManId",
                table: "DeliveryManLocation",
                column: "DeliveryManId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EstablishMentRepresentitive_EstablishmentId",
                table: "EstablishMentRepresentitive",
                column: "EstablishmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NA_Customer_UserId",
                table: "NA_Customer",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NA_EstablishMent_CustomerId",
                table: "NA_EstablishMent",
                column: "CustomerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NA_Individual_CustomerId",
                table: "NA_Individual",
                column: "CustomerId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeliveryManLocation");

            migrationBuilder.DropTable(
                name: "EstablishMentRepresentitive");

            migrationBuilder.DropTable(
                name: "NA_Individual");

            migrationBuilder.DropTable(
                name: "NA_EstablishMent");

            migrationBuilder.DropTable(
                name: "NA_Customer");

            migrationBuilder.DropColumn(
                name: "Active",
                table: "NA_DeliveryMan");
        }
    }
}
