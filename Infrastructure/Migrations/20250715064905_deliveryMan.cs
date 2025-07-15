using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class deliveryMan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NA_DeliveryMan",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdentityNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FrontIdenitytImagePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BackIdenitytImagePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PersonalImagePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdentityExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DrivingLicenseExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FrontDrivingLicenseImage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeliveryType = table.Column<int>(type: "int", nullable: false),
                    DeliveryLicenseType = table.Column<int>(type: "int", nullable: false),
                    FrontDrivingLicenseImagePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BackDrivingLicenseImagePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    VehicleId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NA_DeliveryMan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NA_DeliveryMan_NA_User_UserId",
                        column: x => x.UserId,
                        principalTable: "NA_User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NA_VehicleBrand",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArabicName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EnglishName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NA_VehicleBrand", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NA_VehicleType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArabicName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EnglishName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NA_VehicleType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NA_DeliveryVehicle",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeliveryManId = table.Column<int>(type: "int", nullable: false),
                    VehicleTypeId = table.Column<int>(type: "int", nullable: false),
                    VehicleBrandId = table.Column<int>(type: "int", nullable: false),
                    LicensePlateNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FrontImagePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SideImagePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FrontLicenseImagePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BackLicenseImagePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LicenseExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FrontInsuranceImagePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BackInsuranceImagePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InSuranceExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VehicleOwnerType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NA_DeliveryVehicle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NA_DeliveryVehicle_NA_DeliveryMan_DeliveryManId",
                        column: x => x.DeliveryManId,
                        principalTable: "NA_DeliveryMan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NA_DeliveryVehicle_NA_VehicleBrand_VehicleBrandId",
                        column: x => x.VehicleBrandId,
                        principalTable: "NA_VehicleBrand",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NA_DeliveryVehicle_NA_VehicleType_VehicleTypeId",
                        column: x => x.VehicleTypeId,
                        principalTable: "NA_VehicleType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NA_Company",
                columns: table => new
                {
                    DeliveryVehicleId = table.Column<int>(type: "int", nullable: false),
                    CompanyName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CommercialRecordNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RecordImagePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TaxNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TaxCertificateImagePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BankAccountNumber = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NA_Company", x => x.DeliveryVehicleId);
                    table.ForeignKey(
                        name: "FK_NA_Company_NA_DeliveryVehicle_DeliveryVehicleId",
                        column: x => x.DeliveryVehicleId,
                        principalTable: "NA_DeliveryVehicle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NA_Renter",
                columns: table => new
                {
                    DeliveryVehicleId = table.Column<int>(type: "int", nullable: false),
                    CitizenName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdentityNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FrontIdentityImagePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BackIdentityImagePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RentContractImagePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BankAccountNumber = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NA_Renter", x => x.DeliveryVehicleId);
                    table.ForeignKey(
                        name: "FK_NA_Renter_NA_DeliveryVehicle_DeliveryVehicleId",
                        column: x => x.DeliveryVehicleId,
                        principalTable: "NA_DeliveryVehicle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NA_Resident",
                columns: table => new
                {
                    DeliveryVehicleId = table.Column<int>(type: "int", nullable: false),
                    CitizenName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FrontIdentityImagePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BackIdentityImagePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BankAccountNumber = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NA_Resident", x => x.DeliveryVehicleId);
                    table.ForeignKey(
                        name: "FK_NA_Resident_NA_DeliveryVehicle_DeliveryVehicleId",
                        column: x => x.DeliveryVehicleId,
                        principalTable: "NA_DeliveryVehicle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NA_DeliveryMan_UserId",
                table: "NA_DeliveryMan",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NA_DeliveryVehicle_DeliveryManId",
                table: "NA_DeliveryVehicle",
                column: "DeliveryManId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NA_DeliveryVehicle_VehicleBrandId",
                table: "NA_DeliveryVehicle",
                column: "VehicleBrandId");

            migrationBuilder.CreateIndex(
                name: "IX_NA_DeliveryVehicle_VehicleTypeId",
                table: "NA_DeliveryVehicle",
                column: "VehicleTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NA_Company");

            migrationBuilder.DropTable(
                name: "NA_Renter");

            migrationBuilder.DropTable(
                name: "NA_Resident");

            migrationBuilder.DropTable(
                name: "NA_DeliveryVehicle");

            migrationBuilder.DropTable(
                name: "NA_DeliveryMan");

            migrationBuilder.DropTable(
                name: "NA_VehicleBrand");

            migrationBuilder.DropTable(
                name: "NA_VehicleType");
        }
    }
}
