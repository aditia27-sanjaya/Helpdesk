using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hepldesk.Migrations
{
    /// <inheritdoc />
    public partial class AddTiketStatusHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TiketStatusHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TiketId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TanggalPerubahan = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Keterangan = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiketStatusHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TiketStatusHistories_Tikets_TiketId",
                        column: x => x.TiketId,
                        principalTable: "Tikets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TiketStatusHistories_TiketId",
                table: "TiketStatusHistories",
                column: "TiketId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TiketStatusHistories");
        }
    }
}
