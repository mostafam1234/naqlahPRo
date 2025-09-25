using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedDataForMainCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"-- Enable identity insert if Id is an identity column
SET IDENTITY_INSERT NA_MainCategory ON;

INSERT INTO NA_MainCategory (Id, ArabicName, EnglishName)
VALUES
(1, N'بلاستيك', 'Plastic'),
(2, N'ورق', 'Paper'),
(3, N'كرتون', 'Carton'),
(4, N'دواء', 'Medicine'),
(5, N'زجاج', 'Glass'),
(6, N'معدن', 'Metal'),
(7, N'خشب', 'Wood'),
(8, N'أقمشة', 'Textiles');

SET IDENTITY_INSERT NA_MainCategory OFF;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
