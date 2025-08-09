using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VideoGenerator.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GeneratedHistories",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    RawMetadata = table.Column<string>(type: "text", nullable: true),
                    ClearMetadata = table.Column<string>(type: "text", nullable: true),
                    GenerationDuration = table.Column<TimeSpan>(type: "interval", nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GeneratedSubtitleId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneratedHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SourceVideos",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Path = table.Column<string>(type: "text", nullable: true),
                    Duration = table.Column<TimeSpan>(type: "interval", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SourceVideos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GeneratedSubtitle",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    BlobPath = table.Column<string>(type: "text", nullable: true),
                    GenerationDuration = table.Column<TimeSpan>(type: "interval", nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GeneratedHistoryId = table.Column<string>(type: "text", nullable: true),
                    ProcessedVideoId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneratedSubtitle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GeneratedSubtitle_GeneratedHistories_GeneratedHistoryId",
                        column: x => x.GeneratedHistoryId,
                        principalTable: "GeneratedHistories",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProcessedVideos",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Duration = table.Column<TimeSpan>(type: "interval", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessingDuration = table.Column<TimeSpan>(type: "interval", nullable: false),
                    SourceVideoId = table.Column<string>(type: "text", nullable: true),
                    GeneratedSubtitleId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessedVideos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcessedVideos_GeneratedSubtitle_GeneratedSubtitleId",
                        column: x => x.GeneratedSubtitleId,
                        principalTable: "GeneratedSubtitle",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProcessedVideos_SourceVideos_SourceVideoId",
                        column: x => x.SourceVideoId,
                        principalTable: "SourceVideos",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedSubtitle_GeneratedHistoryId",
                table: "GeneratedSubtitle",
                column: "GeneratedHistoryId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProcessedVideos_GeneratedSubtitleId",
                table: "ProcessedVideos",
                column: "GeneratedSubtitleId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProcessedVideos_SourceVideoId",
                table: "ProcessedVideos",
                column: "SourceVideoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProcessedVideos");

            migrationBuilder.DropTable(
                name: "GeneratedSubtitle");

            migrationBuilder.DropTable(
                name: "SourceVideos");

            migrationBuilder.DropTable(
                name: "GeneratedHistories");
        }
    }
}
