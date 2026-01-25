# Implementation Plan - Frontend Date Migration to Day.js

## 1. 🔍 Analysis & Context
*   **Objective:** Replace native `Date` usage with `dayjs` across the frontend to ensure consistent formatting, avoid timezone bugs, and centralize date configuration.
*   **Affected Files:** 
    *   Config: `src/App.tsx`, new `src/utils/dayjsConfig.ts`
    *   Pages: `AdminSprintsPage.tsx`, `CurrentSprintsPage.tsx`, `DashboardPage.tsx`, `PastSprintsPage.tsx`, `SprintDetailsPage.tsx`, `AdminUsersPage.tsx`, `ProfilePage.tsx`
    *   Components: `AdminCreateSprintForm.tsx`, `AdminEditSprintDialog.tsx`, `sprints/SprintMoodGrid.tsx`, `sprints/PastSprintDetails.tsx`
    *   Hooks/Utils: `hooks/useMoodMutation.ts`, `utils/moodTrendUtils.ts`
*   **Key Dependencies:** `dayjs` (installed).
*   **Risks/Unknowns:** Potential subtle bugs if `dayjs` parsing logic differs slightly from current custom logic (e.g. strict vs loose parsing). Syncing dayjs locale with app language.

## 2. 📋 Checklist
- [x] Step 1: Centralize Day.js configuration
- [x] Step 2: Clean up scattered `dayjs.extend` calls
- [x] Step 3: Refactor Admin Pages (Sprints & Users)
- [x] Step 4: Refactor Sprint Display Pages (Dashboard, Current, Past, Details)
- [x] Step 5: Refactor Sprint Forms & Dialogs
- [x] Step 6: Refactor Grid & Logic Components
- [x] Verification

## 3. 📝 Step-by-Step Implementation Details

### 🚀 Execution Rules
1. **Interactive Flow**: Execute ONLY ONE step at a time.
2. **Atomic Updates**: Before and after each step, you MUST use `replace` to update the checklist in section 2 and the status in section 3.
3. **Status Vocabulary**: Use `[x]` (Done), `[>]` (In Progress), `[-]` (Skipped/N.A.), `[!]` (Failed/Blocked).
4. **User Confirmation**: After updating the file, stop and wait for explicit user confirmation before proceeding to the next step.
5. **No Stealth Actions**: NEVER execute code or modify files without having first updated the plan to reflect that you are about to do so.

### Step 1: Centralize Day.js configuration
*   **Goal:** Create a single source of truth for Day.js plugins and initialization.
*   **Action:**
    *   Create `app/frontend/src/utils/dayjsConfig.ts`.
    *   Import `dayjs` and extend with: `localizedFormat`, `utc`, `timezone`, `isSameOrAfter`, `isSameOrBefore`, `minMax`.
    *   Import specific locales (`fr`, `en`).
    *   Modify `app/frontend/src/main.tsx` (or `App.tsx`) to import this config file at the top.
*   **Verification:** App compiles, no runtime errors in console regarding missing dayjs functions.

### Step 2: Clean up scattered `dayjs.extend` calls
*   **Goal:** Remove redundant configuration calls to avoid side effects or confusion.
*   **Action:**
    *   Remove `dayjs.extend(...)` from:
        *   `src/App.tsx`
        *   `src/pages/PastSprintsPage.tsx`
        *   `src/pages/DashboardPage.tsx`
        *   `src/pages/CurrentSprintsPage.tsx`
        *   `src/utils/moodTrendUtils.ts`
*   **Verification:** `search_file_content` for "dayjs.extend" should only return the new config file.

### Step 3: Refactor Admin Pages (Sprints & Users)
*   **Goal:** Replace native date formatting in admin tables.
*   **Action:**
    *   In `src/pages/AdminSprintsPage.tsx`: Replace `formatDate` helper and `new Date().toLocaleDateString()` with `dayjs(date).format('L')` (or 'DD/MM/YYYY' if strict format needed).
    *   In `src/pages/AdminUsersPage.tsx`: Replace `toLocaleDateString()` for `createdAt` and `expirationDate`.
*   **Verification:** Admin lists show dates correctly.

### Step 4: Refactor Sprint Display Pages
*   **Goal:** Standardize date display in user-facing sprint pages.
*   **Action:**
    *   In `src/pages/CurrentSprintsPage.tsx`, `src/pages/DashboardPage.tsx`, `src/pages/PastSprintsPage.tsx`, `src/pages/SprintDetailsPage.tsx`:
        *   Replace `new Date(...).toLocaleDateString()` with `dayjs(...).format('L')`.
*   **Verification:** Dashboard and sprint details show formatted dates.

### Step 5: Refactor Sprint Forms & Dialogs
*   **Goal:** Use Day.js for date comparisons and inputs in forms.
*   **Action:**
    *   In `src/components/AdminCreateSprintForm.tsx`: Replace `new Date(startDate) >= new Date(endDate)` with `dayjs(startDate).isSameOrAfter(dayjs(endDate))`.
    *   In `src/components/AdminEditSprintDialog.tsx`: Replace comparison logic similarly. Note: The input `value` for type="date" still needs "YYYY-MM-DD", `dayjs().format('YYYY-MM-DD')` handles this well.
*   **Verification:** Creating/Editing sprints prevents invalid date ranges correctly.

### Step 6: Refactor Grid & Logic Components
*   **Goal:** Secure complex date arithmetic (loops, UTC handling).
*   **Action:**
    *   In `src/components/sprints/SprintMoodGrid.tsx` & `src/components/sprints/PastSprintDetails.tsx`: Replace `new Date()` loop logic with `dayjs` iteration (`current.add(1, 'day')`).
    *   In `src/hooks/useMoodMutation.ts`: Review `safeDate` construction. Ensure we send the correct timestamp/date expected by backend.
    *   In `src/pages/ProfilePage.tsx`: Use `dayjs().format('YYYY-MM-DD')` for the filename timestamp.
*   **Verification:** Mood grids render the correct number of days without "missing" days due to DST/timezone shifts.

## 4. 🧪 Testing Strategy
*   **Unit Tests:** None explicitly (frontend logic mostly).
*   **Manual Verification:**
    1.  Open Dashboard: Check Sprint Dates (e.g. "01/01/2026 - 15/01/2026").
    2.  Switch Language (FR <-> EN): Check if date format adapts (DD/MM vs MM/DD).
    3.  Admin Sprints: Create a sprint, verify dates in list. Edit it, verify dates in picker.
    4.  Mood Grid: Verify "Today" is highlighted correctly.

## 5. ✅ Success Criteria
*   Zero occurrences of `new Date()` (except maybe for strictly required libraries/constructors not compatible with dayjs).
*   Zero occurrences of `toLocaleDateString()`.
*   All dates displayed in UI use `dayjs`.
*   Application builds and runs without errors.
