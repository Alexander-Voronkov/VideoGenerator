using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VideoGenerator.Migrations
{
    /// <inheritdoc />
    public partial class RemoveInterestingFactsQueueItemEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InterestingFactQueueItem");

            migrationBuilder.AddColumn<List<string>>(
                name: "Keywords",
                table: "GenerationQueue",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "GenerationQueue",
                type: "integer",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Keywords",
                table: "GenerationQueue");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "GenerationQueue");

            migrationBuilder.CreateTable(
                name: "InterestingFactQueueItem",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Tags = table.Column<List<string>>(type: "text[]", nullable: true),
                    Text = table.Column<string>(type: "text", nullable: true),
                    Title = table.Column<string>(type: "text", nullable: true),
                    YoutubePlaylistId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterestingFactQueueItem", x => x.Id);
                });
        }
    }
}
