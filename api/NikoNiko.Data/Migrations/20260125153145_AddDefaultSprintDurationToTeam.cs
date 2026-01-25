using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NikoNiko.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDefaultSprintDurationToTeam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DefaultSprintDuration",
                table: "Teams",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultSprintDuration",
                table: "Teams");
        }
    }
}