using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VideoGenerator.Migrations;

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
                LanguageId = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                LanguageCode = table.Column<string>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_LanguageId", x => x.LanguageId);
            });

        migrationBuilder.CreateTable(
            name: "Topics",
            columns: table => new
            {
                TopicId = table.Column<long>(type: "INTEGER", nullable: false),
                TopicName = table.Column<string>(type: "TEXT", nullable: true),
                LanguageId = table.Column<int>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TopicId", x => x.TopicId);
                table.ForeignKey(
                    name: "FK_Topics_Languages_LanguageId",
                    column: x => x.LanguageId,
                    principalTable: "Languages",
                    principalColumn: "LanguageId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Groups",
            columns: table => new
            {
                GroupId = table.Column<long>(type: "INTEGER", nullable: false),
                GroupLink = table.Column<string>(type: "TEXT", nullable: true),
                GroupName = table.Column<string>(type: "TEXT", nullable: true),
                TopicId = table.Column<long>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_GroupId", x => x.GroupId);
                table.ForeignKey(
                    name: "FK_Groups_Topics_TopicId",
                    column: x => x.TopicId,
                    principalTable: "Topics",
                    principalColumn: "TopicId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "PublishedMessages",
            columns: table => new
            {
                PublishedMessageId = table.Column<long>(type: "INTEGER", nullable: false),
                SourceGroupId = table.Column<long>(type: "INTEGER", nullable: false),
                TargetGroupId = table.Column<long>(type: "INTEGER", nullable: false),
                Text = table.Column<string>(type: "TEXT", nullable: false),
                StolenAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                PublishedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PublishedMessageId", x => x.PublishedMessageId);
                table.CheckConstraint("CK_Message_Text", "LENGTH(Text) > 1");
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
                QueueMessageId = table.Column<long>(type: "INTEGER", nullable: false),
                SourceGroupId = table.Column<long>(type: "INTEGER", nullable: false),
                TargetGroupId = table.Column<long>(type: "INTEGER", nullable: false),
                Text = table.Column<string>(type: "TEXT", nullable: false),
                StolenAt = table.Column<DateTime>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_QueueMessageId", x => x.QueueMessageId);
                table.CheckConstraint("CK_Message_Text", "LENGTH(Text) > 1");
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
            });

        migrationBuilder.CreateTable(
            name: "MessageAttachments",
            columns: table => new
            {
                AttachmentId = table.Column<long>(type: "INTEGER", nullable: false),
                MessageId = table.Column<long>(type: "INTEGER", nullable: false),
                MimeType = table.Column<string>(type: "TEXT", nullable: true),
                Content = table.Column<byte[]>(type: "BLOB", nullable: true),
                Type = table.Column<int>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AttachmentId", x => x.AttachmentId);
                table.ForeignKey(
                    name: "FK_MessageAttachments_PublishedMessages_MessageId",
                    column: x => x.MessageId,
                    principalTable: "PublishedMessages",
                    principalColumn: "PublishedMessageId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_MessageAttachments_QueueMessages_MessageId",
                    column: x => x.MessageId,
                    principalTable: "QueueMessages",
                    principalColumn: "QueueMessageId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "UI_TopicId_GroupId",
            table: "Groups",
            columns: new[] { "TopicId", "GroupId" },
            unique: true,
            descending: new bool[0]);

        migrationBuilder.CreateIndex(
            name: "IX_Languages_LanguageId_LanguageCode",
            table: "Languages",
            columns: new[] { "LanguageId", "LanguageCode" },
            unique: true,
            descending: new bool[0]);

        migrationBuilder.CreateIndex(
            name: "IX_MessageAttachments_AttachmentId_MessageId",
            table: "MessageAttachments",
            columns: new[] { "AttachmentId", "MessageId" },
            unique: true,
            descending: new bool[0]);

        migrationBuilder.CreateIndex(
            name: "IX_MessageAttachments_MessageId",
            table: "MessageAttachments",
            column: "MessageId");

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
            unique: true,
            descending: new bool[0]);

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
            unique: true,
            descending: new bool[0]);

        migrationBuilder.CreateIndex(
            name: "IX_Topics_LanguageId",
            table: "Topics",
            column: "LanguageId");

        migrationBuilder.CreateIndex(
            name: "UI_TopicName_TopicId",
            table: "Topics",
            columns: new[] { "TopicName", "TopicId", "LanguageId" },
            unique: true,
            descending: new bool[0]);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "MessageAttachments");

        migrationBuilder.DropTable(
            name: "PublishedMessages");

        migrationBuilder.DropTable(
            name: "QueueMessages");

        migrationBuilder.DropTable(
            name: "Groups");

        migrationBuilder.DropTable(
            name: "Topics");

        migrationBuilder.DropTable(
            name: "Languages");
    }
}
