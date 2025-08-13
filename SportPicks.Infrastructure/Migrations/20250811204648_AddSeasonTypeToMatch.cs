using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportPicks.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSeasonTypeToMatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SeasonType",
                table: "Matches",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SeasonTypeSlug",
                table: "Matches",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SeasonType",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "SeasonTypeSlug",
                table: "Matches");
        }
    }
}
