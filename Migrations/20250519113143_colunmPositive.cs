using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace apisteamstats.Migrations
{
    /// <inheritdoc />
    public partial class colunmPositive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "averagePlayTime",
                table: "games",
                newName: "positive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "positive",
                table: "games",
                newName: "averagePlayTime");
        }
    }
}
