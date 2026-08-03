using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VideoGenerator.Migrations
{
    /// <inheritdoc />
    public partial class AddUploadStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UploadingStatus",
                table: "GeneratedVideos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "VideoUploadHistories",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    GeneratedVideoId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Metadata = table.Column<string>(type: "text", nullable: true),
                    UploadType = table.Column<int>(type: "integer", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoUploadHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VideoUploadHistories_GeneratedVideos_GeneratedVideoId",
                        column: x => x.GeneratedVideoId,
                        principalTable: "GeneratedVideos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VideoUploadHistories_GeneratedVideoId_UploadType",
                table: "VideoUploadHistories",
                columns: new[] { "GeneratedVideoId", "UploadType" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VideoUploadHistories");

            migrationBuilder.DropColumn(
                name: "UploadingStatus",
                table: "GeneratedVideos");
        }
    }
}
