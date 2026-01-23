# Implementation Plan - Update Mood Scale to 5 Levels

## 1. 🔍 Analysis & Context
*   **Objective:** Transition from a 3-level mood scale (Sad, Neutral, Happy) to a 5-level scale (Very Sad, Sad, Neutral, Happy, Very Happy) and migrate existing data to preserve semantics.
*   **Affected Files:**
    *   Backend: `api/NikoNiko.Core/Models/MoodType.cs`, `api/NikoNiko.Data/Migrations/`
    *   Frontend: `app/frontend/src/models/MoodType.ts`, `app/frontend/src/components/MoodEntryForm.tsx`, `app/frontend/src/i18n/locales/fr.json`, `app/frontend/src/i18n/locales/en.json`.
*   **Key Dependencies:** Entity Framework Core (Migration), MUI Components (Icons).
*   **Risques/Unknowns:**
    *   **Data Migration:** Existing values must be shifted (+1) so that the old "Sad" (0) becomes the new "Sad" (1), etc.
    *   **Visuals:** Dashboards will need to adapt their color palettes for 5 values.

## 2. 📋 Checklist
- [x] Step 1: Update Backend Model (Enum & DbContext)
- [x] Step 2: Create and Script EF Core Migration
- [x] Step 3: Update Frontend Models and Translations
- [x] Step 4: Update Entry Interface (DailyMoodWidget)
- [x] Step 5: Update Grid Display and Interaction (MoodGridDisplay)
- [x] Step 6: Update Trend Chart (MoodTrendChart & Utils)
- [x] Verification

## 3. 📝 Step-by-Step Implementation Details

### Step 6: Update Trend Chart (MoodTrendChart & Utils)
*   **Goal:** Adapt the trend chart to the 5-level scale for accurate visualization.
*   **Action:**
    *   Modify `app/frontend/src/utils/moodTrendUtils.ts`: Map 5 mood levels to scores 1-5 (instead of 1-3).
    *   Modify `app/frontend/src/components/dashboard/MoodTrendChart.tsx`:
        *   Update `getY` normalization to handle scale 1-5.
        *   Update Y-axis grid lines and emojis to display all 5 levels.
*   **Verification:** Chart displays data correctly across the full height and shows 5 emojis on the Y-axis.

### 🚀 Execution Rules
1. **Interactive Flow**: Execute ONLY ONE step at a time.
2. **Atomic Updates**: Before and after each step, you MUST use `replace` to update the checklist in section 2 and the status in section 3.
3. **Status Vocabulary**: Use `[x]` (Done), `[>]` (In Progress), `[-]` (Skipped/N.A.), `[!]` (Failed/Blocked).
4. **User Confirmation**: After updating the file, stop and wait for explicit user confirmation before proceeding to the next step.
5. **No Stealth Actions**: NEVER execute code or modify files without having first updated the plan to reflect that you are about to do so.

### Step 1: Update Backend Model (Enum & DbContext)
*   **Goal:** Define the new data structure (5 values) and switch storage to `int`.
*   **Action:**
    *   Modify `api/NikoNiko.Core/Models/MoodType.cs`:
        ```csharp
        public enum MoodType
        {
            VerySad = 0,
            Sad = 1,
            Neutral = 2,
            Happy = 3,
            VeryHappy = 4
        }
        ```
    *   Modify `api/NikoNiko.Data/ApplicationDbContext.cs`:
        *   **Remove** the lines configuring `MoodEntry.Mood` as string:
            ```csharp
            // REMOVE THIS BLOCK
            // modelBuilder.Entity<MoodEntry>()
            //    .Property(me => me.Mood)
            //    .HasConversion<string>();
            ```
*   **Verification:** Code compiles.

### Step 2: Create and Script EF Core Migration
*   **Goal:** Update database schema (String -> Int) and migrate data.
*   **Action:**
    *   Run `dotnet ef migrations add UpdateMoodScaleToFive --project api/NikoNiko.Data --startup-project api/NikoNiko.Api`.
    *   **CRITICAL:** Modify the generated migration file `Up()` method to handle the Type Change and Data Migration safely.
    *   *Strategy:* Use a temporary column to ensure data safety across SQLite/Postgres.
        ```csharp
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Add temporary int column
            migrationBuilder.AddColumn<int>(
                name: "MoodTemp",
                table: "MoodEntries",
                type: "INTEGER", // Or "integer" depending on provider, let EF handle type if possible or use generic add
                nullable: true); // Temporarily nullable

            // 2. Migrate Data (String -> Int + Shift)
            // Sad (old 0) -> 1 (new Sad)
            migrationBuilder.Sql("UPDATE \"MoodEntries\" SET \"MoodTemp\" = 1 WHERE \"Mood\" = 'Sad'");
            // Neutral (old 1) -> 2 (new Neutral)
            migrationBuilder.Sql("UPDATE \"MoodEntries\" SET \"MoodTemp\" = 2 WHERE \"Mood\" = 'Neutral'");
            // Happy (old 2) -> 3 (new Happy)
            migrationBuilder.Sql("UPDATE \"MoodEntries\" SET \"MoodTemp\" = 3 WHERE \"Mood\" = 'Happy'");
            
            // Fallback for safety
            migrationBuilder.Sql("UPDATE \"MoodEntries\" SET \"MoodTemp\" = 2 WHERE \"MoodTemp\" IS NULL");

            // 3. Drop old column
            migrationBuilder.DropColumn(name: "Mood", table: "MoodEntries");

            // 4. Rename Temp to Mood
            migrationBuilder.RenameColumn(name: "MoodTemp", table: "MoodEntries", newName: "Mood");
            
            // 5. Enforce non-nullable (if needed) - EF might do this in subsequent steps or we add alter column
            // For SQLite/Postgres compatibility, this might need specific care, 
            // but Rename usually keeps properties. We might need to AlterColumn to set NotNull.
        }
        ```
    *   Apply migration: `docker compose restart backend`.
*   **Verification:** Check database `MoodEntries` table. Column `Mood` should be numeric, and values should be 1, 2, or 3.

### Step 3: Update Frontend Models and Translations
*   **Goal:** Prepare frontend to understand the 5 new values.
*   **Action:**
    *   Modify `app/frontend/src/models/MoodType.ts`:
        ```typescript
        export type MoodType = 0 | 1 | 2 | 3 | 4;
        export const MoodValues = {
          VerySad: 0 as MoodType,
          Sad: 1 as MoodType,
          Neutral: 2 as MoodType,
          Happy: 3 as MoodType,
          VeryHappy: 4 as MoodType,
        };
        ```
    *   Update `app/frontend/src/i18n/locales/fr.json` and `en.json`:
        *   Add `mood.verySad`, `mood.veryHappy`.
        *   Ensure `sad`, `neutral`, `happy` are still present.
*   **Verification:** No TypeScript type errors.

### Step 4: Update Entry Interface (DailyMoodWidget)
*   **Goal:** Allow selection of 5 moods with a balanced layout.
*   **Action:**
    *   Modify `app/frontend/src/components/dashboard/DailyMoodWidget.tsx`.
    *   Add `VerySad` and `VeryHappy` to the buttons list.
    *   Reduce button size (e.g., from 110px to 75px) and icon size to fit 5 items horizontally.
    *   Map icons: `SentimentVeryDissatisfied`, `SentimentDissatisfied`, `SentimentNeutral`, `SentimentSatisfied`, `SentimentVerySatisfied`.
*   **Verification:** Widget displays 5 icons nicely on the dashboard.

### Step 5: Update Grid Display and Interaction (MoodGridDisplay)
*   **Goal:** Efficiently select moods in the grid without multiple clicks.
*   **Action:**
    *   Modify `app/frontend/src/components/sprints/MoodGridDisplay.tsx`.
    *   **Update Colors/Icons:** Create a helper to map 5 levels to 5 colors and icons.
    *   **Implement Popover Selection:** 
        *   Remove the "click to increment" logic.
        *   Add a `Menu` or `Popover` component that opens at the clicked cell's position.
        *   Show the 5 mood icons in this menu for direct selection.
*   **Verification:** Clicking a cell in the grid opens a menu, and selecting an icon updates the mood.

## 4. 🧪 Testing Strategy
*   **Backend Unit Tests:** Verify `MoodService` accepts values 0-4.
*   **Manual Tests:**
    1.  Verify that an old "Happy" entry (formerly 2) still displays as "Happy" (now 3).
    2.  Create a "Very Happy" entry and verify display.
    3.  Create a "Very Sad" entry and verify display.

## 5. ✅ Success Criteria
*   Application allows input of 5 mood levels.
*   Old data is not corrupted and is semantically shifted.
*   Interface is consistent (translations, icons).