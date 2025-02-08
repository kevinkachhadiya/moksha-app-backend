using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MAPI.Migrations
{
    /// <inheritdoc />
    public partial class m3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Material_Id",
                table: "Stocks");

            migrationBuilder.RenameColumn(
                name: "TotalPrice",
                table: "Bills",
                newName: "TotalBillPrice");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TotalBillPrice",
                table: "Bills",
                newName: "TotalPrice");

            migrationBuilder.AddColumn<int>(
                name: "Material_Id",
                table: "Stocks",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
