# Implementation Plan - MoodGrid Column Highlight & Button Cleanup

## 1. 🔍 Analysis & Context
*   **Objective:** Highlight the "Today" column using the header and borders, while ensuring that the date is never displayed on interactive buttons (including today).
*   **Affected Files:**
    *   `app/frontend/src/components/sprints/SprintMoodGrid.tsx`
*   **Key Dependencies:** Material UI (MUI), dayjs.
*   **Risks/Unknowns:** Ensuring border continuity across cells for the column highlight.

## 2. 📋 Checklist
- [ ] Step 1: Highlight "Today" in the Header Row.
- [ ] Step 2: Implement vertical border highlight for the "Today" column.
- [ ] Step 3: Remove date display from "Today" cells (keep only for future).
- [ ] Verification

## 3. 📝 Step-by-Step Implementation Details

### Step 1: Highlight "Today" in the Header Row
*   **Goal:** Make the current date cell in the header stand out.
*   **Action:**
    *   In the `sprintDates.map` for the header row:
    *   Add `const isToday = dayjs(date).isSame(today, 'day');`.
    *   Update `Paper` sx:
        ```tsx
        backgroundColor: isToday 
          ? theme.palette.primary.main 
          : (theme.palette.mode === 'dark' ? theme.palette.grey[800] : theme.palette.grey[200]),
        color: isToday ? theme.palette.primary.contrastText : 'inherit',
        ```
*   **Verification:** Today's date in the header should be highlighted with the primary color.

### Step 2: Implement vertical border highlight for the "Today" column
*   **Goal:** Create a visual "rail" for the current day.
*   **Action:**
    *   In the member rows `sprintDates.map`:
    *   Apply a specific border style to the `Paper` if `isToday` is true.
    *   Use `outline` or `boxShadow` to avoid layout shifts, or adjust `borderLeft` and `borderRight`.
    *   Example: `border: isToday ? `2px solid ${theme.palette.primary.main}` : 'none'`.
*   **Verification:** All cells under "Today" should have a visible vertical highlight.

### Step 3: Remove date display from "Today" cells
*   **Goal:** Clean up buttons to show only icons/moods.
*   **Action:**
    *   Locate the `Typography` rendering `{date.getDate()}`.
    *   Change the condition from `(isFuture || isToday)` to just `isFuture`.
    *   Ensure `isToday` cells only display the mood icon (via `getMoodIcon`) and maintain their background color logic.
*   **Verification:** "Today" cells should be empty or show an icon, with NO number. Future cells should still show the number.

## 4. 🧪 Testing Strategy
*   **Manual Verification:**
    1.  Open Dashboard.
    2.  Check that the header for today is blue/primary.
    3.  Check that the column for today has vertical borders.
    4.  Verify that today's cell does NOT show the day number.
    5.  Verify that future cells DO show the day number and are not clickable.
    6.  Verify that clicking today's cell cycles through moods correctly.

## 5. ✅ Success Criteria
*   The current day's column is clearly identified via header color and borders.
*   Dates are only visible on non-interactive future cells.
*   Buttons (today and past) never display a date number.