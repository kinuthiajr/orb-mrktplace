using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Orb.API.Migrations
{
    /// <inheritdoc />
    public partial class ChangeToOneShopPerSeller : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Shops_SellerId",
                table: "Shops");

            migrationBuilder.AddColumn<Guid>(
                name: "ShopId",
                table: "AspNetUsers",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shops_SellerId",
                table: "Shops",
                column: "SellerId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Shops_SellerId",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "ShopId",
                table: "AspNetUsers");

            migrationBuilder.CreateIndex(
                name: "IX_Shops_SellerId",
                table: "Shops",
                column: "SellerId");
        }
    }
}
