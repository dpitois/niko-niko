using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NikoNiko.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMoodScaleToFive : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Add temporary int column
            migrationBuilder.AddColumn<int>(
                name: "MoodTemp",
                table: "MoodEntries",
                type: "INTEGER",
                nullable: true);

            // 2. Migrate Data (String -> Int + Shift)
            // Sad (old 0) -> 1 (new Sad)
            migrationBuilder.Sql("UPDATE \"MoodEntries\" SET \"MoodTemp\" = 1 WHERE \"Mood\" = 'Sad'");
            // Neutral (old 1) -> 2 (new Neutral)
            migrationBuilder.Sql("UPDATE \"MoodEntries\" SET \"MoodTemp\" = 2 WHERE \"Mood\" = 'Neutral'");
            // Happy (old 2) -> 3 (new Happy)
            migrationBuilder.Sql("UPDATE \"MoodEntries\" SET \"MoodTemp\" = 3 WHERE \"Mood\" = 'Happy'");

            // Fallback for safety (e.g. if any record was somehow invalid)
            migrationBuilder.Sql("UPDATE \"MoodEntries\" SET \"MoodTemp\" = 2 WHERE \"MoodTemp\" IS NULL");

            // 3. Drop old column
            migrationBuilder.DropColumn(
                name: "Mood",
                table: "MoodEntries");

            // 4. Rename Temp to Mood
            migrationBuilder.RenameColumn(
                name: "MoodTemp",
                table: "MoodEntries",
                newName: "Mood");

            // 5. Ensure the new column is NOT NULL (AlterColumn to enforce non-nullable)
            migrationBuilder.AlterColumn<int>(
                name: "Mood",
                table: "MoodEntries",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 1. Add temporary string column
            migrationBuilder.AddColumn<string>(
                name: "MoodTemp",
                table: "MoodEntries",
                type: "TEXT",
                nullable: true);

            // 2. Revert Data (Int -> String - Shift)
            migrationBuilder.Sql("UPDATE \"MoodEntries\" SET \"MoodTemp\" = 'Sad' WHERE \"Mood\" = 1");
            migrationBuilder.Sql("UPDATE \"MoodEntries\" SET \"MoodTemp\" = 'Neutral' WHERE \"Mood\" = 2");
            migrationBuilder.Sql("UPDATE \"MoodEntries\" SET \"MoodTemp\" = 'Happy' WHERE \"Mood\" = 3");

            // Map extremes back to nearest if they were used
            migrationBuilder.Sql("UPDATE \"MoodEntries\" SET \"MoodTemp\" = 'Sad' WHERE \"Mood\" = 0");
            migrationBuilder.Sql("UPDATE \"MoodEntries\" SET \"MoodTemp\" = 'Happy' WHERE \"Mood\" = 4");

            // 3. Drop old column
            migrationBuilder.DropColumn(
                name: "Mood",
                table: "MoodEntries");

            // 4. Rename Temp to Mood
            migrationBuilder.RenameColumn(
                name: "MoodTemp",
                table: "MoodEntries",
                newName: "Mood");
        }
    }
}