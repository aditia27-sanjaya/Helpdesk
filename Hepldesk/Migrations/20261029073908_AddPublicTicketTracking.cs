using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hepldesk.Migrations
{
    /// <inheritdoc />
    public partial class AddPublicTicketTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmailPelapor",
                table: "Tikets",
                type: "nvarchar(254)",
                maxLength: 254,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IzinkanPelacakanPublik",
                table: "Tikets",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "KodePelacakanHash",
                table: "Tikets",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NamaPelapor",
                table: "Tikets",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NomorHpPelapor",
                table: "Tikets",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NomorTiketPublik",
                table: "Tikets",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailPelapor",
                table: "Tikets");

            migrationBuilder.DropColumn(
                name: "IzinkanPelacakanPublik",
                table: "Tikets");

            migrationBuilder.DropColumn(
                name: "KodePelacakanHash",
                table: "Tikets");

            migrationBuilder.DropColumn(
                name: "NamaPelapor",
                table: "Tikets");

            migrationBuilder.DropColumn(
                name: "NomorHpPelapor",
                table: "Tikets");

            migrationBuilder.DropColumn(
                name: "NomorTiketPublik",
                table: "Tikets");
        }
    }
}
