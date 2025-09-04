using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedDataForPamentMethod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"-- Enable identity insert if Id is an identity column
SET IDENTITY_INSERT NA_PaymentMethod ON;

INSERT INTO NA_PaymentMethod (Id, ArabicName, EnglishName, Active)
VALUES 
(1, N'نقدي', 'Cash', 1),   -- Cash
(2, N'اونلاين', 'Online', 1), -- Online
(3, N'محفظة', 'Wallet', 1); -- Wallet

SET IDENTITY_INSERT NA_PaymentMethod OFF;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
