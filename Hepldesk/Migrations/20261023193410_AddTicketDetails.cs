using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hepldesk.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AssignedToUserId",
                table: "Tikets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "Tikets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateCreate",
                table: "Tikets",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()");

            migrationBuilder.AddColumn<string>(
                name: "Keterangan",
                table: "Tikets",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ScreenshotPath",
                table: "Tikets",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssignedToUserId",
                table: "Tikets");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Tikets");

            migrationBuilder.DropColumn(
                name: "DateCreate",
                table: "Tikets");

            migrationBuilder.DropColumn(
                name: "Keterangan",
                table: "Tikets");

            migrationBuilder.DropColumn(
                name: "ScreenshotPath",
                table: "Tikets");
        }
    }
}
