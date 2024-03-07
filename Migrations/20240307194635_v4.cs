using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VideoGenerator.Migrations
{
    /// <inheritdoc />
    public partial class v4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MessageAttachments_PublishedMessages_MessageId",
                table: "MessageAttachments");

            migrationBuilder.DropForeignKey(
                name: "FK_MessageAttachments_QueueMessages_MessageId",
                table: "MessageAttachments");

            migrationBuilder.DropForeignKey(
                name: "FK_Topics_Languages_LanguageId",
                table: "Topics");

            migrationBuilder.DropIndex(
                name: "IX_Topics_LanguageId",
                table: "Topics");

            migrationBuilder.DropIndex(
                name: "UI_TopicName_TopicId",
                table: "Topics");

            migrationBuilder.DropColumn(
                name: "LanguageId",
                table: "Topics");

            migrationBuilder.RenameColumn(
                name: "MessageId",
                table: "MessageAttachments",
                newName: "QueueMessageId");

            migrationBuilder.RenameIndex(
                name: "IX_MessageAttachments_MessageId",
                table: "MessageAttachments",
                newName: "IX_MessageAttachments_QueueMessageId");

            migrationBuilder.AddColumn<long>(
                name: "PublishedMessageId",
                table: "QueueMessages",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "MessageID",
                table: "PublishedMessages",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "TopicId",
                table: "Languages",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "LanguageId",
                table: "Groups",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "AttachmentPublishedMessage",
                columns: table => new
                {
                    AttachmentsAttachmentId = table.Column<long>(type: "INTEGER", nullable: false),
                    PublishedMessagesPublishedMessageId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttachmentPublishedMessage", x => new { x.AttachmentsAttachmentId, x.PublishedMessagesPublishedMessageId });
                    table.ForeignKey(
                        name: "FK_AttachmentPublishedMessage_MessageAttachments_AttachmentsAttachmentId",
                        column: x => x.AttachmentsAttachmentId,
                        principalTable: "MessageAttachments",
                        principalColumn: "AttachmentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AttachmentPublishedMessage_PublishedMessages_PublishedMessagesPublishedMessageId",
                        column: x => x.PublishedMessagesPublishedMessageId,
                        principalTable: "PublishedMessages",
                        principalColumn: "PublishedMessageId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "UI_TopicName_TopicId",
                table: "Topics",
                columns: new[] { "TopicName", "TopicId" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_QueueMessages_PublishedMessageId",
                table: "QueueMessages",
                column: "PublishedMessageId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Languages_TopicId",
                table: "Languages",
                column: "TopicId");

            migrationBuilder.CreateIndex(
                name: "IX_Groups_LanguageId",
                table: "Groups",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_AttachmentPublishedMessage_PublishedMessagesPublishedMessageId",
                table: "AttachmentPublishedMessage",
                column: "PublishedMessagesPublishedMessageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_Languages_LanguageId",
                table: "Groups",
                column: "LanguageId",
                principalTable: "Languages",
                principalColumn: "LanguageId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Languages_Topics_TopicId",
                table: "Languages",
                column: "TopicId",
                principalTable: "Topics",
                principalColumn: "TopicId");

            migrationBuilder.AddForeignKey(
                name: "FK_MessageAttachments_QueueMessages_QueueMessageId",
                table: "MessageAttachments",
                column: "QueueMessageId",
                principalTable: "QueueMessages",
                principalColumn: "QueueMessageId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QueueMessages_PublishedMessages_PublishedMessageId",
                table: "QueueMessages",
                column: "PublishedMessageId",
                principalTable: "PublishedMessages",
                principalColumn: "PublishedMessageId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Groups_Languages_LanguageId",
                table: "Groups");

            migrationBuilder.DropForeignKey(
                name: "FK_Languages_Topics_TopicId",
                table: "Languages");

            migrationBuilder.DropForeignKey(
                name: "FK_MessageAttachments_QueueMessages_QueueMessageId",
                table: "MessageAttachments");

            migrationBuilder.DropForeignKey(
                name: "FK_QueueMessages_PublishedMessages_PublishedMessageId",
                table: "QueueMessages");

            migrationBuilder.DropTable(
                name: "AttachmentPublishedMessage");

            migrationBuilder.DropIndex(
                name: "UI_TopicName_TopicId",
                table: "Topics");

            migrationBuilder.DropIndex(
                name: "IX_QueueMessages_PublishedMessageId",
                table: "QueueMessages");

            migrationBuilder.DropIndex(
                name: "IX_Languages_TopicId",
                table: "Languages");

            migrationBuilder.DropIndex(
                name: "IX_Groups_LanguageId",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "PublishedMessageId",
                table: "QueueMessages");

            migrationBuilder.DropColumn(
                name: "MessageID",
                table: "PublishedMessages");

            migrationBuilder.DropColumn(
                name: "TopicId",
                table: "Languages");

            migrationBuilder.DropColumn(
                name: "LanguageId",
                table: "Groups");

            migrationBuilder.RenameColumn(
                name: "QueueMessageId",
                table: "MessageAttachments",
                newName: "MessageId");

            migrationBuilder.RenameIndex(
                name: "IX_MessageAttachments_QueueMessageId",
                table: "MessageAttachments",
                newName: "IX_MessageAttachments_MessageId");

            migrationBuilder.AddColumn<int>(
                name: "LanguageId",
                table: "Topics",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Topics_LanguageId",
                table: "Topics",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "UI_TopicName_TopicId",
                table: "Topics",
                columns: new[] { "TopicName", "TopicId", "LanguageId" },
                descending: new bool[0]);

            migrationBuilder.AddForeignKey(
                name: "FK_MessageAttachments_PublishedMessages_MessageId",
                table: "MessageAttachments",
                column: "MessageId",
                principalTable: "PublishedMessages",
                principalColumn: "PublishedMessageId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MessageAttachments_QueueMessages_MessageId",
                table: "MessageAttachments",
                column: "MessageId",
                principalTable: "QueueMessages",
                principalColumn: "QueueMessageId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Topics_Languages_LanguageId",
                table: "Topics",
                column: "LanguageId",
                principalTable: "Languages",
                principalColumn: "LanguageId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
