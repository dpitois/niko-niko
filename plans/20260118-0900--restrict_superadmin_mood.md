# Implementation Plan - Restrict Mood Entry for Super Admins

## 1. 🔍 Analysis & Context
*   **Objective:** Restrict mood submission (POST) to actual team members/admins only. **Critical:** Super Admins must retain full READ access to all sprints, charts, and grids on the dashboard, but must be explicitly blocked from WRITING (entering moods) for teams they are not members of.
*   **Affected Files:**
    *   `api/NikoNiko.Api/Controllers/MoodEntriesController.cs`
    *   `app/frontend/src/pages/DashboardPage.tsx`
    *   `app/frontend/src/components/dashboard/DailyMoodWidget.tsx`
*   **Key Dependencies:** `AuthContext` (Frontend), `ApplicationDbContext` (Backend).
*   **Risks/Unknowns:** Ensure the "Observer" state in the UI is clear and doesn't break the layout.

## 2. 📋 Checklist
- [x] Step 1: Create Integration Test (Reproduce Security Gap)
- [x] Step 2: Secure Backend Endpoint (`CreateMoodEntry`)
- [x] Step 3: Pass `teamId` to Dashboard Widgets
- [x] Step 4: Implement "Observer Mode" in DailyMoodWidget
- [>] Verification

## 3. 📝 Step-by-Step Implementation Details

### 🚀 Execution Rules
1. **Interactive Flow**: Execute ONLY ONE step at a time.
2. **Atomic Updates**: Before and after each step, you MUST use `replace` to update the checklist in section 2 and the status in section 3.
3. **Status Vocabulary**: Use `[x]` (Done), `[>]` (In Progress), `[-]` (Skipped/N.A.), `[!]` (Failed/Blocked).
4. **User Confirmation**: After updating the file, stop and wait for explicit user confirmation before proceeding to the next step.
5. **No Stealth Actions**: NEVER execute code or modify files without having first updated the plan to reflect that you are about to do so.

### Step 1: Create Integration Test (Reproduce Security Gap)
*   **Goal:** Prove that a non-member (specifically a Super Admin) can currently post a mood entry, then verify the fix.
*   **Status:** [x] Done

### Step 2: Secure Backend Endpoint (`CreateMoodEntry`)
*   **Goal:** Enforce strict membership checks in the API (Write access).
*   **Status:** [x] Done

### Step 3: Pass `teamId` to Dashboard Widgets
*   **Goal:** Provide necessary context to the widget to allow frontend permission checks.
*   **Status:** [x] Done

### Step 4: Implement "Observer Mode" in DailyMoodWidget
*   **Goal:** Improve UX by replacing input buttons with an "Observer" message for non-members.
*   **Status:** [x] Done

### Verification
*   **Goal:** Final review and manual testing.
*   **Status:** [In Progress]
*   **Action:**
    *   Modify `app/frontend/src/components/dashboard/DailyMoodWidget.tsx`:
        *   Import `useAuth`.
        *   Get `userTeamRoles`.
        *   Check `const canPost = userTeamRoles[teamId]?.isMember || userTeamRoles[teamId]?.isAdmin;`.
        *   If `!canPost`, render a nice "Observer Mode" UI (e.g., a Box with Typography "You are observing this team" or similar) instead of the Mood Buttons.
        *   **Important**: Keep the DatePicker or at least the visual container height consistent if possible, or adapt gracefully.
*   **Verification:** Manual check in browser.

## 4. 🧪 Testing Strategy
*   **Integration Tests:** Verify `MoodEntriesSecurityTests` passes.
*   **Manual Verification:**
    1.  Login as Super Admin.
    2.  Go to Dashboard.
    3.  Verify teams where Super Admin is NOT a member show "Observer Mode" (no buttons).
    4.  Verify teams where Super Admin IS a member DO show the mood buttons.
    5.  Verify Charts/Trends are still visible for ALL teams.

## 5. ✅ Success Criteria
*   Super Admins cannot post moods via API for teams they don't belong to (403 Forbidden).
*   Super Admins CAN still see all data (Read access).
*   UI clearly indicates "Observer" status instead of offering unusable buttons.