# Implementation Plan - New Dashboard UX

## 1. 🔍 Analysis & Context
*   **Objective:** Redesign the dashboard to feature a prominent daily mood selector, team mood trends, and move the detailed grid to a separate view to improve user engagement ("sexy" UI).
*   **Affected Files:** `app/frontend/package.json`, `app/frontend/src/App.tsx`, `app/frontend/src/pages/DashboardPage.tsx`, new components in `app/frontend/src/components/dashboard/`, `app/frontend/src/pages/SprintDetailsPage.tsx`.
*   **Key Dependencies:** `recharts` (for trend charts), `Material UI` (Grid, Card, DatePicker).
*   **Risks/Unknowns:** "Sexy" design is subjective; adhering to standard MUI with good spacing/shadows. Client-side aggregation for charts assumes reasonable data size per sprint.

## 2. 📋 Checklist
- [ ] Step 1: Install Dependencies (recharts)
- [ ] Step 2: Create Sprint Details Page
- [ ] Step 3: Update Routing
- [ ] Step 4: Create Daily Mood Widget
- [ ] Step 5: Create Team Trend Widget
- [ ] Step 6: Refactor Dashboard Page
- [ ] Verification

## 3. 📝 Step-by-Step Implementation Details

### Step 1: Install Dependencies
*   **Goal:** Add charting capability.
*   **Action:**
    *   Run command: `npm install recharts` in `app/frontend`.

### Step 2: Create Sprint Details Page
*   **Goal:** A dedicated page for the detailed "Excel-like" view of the sprint (MoodGrid).
*   **Action:**
    *   Create `app/frontend/src/pages/SprintDetailsPage.tsx`.
    *   **Logic:**
        *   Read `teamId` and `sprintId` from `useParams`.
        *   Fetch Team details (using `useTeams` or specific service) and Sprint details (using `useSprints` or derived from team).
        *   Render `PageContainer` with a back button.
        *   Render `SprintMoodGrid` (reuse existing component).
    *   **Code Structure (Sketch):**
        ```tsx
        export default function SprintDetailsPage() {
           const { teamId, sprintId } = useParams();
           // ... fetch logic ...
           return (
             <PageContainer title="Sprint Details" backPath="/my-teams">
                <SprintMoodGrid ... />
             </PageContainer>
           );
        }
        ```

### Step 3: Update Routing
*   **Goal:** Enable navigation to the new detailed view.
*   **Action:**
    *   Modify `app/frontend/src/App.tsx`:
        *   Import `SprintDetailsPage`.
        *   Add route: `<Route path="/teams/:teamId/sprints/:sprintId" element={<ProtectedRoute><SprintDetailsPage /></ProtectedRoute>} />`.

### Step 4: Create Daily Mood Widget
*   **Goal:** A "sexy", prominent card for entering today's (or past) mood.
*   **Action:**
    *   Create `app/frontend/src/components/dashboard/DailyMoodWidget.tsx`.
    *   **Props:** `teamId`, `sprintId`.
    *   **UI:**
        *   MUI `Card` with `elevation={3}`.
        *   Header: "How are you feeling?"
        *   `DatePicker` (MUI X Date Pickers) to select date (default today, constrained to sprint start/end).
        *   Row of 3 large IconButton/Avatar for Happy/Neutral/Sad.
        *   Visual feedback (highlight selected mood).
    *   **Logic:**
        *   Fetch single user mood for selected date (`useMoods` or direct service call).
        *   `onClick` -> call `updateMoodEntry` -> `mutate` SWR.

### Step 5: Create Team Trend Widget
*   **Goal:** Visualize team morale over the sprint.
*   **Action:**
    *   Create `app/frontend/src/components/dashboard/TeamMoodTrendWidget.tsx`.
    *   **Props:** `teamId`, `sprintId`.
    *   **UI:**
        *   MUI `Card`.
        *   `ResponsiveContainer`, `AreaChart` from `recharts`.
    *   **Logic:**
        *   Use `useMoods(sprintId)` to get all moods.
        *   **Aggregation:** Group by `date`. Calculate average mood score (Happy=2, Neutral=1, Sad=0) per day.
        *   Data passed to Chart: `[{ date: '2023-01-01', score: 1.5 }, ...]`.

### Step 6: Refactor Dashboard Page
*   **Goal:** Assemble the new homepage.
*   **Action:**
    *   Modify `app/frontend/src/pages/DashboardPage.tsx`.
    *   **Layout:**
        *   Remove the loop that renders `SprintMoodGrid` directly.
        *   Loop through `teams`.
        *   For each team with an active sprint:
            *   Render a container (e.g., `Box` or `Paper` with header).
            *   Use `Grid` (MUI v7 syntax `size={{ xs: 12, md: 6 }}`).
            *   Col 1: `DailyMoodWidget`.
            *   Col 2: `TeamMoodTrendWidget`.
            *   Footer/Header of section: Button "View Full Board" linking to `/teams/:teamId/sprints/:sprintId`.

## 4. 🧪 Testing Strategy
*   **Unit Tests:** Verify `DailyMoodWidget` updates correctly on click.
*   **Integration Tests:**
    *   Navigate to Dashboard.
    *   Ensure "My Teams" are listed.
    *   Change date on Widget -> Verify mood updates if it existed.
    *   Click "View Full Board" -> Verify navigation to `SprintDetailsPage`.
*   **Manual Verification:**
    *   Run app.
    *   Check responsive layout (Mobile vs Desktop).
    *   Verify Chart renders lines.

## 5. ✅ Success Criteria
*   Dashboard displays widgets instead of full grids.
*   Daily Mood can be entered for Today and Yesterday via the widget.
*   Trend chart is visible and not crashing.
*   Full Grid is accessible via click.
