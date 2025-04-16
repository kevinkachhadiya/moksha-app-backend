using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MAPI.Migrations
{
    /// <inheritdoc />
    public partial class addisactive1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "B_BillItem");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "B_Bill",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "B_Bill");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "B_BillItem",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
