using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
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
                name: "NA_OrderPackage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArabicDescripton = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EnglishDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MinWeightInKiloGram = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxWeightInKiloGram = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NA_OrderPackage", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NA_PaymentMethod",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArabicName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EnglishName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NA_PaymentMethod", x => x.Id);
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
                name: "Na_Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArabicName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Na_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NA_User",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    ActivationCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NA_User", x => x.Id);
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
                    EnglishName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IconImagePath = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NA_VehicleType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NA_Order",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    VehicleTypeId = table.Column<int>(type: "int", nullable: true),
                    DeliveryManId = table.Column<int>(type: "int", nullable: true),
                    OrderPackageId = table.Column<int>(type: "int", nullable: false),
                    OrderType = table.Column<int>(type: "int", nullable: false),
                    OrderStatus = table.Column<int>(type: "int", nullable: false),
                    VehicleTypdId = table.Column<int>(type: "int", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NA_Order", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NA_Order_NA_OrderPackage_OrderPackageId",
                        column: x => x.OrderPackageId,
                        principalTable: "NA_OrderPackage",
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
                name: "NA_RoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NA_RoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NA_RoleClaims_Na_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Na_Roles",
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
                    DeliveryType = table.Column<int>(type: "int", nullable: false),
                    DeliveryState = table.Column<int>(type: "int", nullable: false),
                    DeliveryLicenseType = table.Column<int>(type: "int", nullable: false),
                    FrontDrivingLicenseImagePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BackDrivingLicenseImagePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    VehicleId = table.Column<int>(type: "int", nullable: true),
                    AndriodDevice = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IosDevice = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false)
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
                name: "NA_UserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NA_UserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NA_UserClaims_NA_User_UserId",
                        column: x => x.UserId,
                        principalTable: "NA_User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NA_UserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NA_UserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_NA_UserLogins_NA_User_UserId",
                        column: x => x.UserId,
                        principalTable: "NA_User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NA_UserRoles",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NA_UserRoles", x => new { x.RoleId, x.UserId });
                    table.ForeignKey(
                        name: "FK_NA_UserRoles_NA_User_UserId",
                        column: x => x.UserId,
                        principalTable: "NA_User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NA_UserRoles_Na_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Na_Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NA_UserTokens",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NA_UserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_NA_UserTokens_NA_User_UserId",
                        column: x => x.UserId,
                        principalTable: "NA_User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NA_VehicleTypeCategory",
                columns: table => new
                {
                    VehicleTypeId = table.Column<int>(type: "int", nullable: false),
                    MainCategoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NA_VehicleTypeCategory", x => new { x.VehicleTypeId, x.MainCategoryId });
                    table.ForeignKey(
                        name: "FK_NA_VehicleTypeCategory_NA_MainCategory_MainCategoryId",
                        column: x => x.MainCategoryId,
                        principalTable: "NA_MainCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NA_VehicleTypeCategory_NA_VehicleType_VehicleTypeId",
                        column: x => x.VehicleTypeId,
                        principalTable: "NA_VehicleType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "NA_OrderPaymentMethod",
                columns: table => new
                {
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    PaymentMethodId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OrderPaymentStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NA_OrderPaymentMethod", x => new { x.OrderId, x.PaymentMethodId });
                    table.ForeignKey(
                        name: "FK_NA_OrderPaymentMethod_NA_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "NA_Order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NA_OrderPaymentMethod_NA_PaymentMethod_PaymentMethodId",
                        column: x => x.PaymentMethodId,
                        principalTable: "NA_PaymentMethod",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NA_OrderServices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    WorkId = table.Column<int>(type: "int", nullable: false),
                    ArabicName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EnglishName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NA_OrderServices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NA_OrderServices_NA_AssistanWork_WorkId",
                        column: x => x.WorkId,
                        principalTable: "NA_AssistanWork",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NA_OrderServices_NA_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "NA_Order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NA_OrderStatusHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    OrderStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NA_OrderStatusHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NA_OrderStatusHistory_NA_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "NA_Order",
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
                name: "NA_WalletTransctions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArabicDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EnglishDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Withdraw = table.Column<bool>(type: "bit", nullable: false),
                    OrderId = table.Column<int>(type: "int", nullable: true),
                    CustomerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NA_WalletTransctions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NA_WalletTransctions_NA_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "NA_Customer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeliveryManLocations",
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
                    table.PrimaryKey("PK_DeliveryManLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeliveryManLocations_NA_DeliveryMan_DeliveryManId",
                        column: x => x.DeliveryManId,
                        principalTable: "NA_DeliveryMan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                    PickedUpDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OrderWayPointsStatus = table.Column<int>(type: "int", nullable: false),
                    PackImagePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                    IdentityNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
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

            migrationBuilder.InsertData(
                table: "Na_Roles",
                columns: new[] { "Id", "ArabicName", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { 1, "أدمن", null, "Admin", "ADMIN" },
                    { 2, "الطيار", null, "DeliveryMan", "DELIVERYMAN" },
                    { 3, "العميل", null, "Customer", "Customer" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryManLocations_DeliveryManId",
                table: "DeliveryManLocations",
                column: "DeliveryManId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EstablishMentRepresentitive_EstablishmentId",
                table: "EstablishMentRepresentitive",
                column: "EstablishmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NA_Assistant_AssistanWorkId",
                table: "NA_Assistant",
                column: "AssistanWorkId");

            migrationBuilder.CreateIndex(
                name: "IX_NA_Assistant_DeliveryManId",
                table: "NA_Assistant",
                column: "DeliveryManId");

            migrationBuilder.CreateIndex(
                name: "IX_NA_City_RegionId",
                table: "NA_City",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_NA_Customer_UserId",
                table: "NA_Customer",
                column: "UserId",
                unique: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_NA_Neighborhood_CityId",
                table: "NA_Neighborhood",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_NA_Order_OrderPackageId",
                table: "NA_Order",
                column: "OrderPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_NA_OrderDetails_MainCategoryId",
                table: "NA_OrderDetails",
                column: "MainCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_NA_OrderDetails_OrderId",
                table: "NA_OrderDetails",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_NA_OrderPaymentMethod_PaymentMethodId",
                table: "NA_OrderPaymentMethod",
                column: "PaymentMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_NA_OrderServices_OrderId",
                table: "NA_OrderServices",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_NA_OrderServices_WorkId",
                table: "NA_OrderServices",
                column: "WorkId");

            migrationBuilder.CreateIndex(
                name: "IX_NA_OrderStatusHistory_OrderId",
                table: "NA_OrderStatusHistory",
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

            migrationBuilder.CreateIndex(
                name: "IX_NA_RoleClaims_RoleId",
                table: "NA_RoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "Na_Roles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "NA_User",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "NA_User",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_NA_UserClaims_UserId",
                table: "NA_UserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_NA_UserLogins_UserId",
                table: "NA_UserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_NA_UserRoles_UserId",
                table: "NA_UserRoles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_NA_VehicleTypeCategory_MainCategoryId",
                table: "NA_VehicleTypeCategory",
                column: "MainCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_NA_WalletTransctions_CustomerId",
                table: "NA_WalletTransctions",
                column: "CustomerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeliveryManLocations");

            migrationBuilder.DropTable(
                name: "EstablishMentRepresentitive");

            migrationBuilder.DropTable(
                name: "NA_Assistant");

            migrationBuilder.DropTable(
                name: "NA_Company");

            migrationBuilder.DropTable(
                name: "NA_Individual");

            migrationBuilder.DropTable(
                name: "NA_OrderDetails");

            migrationBuilder.DropTable(
                name: "NA_OrderPaymentMethod");

            migrationBuilder.DropTable(
                name: "NA_OrderServices");

            migrationBuilder.DropTable(
                name: "NA_OrderStatusHistory");

            migrationBuilder.DropTable(
                name: "NA_OrderWayPoint");

            migrationBuilder.DropTable(
                name: "NA_Renter");

            migrationBuilder.DropTable(
                name: "NA_Resident");

            migrationBuilder.DropTable(
                name: "NA_RoleClaims");

            migrationBuilder.DropTable(
                name: "NA_UserClaims");

            migrationBuilder.DropTable(
                name: "NA_UserLogins");

            migrationBuilder.DropTable(
                name: "NA_UserRoles");

            migrationBuilder.DropTable(
                name: "NA_UserTokens");

            migrationBuilder.DropTable(
                name: "NA_VehicleTypeCategory");

            migrationBuilder.DropTable(
                name: "NA_WalletTransctions");

            migrationBuilder.DropTable(
                name: "NA_EstablishMent");

            migrationBuilder.DropTable(
                name: "NA_PaymentMethod");

            migrationBuilder.DropTable(
                name: "NA_AssistanWork");

            migrationBuilder.DropTable(
                name: "NA_Neighborhood");

            migrationBuilder.DropTable(
                name: "NA_Order");

            migrationBuilder.DropTable(
                name: "NA_DeliveryVehicle");

            migrationBuilder.DropTable(
                name: "Na_Roles");

            migrationBuilder.DropTable(
                name: "NA_MainCategory");

            migrationBuilder.DropTable(
                name: "NA_Customer");

            migrationBuilder.DropTable(
                name: "NA_City");

            migrationBuilder.DropTable(
                name: "NA_OrderPackage");

            migrationBuilder.DropTable(
                name: "NA_DeliveryMan");

            migrationBuilder.DropTable(
                name: "NA_VehicleBrand");

            migrationBuilder.DropTable(
                name: "NA_VehicleType");

            migrationBuilder.DropTable(
                name: "NA_Region");

            migrationBuilder.DropTable(
                name: "NA_User");
        }
    }
}
