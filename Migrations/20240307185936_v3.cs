using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VideoGenerator.Migrations
{
    /// <inheritdoc />
    public partial class v3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Message_Text",
                table: "PublishedMessages");

            migrationBuilder.RenameIndex(
                name: "IX_MessageAttachments_AttachmentId_MessageId",
                table: "MessageAttachments",
                newName: "UI_AttachmentId_MessageId");

            migrationBuilder.RenameIndex(
                name: "IX_Languages_LanguageId_LanguageCode",
                table: "Languages",
                newName: "UI_LanguageId_LanguageCode");

            migrationBuilder.AlterColumn<long>(
                name: "TopicId",
                table: "Topics",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "INTEGER")
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddColumn<bool>(
                name: "IsTarget",
                table: "Groups",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Published_Message_Text",
                table: "PublishedMessages",
                sql: "LENGTH(Text) > 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Published_Message_Text",
                table: "PublishedMessages");

            migrationBuilder.DropColumn(
                name: "IsTarget",
                table: "Groups");

            migrationBuilder.RenameIndex(
                name: "UI_AttachmentId_MessageId",
                table: "MessageAttachments",
                newName: "IX_MessageAttachments_AttachmentId_MessageId");

            migrationBuilder.RenameIndex(
                name: "UI_LanguageId_LanguageCode",
                table: "Languages",
                newName: "IX_Languages_LanguageId_LanguageCode");

            migrationBuilder.AlterColumn<long>(
                name: "TopicId",
                table: "Topics",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "INTEGER")
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Message_Text",
                table: "PublishedMessages",
                sql: "LENGTH(Text) > 1");
        }
    }
}
