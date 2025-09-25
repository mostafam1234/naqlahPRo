using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedDataForOrderPack : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"-- Enable identity insert if Id is an identity column
SET IDENTITY_INSERT NA_OrderPackage ON;

INSERT INTO NA_OrderPackage (Id, ArabicDescripton, EnglishDescription, MinWeightInKiloGram, MaxWeightInKiloGram)
VALUES
(1, N'ربع حمولة', 'Quarter Load', 0, 250),
(2, N'نصف حمولة', 'Half Load', 251, 500),
(3, N'حمولة كاملة', 'Full Load', 501, 1000);

SET IDENTITY_INSERT NA_OrderPackage OFF;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
