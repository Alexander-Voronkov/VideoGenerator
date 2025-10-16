using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VideoGenerator.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GeneratedSubtitle",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    TtsBlobPath = table.Column<string>(type: "text", nullable: true),
                    AssBlobPath = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneratedSubtitle", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GenerationQueue",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Text = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Tags = table.Column<List<string>>(type: "text[]", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GenerationQueue", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SplitHistories",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    ParentBlobPath = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BlobPath = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Duration = table.Column<TimeSpan>(type: "interval", nullable: false),
                    LastTookPartAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SplitHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GeneratedVideos",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    BlobPath = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    GenerationQueueId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneratedVideos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GeneratedVideos_GenerationQueue_GenerationQueueId",
                        column: x => x.GenerationQueueId,
                        principalTable: "GenerationQueue",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedVideos_GenerationQueueId",
                table: "GeneratedVideos",
                column: "GenerationQueueId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GeneratedSubtitle");

            migrationBuilder.DropTable(
                name: "GeneratedVideos");

            migrationBuilder.DropTable(
                name: "SplitHistories");

            migrationBuilder.DropTable(
                name: "GenerationQueue");
        }
    }
}
