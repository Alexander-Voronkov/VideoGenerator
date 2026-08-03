using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VideoGenerator.Migrations
{
    /// <inheritdoc />
    public partial class AddPartsSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PartNumber",
                table: "GeneratedVideos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PlaylistId",
                table: "GeneratedVideos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalParts",
                table: "GeneratedVideos",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PartNumber",
                table: "GeneratedVideos");

            migrationBuilder.DropColumn(
                name: "PlaylistId",
                table: "GeneratedVideos");

            migrationBuilder.DropColumn(
                name: "TotalParts",
                table: "GeneratedVideos");
        }
    }
}
