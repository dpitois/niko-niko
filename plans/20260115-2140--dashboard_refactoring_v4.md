# Implementation Plan - Dashboard Refactoring & Fixes

## 1. 🔍 Analysis & Context
*   **Objective:**
    1.  **UX/UI:** Modernize Dashboard (remove Card-in-Card, simplify header).
    2.  **Logic:** Fix initial mood loading & ensure instant visual feedback (optimistic update).
    3.  **Style:** Fix Chart Tooltip visibility in Dark Mode.
*   **Affected Files:** `app/frontend/src/pages/DashboardPage.tsx`, `app/frontend/src/components/dashboard/DailyMoodWidget.tsx`, `app/frontend/src/components/dashboard/TeamMoodTrendWidget.tsx`.

## 2. 📋 Checklist
- [ ] Step 1: Fix Chart Tooltip (Dark Mode)
- [ ] Step 2: Fix Mood Loading & Optimistic Update
- [ ] Step 3: Clean Dashboard UI & Implement Transparent Layout
- [ ] Verification

## 3. 📝 Step-by-Step Implementation Details

### Step 1: Fix Chart Tooltip (Dark Mode)
*   **File:** `app/frontend/src/components/dashboard/TeamMoodTrendWidget.tsx`
*   **Action:**
    *   Inject `theme` into `<Tooltip>` props.
    *   Set `contentStyle={{ backgroundColor: theme.palette.background.paper, color: theme.palette.text.primary, border: '1px solid ' + theme.palette.divider }}`.

### Step 2: Fix Mood Loading & Optimistic Update
*   **File:** `app/frontend/src/components/dashboard/DailyMoodWidget.tsx`
*   **Action:**
    *   **Logic Fix:** Compare dates using `startOf('day')` strictly.
    *   **Optimistic UI:**
        *   Add state `tempMood: MoodType | null`.
        *   On Click -> `setTempMood(newMood)`.
        *   Render checks `tempMood ?? serverMood`.
        *   On API Success -> `setTempMood(null)` (let server data take over) + `mutate()`.
        *   On Error -> `setTempMood(null)` (revert).

### Step 3: Clean Dashboard UI & Transparent Layout
*   **File:** `app/frontend/src/pages/DashboardPage.tsx`
*   **Action:**
    *   **Remove:** `<Card>` wrapper, `EditDialog`, Owner Chip, Edit Button.
    *   **Structure:**
        ```tsx
        <Box sx={{ mb: 6 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', borderBottom: 1, borderColor: 'divider', pb: 1, mb: 2 }}>
             <Box sx={{ display: 'flex', alignItems: 'baseline', gap: 2 }}>
                <Typography variant="h5" fontWeight="bold">{team.name}</Typography>
                <Typography variant="subtitle1" color="text.secondary">
                    {currentSprint.name}
                    <Typography component="span" variant="caption" sx={{ ml: 1 }}>
                        ({formatDate(start)} - {formatDate(end)})
                    </Typography>
                </Typography>
             </Box>
             <Button component={RouterLink} to="..." endIcon={<ArrowForwardIcon />} size="small" color="inherit">
                {t('dashboard.viewFullBoard')}
             </Button>
          </Box>
          <Grid container ...>
             {/* Widgets */}
          </Grid>
        </Box>
        ```

## 4. 🧪 Testing Strategy
*   **Dark Mode:** Check Chart Tooltip readability.
*   **Mood:** Check instant update on click.
*   **Layout:** Verify transparency and new header.

## 5. ✅ Success Criteria
*   Clean UI, correct Dark Mode charts, "snappy" mood selection.
