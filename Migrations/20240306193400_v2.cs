using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VideoGenerator.Migrations;

/// <inheritdoc />
public partial class v2 : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "UI_TopicName_TopicId",
            table: "Topics");

        migrationBuilder.DropIndex(
            name: "UI_QueueMessageId_GroupId",
            table: "QueueMessages");

        migrationBuilder.DropIndex(
            name: "UI_PublishedMessageId_GroupId",
            table: "PublishedMessages");

        migrationBuilder.DropIndex(
            name: "IX_MessageAttachments_AttachmentId_MessageId",
            table: "MessageAttachments");

        migrationBuilder.DropIndex(
            name: "IX_Languages_LanguageId_LanguageCode",
            table: "Languages");

        migrationBuilder.DropIndex(
            name: "UI_TopicId_GroupId",
            table: "Groups");

        migrationBuilder.CreateIndex(
            name: "UI_TopicName_TopicId",
            table: "Topics",
            columns: new[] { "TopicName", "TopicId", "LanguageId" },
            descending: new bool[0]);

        migrationBuilder.CreateIndex(
            name: "UI_QueueMessageId_GroupId",
            table: "QueueMessages",
            columns: new[] { "QueueMessageId", "TargetGroupId", "SourceGroupId" },
            descending: new bool[0]);

        migrationBuilder.CreateIndex(
            name: "UI_PublishedMessageId_GroupId",
            table: "PublishedMessages",
            columns: new[] { "PublishedMessageId", "TargetGroupId", "SourceGroupId" },
            descending: new bool[0]);

        migrationBuilder.CreateIndex(
            name: "IX_MessageAttachments_AttachmentId_MessageId",
            table: "MessageAttachments",
            columns: new[] { "AttachmentId", "MessageId" },
            descending: new bool[0]);

        migrationBuilder.CreateIndex(
            name: "IX_Languages_LanguageId_LanguageCode",
            table: "Languages",
            columns: new[] { "LanguageId", "LanguageCode" },
            descending: new bool[0]);

        migrationBuilder.CreateIndex(
            name: "UI_TopicId_GroupId",
            table: "Groups",
            columns: new[] { "TopicId", "GroupId" },
            descending: new bool[0]);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "UI_TopicName_TopicId",
            table: "Topics");

        migrationBuilder.DropIndex(
            name: "UI_QueueMessageId_GroupId",
            table: "QueueMessages");

        migrationBuilder.DropIndex(
            name: "UI_PublishedMessageId_GroupId",
            table: "PublishedMessages");

        migrationBuilder.DropIndex(
            name: "IX_MessageAttachments_AttachmentId_MessageId",
            table: "MessageAttachments");

        migrationBuilder.DropIndex(
            name: "IX_Languages_LanguageId_LanguageCode",
            table: "Languages");

        migrationBuilder.DropIndex(
            name: "UI_TopicId_GroupId",
            table: "Groups");

        migrationBuilder.CreateIndex(
            name: "UI_TopicName_TopicId",
            table: "Topics",
            columns: new[] { "TopicName", "TopicId", "LanguageId" },
            unique: true,
            descending: new bool[0]);

        migrationBuilder.CreateIndex(
            name: "UI_QueueMessageId_GroupId",
            table: "QueueMessages",
            columns: new[] { "QueueMessageId", "TargetGroupId", "SourceGroupId" },
            unique: true,
            descending: new bool[0]);

        migrationBuilder.CreateIndex(
            name: "UI_PublishedMessageId_GroupId",
            table: "PublishedMessages",
            columns: new[] { "PublishedMessageId", "TargetGroupId", "SourceGroupId" },
            unique: true,
            descending: new bool[0]);

        migrationBuilder.CreateIndex(
            name: "IX_MessageAttachments_AttachmentId_MessageId",
            table: "MessageAttachments",
            columns: new[] { "AttachmentId", "MessageId" },
            unique: true,
            descending: new bool[0]);

        migrationBuilder.CreateIndex(
            name: "IX_Languages_LanguageId_LanguageCode",
            table: "Languages",
            columns: new[] { "LanguageId", "LanguageCode" },
            unique: true,
            descending: new bool[0]);

        migrationBuilder.CreateIndex(
            name: "UI_TopicId_GroupId",
            table: "Groups",
            columns: new[] { "TopicId", "GroupId" },
            unique: true,
            descending: new bool[0]);
    }
}
