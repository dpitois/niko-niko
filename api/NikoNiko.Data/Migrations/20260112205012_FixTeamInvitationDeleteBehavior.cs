using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NikoNiko.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixTeamInvitationDeleteBehavior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TeamInvitations_Users_AcceptedByUserId",
                table: "TeamInvitations");

            migrationBuilder.AddForeignKey(
                name: "FK_TeamInvitations_Users_AcceptedByUserId",
                table: "TeamInvitations",
                column: "AcceptedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TeamInvitations_Users_AcceptedByUserId",
                table: "TeamInvitations");

            migrationBuilder.AddForeignKey(
                name: "FK_TeamInvitations_Users_AcceptedByUserId",
                table: "TeamInvitations",
                column: "AcceptedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
