using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hepldesk.Migrations
{
    /// <inheritdoc />
    public partial class SyncTicketModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateUpdate",
                table: "Tikets",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateUpdate",
                table: "Tikets");
        }
    }
}