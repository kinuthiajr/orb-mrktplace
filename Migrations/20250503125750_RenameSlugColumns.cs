using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Orb.API.Migrations
{
    /// <inheritdoc />
    public partial class RenameSlugColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Slug",
                table: "Shops",
                newName: "ShopSlug");

            migrationBuilder.RenameColumn(
                name: "Slug",
                table: "Products",
                newName: "ProductSlug");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ShopSlug",
                table: "Shops",
                newName: "Slug");

            migrationBuilder.RenameColumn(
                name: "ProductSlug",
                table: "Products",
                newName: "Slug");
        }
    }
}
