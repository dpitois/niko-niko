# Implementation Plan - Feature User Profile & Privacy Control Center

## 1. 🔍 Analysis & Context
*   **Objective:** Create a dedicated user profile page to manage personal information, delete account (GDPR), and view private mood history with pagination.
*   **Affected Files:**
    *   `api/NikoNiko.Api/Controllers/UsersController.cs`
    *   `api/NikoNiko.Api/Controllers/MoodEntriesController.cs`
    *   `app/frontend/src/App.tsx`
    *   `app/frontend/src/services/users.ts`
    *   `app/frontend/src/services/moods.ts`
    *   `app/frontend/src/pages/ProfilePage.tsx` (New)
*   **Key Dependencies:** `MUI` (Tabs, Pagination), `swr`, `axios`, `React Router`.
*   **Risks/Unknowns:**
    *   **Self-Deletion:** Must ensure orphaned teams are handled.
    *   **Pagination:** Need a standard PagedResult structure in API.

## 2. 📋 Checklist
- [x] Step 1: Backend - Implement `DELETE /api/users/me`
- [x] Step 2: Backend - Implement `GET /api/mood-entries/me` (Paginated)
- [x] Step 3: Frontend - Add API Services & Route
- [x] Step 4: Frontend - Create Profile Page Structure (Tabs)
- [x] Step 5: Frontend - Implement General Info & Delete Logic (Export Disabled)
- [x] Step 6: Frontend - Implement Mood History Tab (Paginated)
- [x] Verification

## 3. 📝 Step-by-Step Implementation Details

### 🚀 Execution Rules
1. **Interactive Flow**: Execute ONLY ONE step at a time.
2. **Atomic Updates**: Before and after each step, you MUST use `replace` to update the checklist in section 2 and the status in section 3.
3. **Status Vocabulary**: Use `[x]` (Done), `[>]` (In Progress), `[-]` (Skipped/N.A.), `[!]` (Failed/Blocked).
4. **User Confirmation**: After updating the file, stop and wait for explicit user confirmation before proceeding to the next step.
5. **No Stealth Actions**: NEVER execute code or modify files without having first updated the plan to reflect that you are about to do so.

### Step 1: Backend - Implement `DELETE /api/users/me`
*   **Goal:** Allow a user to delete their own account.
*   **Status:** Done
*   **Action:**
    *   Modify `api/NikoNiko.Api/Controllers/UsersController.cs`:
        *   Add `[HttpDelete("me")]`.
        *   Logic: Get `userId` from claims. Validate (check if admin of team with other members). Delete user.
*   **Verification:** Swagger test `DELETE /api/users/me`.

### Step 2: Backend - Implement `GET /api/mood-entries/me` (Paginated)
*   **Goal:** Efficiently fetch the current user's mood history with pagination.
*   **Status:** Done
*   **Action:**
    *   Modify `api/NikoNiko.Api/Controllers/MoodEntriesController.cs`:
        *   Add `[HttpGet("me")]` with parameters `[FromQuery] int page = 1`, `[FromQuery] int pageSize = 20`.
        *   Logic: `_context.MoodEntries.Where(m => m.UserId == currentUserId).OrderByDescending(m => m.Date)`.
        *   Return: `{ items: IEnumerable<MoodEntryDto>, totalCount: int, page: int, pageSize: int }`.
*   **Verification:** Swagger test `GET /api/mood-entries/me?page=1&pageSize=5`.

### Step 3: Frontend - Add API Services & Route
*   **Goal:** Prepare frontend plumbing.
*   **Status:** Done
*   **Action:**
    *   Modify `app/frontend/src/services/users.ts`: Add `deleteMe()`.
    *   Modify `app/frontend/src/services/moods.ts`: Add `getMyMoodHistory(page, pageSize)`.
    *   Modify `app/frontend/src/App.tsx`: Add Route `/profile` -> `ProfilePage`.
    *   Create `app/frontend/src/pages/ProfilePage.tsx` (Skeleton).
*   **Verification:** App compiles, navigating to `/profile` shows skeleton.

### Step 4: Frontend - Create Profile Page Structure (Tabs)
*   **Goal:** Layout with Tabs.
*   **Status:** Done
*   **Action:**
    *   Modify `app/frontend/src/pages/ProfilePage.tsx`:
        *   Use MUI `Tabs`: "Profile", "Data & Privacy", "Mood History".
*   **Verification:** Visual check of tabs.

### Step 5: Frontend - Implement General Info & Delete Logic
*   **Goal:** Show user info and allow dangerous actions. Export is disabled.
*   **Status:** Done
*   **Action:**
    *   Modify `app/frontend/src/pages/ProfilePage.tsx`:
        *   "Profile": Avatar, Name, Email, Provider.
        *   "Data & Privacy":
            *   Button "Export Data" -> **Disabled** (Tooltip: "Coming soon").
            *   Button "Delete Account" (Red) -> Confirmation Dialog -> calls `deleteMe` -> Logout.
*   **Verification:** Check Export button is disabled. Test Delete (with mock/test account).

### Step 6: Frontend - Implement Mood History Tab (Paginated)
*   **Goal:** Visualize history with pagination.
*   **Status:** Done
*   **Action:**
    *   Modify `app/frontend/src/pages/ProfilePage.tsx`:
        *   "Mood History": Fetch `getMyMoodHistory`.
        *   Render list/table.
        *   Add MUI `TablePagination` or similar control to change page/pageSize.
*   **Verification:** Verify pagination controls load new data.

## 4. 🧪 Testing Strategy
*   **Unit Tests:**
    *   `UsersControllerTests`: Test `DeleteMe` constraints.
    *   `MoodEntriesControllerTests`: Test Pagination logic (page 1 vs page 2).
*   **Manual Verification:**
    *   Go to `/profile`.
    *   Check "Export" is disabled.
    *   Check Mood History pagination (create > 20 moods if needed).
    *   Delete Account.

## 5. ✅ Success Criteria
*   User can navigate to `/profile`.
*   "Export Data" button is visible but disabled.
*   User can delete their account (GDPR).
*   User can view their mood history with pagination.