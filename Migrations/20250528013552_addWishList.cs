using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace apisteamstats.Migrations
{
    /// <inheritdoc />
    public partial class addWishList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_wishLists",
                table: "wishLists");

            migrationBuilder.RenameTable(
                name: "wishLists",
                newName: "wishlists");

            migrationBuilder.AddPrimaryKey(
                name: "PK_wishlists",
                table: "wishlists",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_wishlists",
                table: "wishlists");

            migrationBuilder.RenameTable(
                name: "wishlists",
                newName: "wishLists");

            migrationBuilder.AddPrimaryKey(
                name: "PK_wishLists",
                table: "wishLists",
                column: "Id");
        }
    }
}
