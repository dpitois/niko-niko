using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NikoNiko.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSprintNameTemplateToTeam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SprintNameTemplate",
                table: "Teams",
                type: "TEXT",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SprintNameTemplate",
                table: "Teams");
        }
    }
}
