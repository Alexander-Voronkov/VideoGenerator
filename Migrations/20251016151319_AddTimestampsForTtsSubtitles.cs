using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VideoGenerator.Migrations
{
    /// <inheritdoc />
    public partial class AddTimestampsForTtsSubtitles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Timestamps",
                table: "GeneratedSubtitle",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Timestamps",
                table: "GeneratedSubtitle");
        }
    }
}
