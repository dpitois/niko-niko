using System;

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NikoNiko.Data.Migrations
{
    /// <inheritdoc />
    public partial class GDPR_AccountDeletion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MoodEntries_Users_UserId",
                table: "MoodEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_TeamInvitations_Users_CreatorUserId",
                table: "TeamInvitations");

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatorUserId",
                table: "TeamInvitations",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "MoodEntries",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.CreateTable(
                name: "UserDeletionLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    HashedIdentity = table.Column<string>(type: "TEXT", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDeletionLogs", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_MoodEntries_Users_UserId",
                table: "MoodEntries",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_TeamInvitations_Users_CreatorUserId",
                table: "TeamInvitations",
                column: "CreatorUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MoodEntries_Users_UserId",
                table: "MoodEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_TeamInvitations_Users_CreatorUserId",
                table: "TeamInvitations");

            migrationBuilder.DropTable(
                name: "UserDeletionLogs");

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatorUserId",
                table: "TeamInvitations",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "MoodEntries",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MoodEntries_Users_UserId",
                table: "MoodEntries",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TeamInvitations_Users_CreatorUserId",
                table: "TeamInvitations",
                column: "CreatorUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}