using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MAPI.Migrations
{
    /// <inheritdoc />
    public partial class bbill : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_B_BillItem_Bills_B_BillB_Id",
                table: "B_BillItem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Bills",
                table: "Bills");

            migrationBuilder.RenameTable(
                name: "Bills",
                newName: "B_Bill");

            migrationBuilder.AddPrimaryKey(
                name: "PK_B_Bill",
                table: "B_Bill",
                column: "B_Id");

            migrationBuilder.AddForeignKey(
                name: "FK_B_BillItem_B_Bill_B_BillB_Id",
                table: "B_BillItem",
                column: "B_BillB_Id",
                principalTable: "B_Bill",
                principalColumn: "B_Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_B_BillItem_B_Bill_B_BillB_Id",
                table: "B_BillItem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_B_Bill",
                table: "B_Bill");

            migrationBuilder.RenameTable(
                name: "B_Bill",
                newName: "Bills");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Bills",
                table: "Bills",
                column: "B_Id");

            migrationBuilder.AddForeignKey(
                name: "FK_B_BillItem_Bills_B_BillB_Id",
                table: "B_BillItem",
                column: "B_BillB_Id",
                principalTable: "Bills",
                principalColumn: "B_Id");
        }
    }
}
