using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace apisteamstats.Migrations
{
    /// <inheritdoc />
    public partial class colunmPlayTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "averagePlayTime",
                table: "games",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "averagePlayTime",
                table: "games");
        }
    }
}
