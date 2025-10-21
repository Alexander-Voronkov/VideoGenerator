using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VideoGenerator.Migrations
{
    /// <inheritdoc />
    public partial class PlaylistId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlaylistId",
                table: "GeneratedVideos");

            migrationBuilder.AddColumn<string>(
                name: "YoutubePlaylistId",
                table: "GenerationQueue",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "YoutubePlaylistId",
                table: "GenerationQueue");

            migrationBuilder.AddColumn<string>(
                name: "PlaylistId",
                table: "GeneratedVideos",
                type: "text",
                nullable: true);
        }
    }
}
