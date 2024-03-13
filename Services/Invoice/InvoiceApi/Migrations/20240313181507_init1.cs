using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvoiceApi.Migrations
{
    /// <inheritdoc />
    public partial class init1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SellerId",
                table: "Invoices",
                newName: "SellerCompanyId");

            migrationBuilder.RenameColumn(
                name: "BuyerId",
                table: "Invoices",
                newName: "OwnerId");

            migrationBuilder.AddColumn<Guid>(
                name: "BuyerCompanyId",
                table: "Invoices",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BuyerCompanyId",
                table: "Invoices");

            migrationBuilder.RenameColumn(
                name: "SellerCompanyId",
                table: "Invoices",
                newName: "SellerId");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
                table: "Invoices",
                newName: "BuyerId");
        }
    }
}
