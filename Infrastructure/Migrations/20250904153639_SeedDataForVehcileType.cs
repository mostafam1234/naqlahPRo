using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedDataForVehcileType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"-------------------------------------------------
-- 1. Seed Vehicle Types
-------------------------------------------------

INSERT INTO NA_VehicleType (ArabicName, EnglishName)
VALUES
(N'ربع نقل', 'Quarter Truck'),
(N'نص نقل', 'Half Truck'),
(N'تلاجة', 'Refrigerated Truck');



-------------------------------------------------
-- 2. Seed VehicleTypeCategory (Bridge Table)
-- This assumes MainCategories already exist
-------------------------------------------------
-- Example: Cartesian insert between VehicleType and MainCategory
INSERT INTO NA_VehicleTypeCategory (VehicleTypeId, MainCategoryId)
SELECT vt.Id, mc.Id
FROM NA_VehicleType vt
CROSS JOIN NA_MainCategory mc;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
