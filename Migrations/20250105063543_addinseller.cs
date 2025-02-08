using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MAPI.Migrations
{
    /// <inheritdoc />
    public partial class addinseller : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "RemainAmount",
                table: "S_Bills",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "S_Bills",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RemainAmount",
                table: "S_Bills");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "S_Bills");
        }
    }
}
