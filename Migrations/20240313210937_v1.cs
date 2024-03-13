using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace VideoGenerator.Migrations
{
    /// <inheritdoc />
    public partial class v1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Languages",
                columns: table => new
                {
                    LanguageId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LanguageCode = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LanguageId", x => x.LanguageId);
                });

            migrationBuilder.CreateTable(
                name: "Topics",
                columns: table => new
                {
                    TopicId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TopicName = table.Column<string>(type: "text", nullable: true),
                    IsAvailable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TopicId", x => x.TopicId);
                });

            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    GroupId = table.Column<long>(type: "bigint", nullable: false),
                    GroupLink = table.Column<string>(type: "text", nullable: true),
                    GroupName = table.Column<string>(type: "text", nullable: true),
                    TopicId = table.Column<int>(type: "integer", nullable: false),
                    LanguageId = table.Column<int>(type: "integer", nullable: false),
                    IsTarget = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupId", x => x.GroupId);
                    table.ForeignKey(
                        name: "FK_Groups_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "LanguageId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Groups_Topics_TopicId",
                        column: x => x.TopicId,
                        principalTable: "Topics",
                        principalColumn: "TopicId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LanguageTopic",
                columns: table => new
                {
                    AvailableLanguagesLanguageId = table.Column<int>(type: "integer", nullable: false),
                    TopicsTopicId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LanguageTopic", x => new { x.AvailableLanguagesLanguageId, x.TopicsTopicId });
                    table.ForeignKey(
                        name: "FK_LanguageTopic_Languages_AvailableLanguagesLanguageId",
                        column: x => x.AvailableLanguagesLanguageId,
                        principalTable: "Languages",
                        principalColumn: "LanguageId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LanguageTopic_Topics_TopicsTopicId",
                        column: x => x.TopicsTopicId,
                        principalTable: "Topics",
                        principalColumn: "TopicId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PublishedMessages",
                columns: table => new
                {
                    PublishedMessageId = table.Column<long>(type: "bigint", nullable: false),
                    SourceMessageId = table.Column<long>(type: "bigint", nullable: false),
                    SourceGroupId = table.Column<long>(type: "bigint", nullable: false),
                    TargetGroupId = table.Column<long>(type: "bigint", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false),
                    StolenAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublishedMessageId", x => x.PublishedMessageId);
                    table.ForeignKey(
                        name: "FK_PublishedMessages_Groups_SourceGroupId",
                        column: x => x.SourceGroupId,
                        principalTable: "Groups",
                        principalColumn: "GroupId");
                    table.ForeignKey(
                        name: "FK_PublishedMessages_Groups_TargetGroupId",
                        column: x => x.TargetGroupId,
                        principalTable: "Groups",
                        principalColumn: "GroupId");
                });

            migrationBuilder.CreateTable(
                name: "QueueMessages",
                columns: table => new
                {
                    QueueMessageId = table.Column<long>(type: "bigint", nullable: false),
                    SourceGroupId = table.Column<long>(type: "bigint", nullable: false),
                    TargetGroupId = table.Column<long>(type: "bigint", nullable: false),
                    PublishedMessageId = table.Column<long>(type: "bigint", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false),
                    StolenAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QueueMessageId", x => x.QueueMessageId);
                    table.ForeignKey(
                        name: "FK_QueueMessages_Groups_SourceGroupId",
                        column: x => x.SourceGroupId,
                        principalTable: "Groups",
                        principalColumn: "GroupId");
                    table.ForeignKey(
                        name: "FK_QueueMessages_Groups_TargetGroupId",
                        column: x => x.TargetGroupId,
                        principalTable: "Groups",
                        principalColumn: "GroupId");
                    table.ForeignKey(
                        name: "FK_QueueMessages_PublishedMessages_PublishedMessageId",
                        column: x => x.PublishedMessageId,
                        principalTable: "PublishedMessages",
                        principalColumn: "PublishedMessageId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MessageAttachments",
                columns: table => new
                {
                    AttachmentId = table.Column<long>(type: "bigint", nullable: false),
                    QueueMessageId = table.Column<long>(type: "bigint", nullable: false),
                    MimeType = table.Column<string>(type: "text", nullable: true),
                    Content = table.Column<byte[]>(type: "bytea", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttachmentId", x => x.AttachmentId);
                    table.ForeignKey(
                        name: "FK_MessageAttachments_QueueMessages_QueueMessageId",
                        column: x => x.QueueMessageId,
                        principalTable: "QueueMessages",
                        principalColumn: "QueueMessageId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AttachmentPublishedMessage",
                columns: table => new
                {
                    AttachmentsAttachmentId = table.Column<long>(type: "bigint", nullable: false),
                    PublishedMessagesPublishedMessageId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttachmentPublishedMessage", x => new { x.AttachmentsAttachmentId, x.PublishedMessagesPublishedMessageId });
                    table.ForeignKey(
                        name: "FK_AttachmentPublishedMessage_MessageAttachments_AttachmentsAt~",
                        column: x => x.AttachmentsAttachmentId,
                        principalTable: "MessageAttachments",
                        principalColumn: "AttachmentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AttachmentPublishedMessage_PublishedMessages_PublishedMessa~",
                        column: x => x.PublishedMessagesPublishedMessageId,
                        principalTable: "PublishedMessages",
                        principalColumn: "PublishedMessageId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AttachmentPublishedMessage_PublishedMessagesPublishedMessag~",
                table: "AttachmentPublishedMessage",
                column: "PublishedMessagesPublishedMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Groups_LanguageId",
                table: "Groups",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "UI_TopicId_GroupId",
                table: "Groups",
                columns: new[] { "TopicId", "GroupId" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "UI_LanguageId_LanguageCode",
                table: "Languages",
                columns: new[] { "LanguageId", "LanguageCode" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_LanguageTopic_TopicsTopicId",
                table: "LanguageTopic",
                column: "TopicsTopicId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageAttachments_QueueMessageId",
                table: "MessageAttachments",
                column: "QueueMessageId");

            migrationBuilder.CreateIndex(
                name: "UI_AttachmentId_MessageId",
                table: "MessageAttachments",
                columns: new[] { "AttachmentId", "QueueMessageId" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_PublishedMessages_SourceGroupId",
                table: "PublishedMessages",
                column: "SourceGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_PublishedMessages_TargetGroupId",
                table: "PublishedMessages",
                column: "TargetGroupId");

            migrationBuilder.CreateIndex(
                name: "UI_PublishedMessageId_GroupId",
                table: "PublishedMessages",
                columns: new[] { "PublishedMessageId", "TargetGroupId", "SourceGroupId" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_QueueMessages_PublishedMessageId",
                table: "QueueMessages",
                column: "PublishedMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_QueueMessages_SourceGroupId",
                table: "QueueMessages",
                column: "SourceGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_QueueMessages_TargetGroupId",
                table: "QueueMessages",
                column: "TargetGroupId");

            migrationBuilder.CreateIndex(
                name: "UI_QueueMessageId_GroupId",
                table: "QueueMessages",
                columns: new[] { "QueueMessageId", "TargetGroupId", "SourceGroupId" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "UI_TopicName_TopicId",
                table: "Topics",
                columns: new[] { "TopicName", "TopicId" },
                descending: new bool[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AttachmentPublishedMessage");

            migrationBuilder.DropTable(
                name: "LanguageTopic");

            migrationBuilder.DropTable(
                name: "MessageAttachments");

            migrationBuilder.DropTable(
                name: "QueueMessages");

            migrationBuilder.DropTable(
                name: "PublishedMessages");

            migrationBuilder.DropTable(
                name: "Groups");

            migrationBuilder.DropTable(
                name: "Languages");

            migrationBuilder.DropTable(
                name: "Topics");
        }
    }
}
