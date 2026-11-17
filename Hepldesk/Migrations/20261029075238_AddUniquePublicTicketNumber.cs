using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hepldesk.Migrations
{
    /// <inheritdoc />
    public partial class AddUniquePublicTicketNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Tikets_NomorTiketPublik",
                table: "Tikets",
                column: "NomorTiketPublik",
                unique: true,
                filter: "[NomorTiketPublik] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tikets_NomorTiketPublik",
                table: "Tikets");
        }
    }
}
