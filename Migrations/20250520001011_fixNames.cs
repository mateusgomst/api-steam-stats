using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace apisteamstats.Migrations
{
    /// <inheritdoc />
    public partial class fixNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "senha",
                table: "users",
                newName: "password");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "users",
                newName: "login");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "password",
                table: "users",
                newName: "senha");

            migrationBuilder.RenameColumn(
                name: "login",
                table: "users",
                newName: "email");
        }
    }
}
