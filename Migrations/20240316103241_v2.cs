using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VideoGenerator.Migrations
{
    /// <inheritdoc />
    public partial class v2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QueueMessages_Groups_TargetGroupId",
                table: "QueueMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_QueueMessages_PublishedMessages_PublishedMessageId",
                table: "QueueMessages");

            migrationBuilder.DropIndex(
                name: "IX_QueueMessages_PublishedMessageId",
                table: "QueueMessages");

            migrationBuilder.DropIndex(
                name: "IX_QueueMessages_TargetGroupId",
                table: "QueueMessages");

            migrationBuilder.DropIndex(
                name: "UI_QueueMessageId_GroupId",
                table: "QueueMessages");

            migrationBuilder.DropColumn(
                name: "TargetGroupId",
                table: "QueueMessages");

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "Languages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "UI_QueueMessageId_GroupId",
                table: "QueueMessages",
                columns: new[] { "QueueMessageId", "SourceGroupId" },
                descending: new bool[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UI_QueueMessageId_GroupId",
                table: "QueueMessages");

            migrationBuilder.DropColumn(
                name: "IsAvailable",
                table: "Languages");

            migrationBuilder.AddColumn<long>(
                name: "TargetGroupId",
                table: "QueueMessages",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_QueueMessages_PublishedMessageId",
                table: "QueueMessages",
                column: "PublishedMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_QueueMessages_TargetGroupId",
                table: "QueueMessages",
                column: "TargetGroupId");

            migrationBuilder.CreateIndex(
                name: "UI_QueueMessageId_GroupId",
                table: "QueueMessages",
                columns: new[] { "QueueMessageId", "TargetGroupId", "SourceGroupId" },
                descending: new bool[0]);

            migrationBuilder.AddForeignKey(
                name: "FK_QueueMessages_Groups_TargetGroupId",
                table: "QueueMessages",
                column: "TargetGroupId",
                principalTable: "Groups",
                principalColumn: "GroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_QueueMessages_PublishedMessages_PublishedMessageId",
                table: "QueueMessages",
                column: "PublishedMessageId",
                principalTable: "PublishedMessages",
                principalColumn: "PublishedMessageId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
