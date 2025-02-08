using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MAPI.Migrations
{
    /// <inheritdoc />
    public partial class FixSBillItemsSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "si_Id",
                table: "S_BillItems",
                newName: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ID",
                table: "S_BillItems",
                newName: "si_Id");
        }
    }
}
