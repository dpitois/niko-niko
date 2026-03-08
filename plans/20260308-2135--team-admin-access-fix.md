# Implementation Plan - Access to Teams Administration for Team Admins

## 1. 🔍 Analysis & Context
*   **Objective:** Ensure that Team Administrators can access the "Teams" menu to manage their members, even if they are not Super Admins.
*   **Affected Files:**
    *   **Backend:**
        *   `api/NikoNiko.Services/TokenService.cs` (to add administrative claims).
    *   **Frontend:**
        *   `app/frontend/src/components/layout/Sidebar.tsx` (to update menu visibility).
        *   `app/frontend/src/context/AuthContext.tsx` (to refine role detection).
        *   `app/frontend/src/ProtectedRoute.tsx` (to allow Team Admins).
        *   `app/frontend/src/App.tsx` (to update route guards).
*   **Key Dependencies:** JWT Claims, AuthContext, React Router.
*   **Risks/Unknowns:** None identified.

## 2. 📋 Checklist
- [x] Step 1: Add administrative claims to JWT
- [x] Step 2: Fix Sidebar menu visibility for Team Admins
- [x] Step 3: Expose role refresh mechanism in AuthContext
- [x] Step 4: Fix Route Guard for Admin Teams page
- [x] Verification: Login as Team Admin and confirm access to "Teams" menu

## 3. 📝 Step-by-Step Implementation Details

### 🚀 Execution Rules
1. **Interactive Flow**: Execute ONLY ONE step at a time.
2. **Atomic Updates**: Before and after each step, you MUST use `replace` to update the checklist in section 2 and the status in section 3.
3. **Status Vocabulary**: Use `[x]` (Done), `[>]` (In Progress), `[-]` (Skipped/N.A.), `[!]` (Failed/Blocked).
4. **User Confirmation**: After updating the file, stop and wait for explicit user confirmation before proceeding to the next step.
5. **No Stealth Actions**: NEVER execute code or modify files without having first updated the plan to reflect that you are about to do so.

### Step 1: Add administrative claims to JWT [x]
*   **Goal:** Ensure the JWT reflects if a user is an administrator of at least one team.
*   **Action:**
    *   Modified `TokenService.cs` to inject `ApplicationDbContext`.
    *   Added check: `_context.Teams.Any(t => t.AdminId == user.Id)` to add `is_team_admin: "true"` claim.
    *   Updated `TokenServiceTests.cs` to handle the new dependency.
*   **Verification:** Backend build successful.

### Step 2: Fix Sidebar menu visibility for Team Admins [x]
*   **Goal:** Allow Team Admins to see the "Teams" sub-menu under Administration.
*   **Action:**
    *   Modified `Sidebar.tsx`.
    *   Changed visibility of `/admin/teams` to use `(isSuperAdmin || isAnyTeamAdmin)`.
*   **Verification:** Visual check in UI (menu should be visible if `userTeamRoles` correctly lists `isAdmin`).

### Step 3: Expose role refresh mechanism in AuthContext [x]
*   **Goal:** Ensure roles are up-to-date without logout/login if a transfer occurs.
*   **Action:**
    *   Modified `AuthContext.tsx` to include `fetchUserTeamRoles` in the context type and provider value.
    *   Modified `AdminTeamListItem.tsx` to call `fetchUserTeamRoles` after an administrative transfer.
*   **Verification:** `useAuth()` now returns `fetchUserTeamRoles`.

### Step 4: Fix Route Guard for Admin Teams page [x]
*   **Goal:** Prevent redirection to home page for Team Admins when accessing `/admin/teams`.
*   **Action:**
    *   Modified `usePermissions.ts` to include `isAnyTeamAdmin()`.
    *   Modified `ProtectedRoute.tsx` to handle `requiredAnyAdmin`.
    *   Modified `App.tsx` to use `requiredAnyAdmin={true}` for `/admin/teams`.
*   **Verification:** Click on "Teams" menu as Team Admin and confirm the page loads without redirect.

## 4. 🧪 Testing Strategy
*   **Manual Verification:** 
    1. Login as User A (Admin of Team 1).
    2. Confirm "Administration > Teams" is visible and only shows "Team 1".
    3. Transfer Team 1 to User B.
    4. Confirm "Administration > Teams" disappears or updates for User A.

## 5. ✅ Success Criteria
*   Team Admins can see and access the "Teams" management view.
*   The "Teams" view only shows teams that the user actually administrates (enforced by backend).
*   Super Admins still see all teams.
