using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedDataForRegions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"-------------------------------------------------
-- Seed Regions
-------------------------------------------------
SET IDENTITY_INSERT NA_Region ON;

INSERT INTO NA_Region (Id, ArabicName, EnglishName)
VALUES 
(1, N'الرياض', 'Riyadh'),
(2, N'مكة المكرمة', 'Makkah'),
(3, N'المدينة المنورة', 'Madinah'),
(4, N'الشرقية', 'Eastern Province'),
(5, N'عسير', 'Asir'),
(6, N'القصيم', 'Qassim'),
(7, N'حائل', 'Hail'),
(8, N'تبوك', 'Tabuk'),
(9, N'الباحة', 'Baha'),
(10, N'الجوف', 'Al Jouf'),
(11, N'نجران', 'Najran'),
(12, N'جازان', 'Jazan');

SET IDENTITY_INSERT NA_Region OFF;


-------------------------------------------------
-- Seed Cities
-------------------------------------------------
SET IDENTITY_INSERT NA_City ON;

INSERT INTO NA_City (Id, ArabicName, EnglishName, RegionId)
VALUES
-- Riyadh (Region 1)
(1, N'الرياض', 'Riyadh', 1),

-- Makkah (Region 2)
(2, N'مكة', 'Makkah', 2),

-- Madinah (Region 3)
(3, N'المدينة', 'Madinah', 3),

-- Eastern Province (Region 4)
(4, N'الدمام', 'Dammam', 4),

-- Asir (Region 5)
(5, N'أبها', 'Abha', 5),

-- Qassim (Region 6)
(6, N'بريدة', 'Buraidah', 6),

-- Hail (Region 7)
(7, N'حائل', 'Hail', 7),

-- Tabuk (Region 8)
(8, N'تبوك', 'Tabuk', 8),

-- Baha (Region 9)
(9, N'الباحة', 'Baha', 9),

-- Al Jouf (Region 10)
(10, N'سكاكا', 'Sakaka', 10),

-- Najran (Region 11)
(11, N'نجران', 'Najran', 11),

-- Jazan (Region 12)
(12, N'جيزان', 'Jazan', 12);

SET IDENTITY_INSERT NA_City OFF;


-------------------------------------------------
-- Seed Neighborhoods
-------------------------------------------------
SET IDENTITY_INSERT NA_Neighborhood ON;

INSERT INTO NA_Neighborhood (Id, ArabicName, EnglishName, CityId)
VALUES
-- Riyadh City
(1, N'الملز', 'Al Malaz', 1),

-- Makkah City
(2, N'العزيزية', 'Al Aziziyah', 2),

-- Madinah City
(3, N'قربان', 'Qurban', 3),

-- Dammam City
(4, N'الشاطئ', 'Al Shati', 4),

-- Abha City
(5, N'المحالة', 'Al Mahalah', 5),

-- Buraidah City
(6, N'الفايزية', 'Al Fayziyah', 6),

-- Hail City
(7, N'النقرة', 'Al Naqrah', 7),

-- Tabuk City
(8, N'المروج', 'Al Muruj', 8),

-- Baha City
(9, N'شهبة', 'Shahbah', 9),

-- Sakaka City
(10, N'القدس', 'Al Quds', 10),

-- Najran City
(11, N'الفهد', 'Al Fahd', 11),

-- Jazan City
(12, N'الروضة', 'Al Rawdah', 12);

SET IDENTITY_INSERT NA_Neighborhood OFF;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
