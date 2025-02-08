using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MAPI.Migrations
{
    /// <inheritdoc />
    public partial class sbill : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "S_Bills",
                columns: table => new
                {
                    SId = table.Column<int>(name: "S_Id", type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SBillNo = table.Column<string>(name: "S_BillNo", type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SellerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TotalSPrice = table.Column<decimal>(name: "Total_S_Price", type: "decimal(10,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaymentMethod = table.Column<int>(type: "int", nullable: false),
                    IsPaid = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_S_Bills", x => x.SId);
                });

            migrationBuilder.CreateTable(
                name: "S_BillItems",
                columns: table => new
                {
                    siId = table.Column<int>(name: "si_Id", type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StockId = table.Column<int>(type: "int", nullable: false),
                    StBags = table.Column<int>(name: "St_Bags", type: "int", nullable: false),
                    StWeight = table.Column<decimal>(name: "St_Weight", type: "decimal(18,2)", nullable: false),
                    price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SBillSId = table.Column<int>(name: "S_BillS_Id", type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_S_BillItems", x => x.siId);
                    table.ForeignKey(
                        name: "FK_S_BillItems_S_Bills_S_BillS_Id",
                        column: x => x.SBillSId,
                        principalTable: "S_Bills",
                        principalColumn: "S_Id");
                    table.ForeignKey(
                        name: "FK_S_BillItems_Stocks_StockId",
                        column: x => x.StockId,
                        principalTable: "Stocks",
                        principalColumn: "StockId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_S_BillItems_S_BillS_Id",
                table: "S_BillItems",
                column: "S_BillS_Id");

            migrationBuilder.CreateIndex(
                name: "IX_S_BillItems_StockId",
                table: "S_BillItems",
                column: "StockId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "S_BillItems");

            migrationBuilder.DropTable(
                name: "S_Bills");
        }
    }
}
