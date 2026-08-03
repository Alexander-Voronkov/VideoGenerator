using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VideoGenerator.Migrations
{
    /// <inheritdoc />
    public partial class PublishBatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PublishAccounts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    ContentType = table.Column<int>(type: "integer", nullable: false),
                    LastPublishAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublishAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PublishBatches",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    GenerationQueueItemId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ApproveStatus = table.Column<int>(type: "integer", nullable: false),
                    ContentType = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsScheduled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublishBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PublishBatches_GenerationQueue_GenerationQueueItemId",
                        column: x => x.GenerationQueueItemId,
                        principalTable: "GenerationQueue",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PublishQueueItems",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PublishBatchId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PublishAccountId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PlaylistId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublishQueueItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PublishQueueItems_PublishAccounts_PublishAccountId",
                        column: x => x.PublishAccountId,
                        principalTable: "PublishAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PublishQueueItems_PublishBatches_PublishBatchId",
                        column: x => x.PublishBatchId,
                        principalTable: "PublishBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScheduledUploads",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PublishQueueItemId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    GeneratedVideoId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledUploads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduledUploads_GeneratedVideos_GeneratedVideoId",
                        column: x => x.GeneratedVideoId,
                        principalTable: "GeneratedVideos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScheduledUploads_PublishQueueItems_PublishQueueItemId",
                        column: x => x.PublishQueueItemId,
                        principalTable: "PublishQueueItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PublishBatches_GenerationQueueItemId",
                table: "PublishBatches",
                column: "GenerationQueueItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PublishQueueItems_PublishAccountId",
                table: "PublishQueueItems",
                column: "PublishAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PublishQueueItems_PublishBatchId_PublishAccountId",
                table: "PublishQueueItems",
                columns: new[] { "PublishBatchId", "PublishAccountId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledUploads_GeneratedVideoId",
                table: "ScheduledUploads",
                column: "GeneratedVideoId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledUploads_PublishQueueItemId_GeneratedVideoId",
                table: "ScheduledUploads",
                columns: new[] { "PublishQueueItemId", "GeneratedVideoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledUploads_ScheduledAt_Status",
                table: "ScheduledUploads",
                columns: new[] { "ScheduledAt", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScheduledUploads");

            migrationBuilder.DropTable(
                name: "PublishQueueItems");

            migrationBuilder.DropTable(
                name: "PublishAccounts");

            migrationBuilder.DropTable(
                name: "PublishBatches");
        }
    }
}
