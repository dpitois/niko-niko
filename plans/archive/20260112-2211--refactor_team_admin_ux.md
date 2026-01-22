# Implementation Plan - Refactor Team Admin UX & Security

## 1. 🔍 Analysis & Context
*   **Objective:** Improve the User Experience for Team Admins by providing access to standard management menus (Users, Sprints) with restricted scope, displaying Team Owner information, and securing user deletion.
*   **Affected Files:**
    *   **Backend:** `api/NikoNiko.Core/DTOs/Team/TeamDto.cs`, `api/NikoNiko.Api/Controllers/TeamsController.cs`, `api/NikoNiko.Api/Controllers/UsersController.cs`.
    *   **Frontend:** `app/frontend/src/components/layout/Sidebar.tsx`, `app/frontend/src/components/ProtectedRoute.tsx`, `app/frontend/src/pages/AdminUsersPage.tsx`, `app/frontend/src/pages/DashboardPage.tsx`, `app/frontend/src/models/Team.ts`.
*   **Key Dependencies:** `useAuth`, `usePermissions`.
*   **Risks/Unknowns:** Ensure `TeamsController` properly populates the new `AdminName` field without causing N+1 query issues (use explicit `.Select()`).

## 2. 📋 Checklist
- [ ] Backend: Prevent self-deletion in `UsersController`.
- [ ] Backend: Add `AdminName` to `TeamDto` and populate it in `TeamsController`.
- [ ] Frontend: Update `Team` model to include `adminName`.
- [ ] Frontend: Disable "Delete" button for current user in `AdminUsersPage`.
- [ ] Frontend: Display "Owner" Chip in `DashboardPage` and `AdminTeamsPage`.
- [ ] Frontend: Update `Sidebar` and `App` routing to allow Team Admins to access Admin menus.

## 3. 📝 Step-by-Step Implementation Details

### Step 1: Secure User Deletion (Backend)
*   **Goal:** Prevent accidental or malicious self-deletion.
*   **Action:**
    *   In `api/NikoNiko.Api/Controllers/UsersController.cs`, method `DeleteUser`:
    *   Get `currentUserId` from Claims.
    *   Add check: `if (id == currentUserId) return BadRequest("Cannot delete yourself.");`.

### Step 2: Expose Team Admin Name (Backend)
*   **Goal:** Provide data to display the "Owner" chip.
*   **Action:**
    *   Modify `api/NikoNiko.Core/DTOs/Team/TeamDto.cs`: Add `public string AdminName { get; set; } = null!;`.
    *   Modify `api/NikoNiko.Api/Controllers/TeamsController.cs`:
        *   In `GetTeams` and `GetTeam`, update the LINQ `.Select()` projection.
        *   `AdminName = t.Admin.Name` (Ensure `t.Admin` is joined/available).

### Step 3: Update Frontend Model & UI (Owner Chip)
*   **Goal:** Visual indication of team ownership.
*   **Action:**
    *   Update `app/frontend/src/models/Team.ts`: Add `adminName: string;`.
    *   In `app/frontend/src/pages/DashboardPage.tsx` (inside `TeamDashboardSection`):
        *   Import `Chip`, `FaceIcon`.
        *   Add `<Chip icon={<FaceIcon />} label={`Owner: ${team.adminName}`} ... />` near the team title.
    *   *(Optional)* Do the same for `AdminTeamsPage.tsx` if strictly required, but priority is Dashboard/MyTeams.

### Step 4: Refactor Navigation & Routing
*   **Goal:** Allow Team Admins to access `/admin/users` and `/admin/sprints`.
*   **Action:**
    *   In `app/frontend/src/components/layout/Sidebar.tsx`:
        *   Update the condition `{isSuperAdmin && (...)}`.
        *   Change to `{(isSuperAdmin || hasAnyTeamAdminRole) && (...)}` (You might need to derive `hasAnyTeamAdminRole` from `usePermissions` or `useAuth` -> `Object.values(userTeamRoles).some(r => r.isAdmin)`).
    *   In `app/frontend/src/App.tsx`:
        *   Update `ProtectedRoute` for `/admin/users` and `/admin/sprints`.
        *   Remove `requiredSuperAdmin={true}`.
        *   The pages themselves (or the API) handle the data filtering.

### Step 5: Secure Frontend User Deletion
*   **Goal:** Visual feedback that you can't delete yourself.
*   **Action:**
    *   In `app/frontend/src/pages/AdminUsersPage.tsx`:
        *   Get `user` (current user) from `useAuth`.
        *   In the `map` loop for users table:
        *   `disabled={currentUser.id === targetUser.id}` on the Delete IconButton.

## 4. 🧪 Testing Strategy
*   **Security Test:** Try to delete own account via API (Postman) -> Expect 400.
*   **UI Test (Team Admin):**
    *   Login as Team Admin.
    *   Verify "Admin" menu is visible.
    *   Go to "Users" -> See only my team members.
    *   Try to delete myself -> Button disabled.
    *   Go to Dashboard -> See "Owner: [Name]" chip.
*   **UI Test (Super Admin):**
    *   Verify all standard access remains unchanged.

## 5. ✅ Success Criteria
*   Self-deletion is impossible (Backend & Frontend).
*   Team Owner is visible on Team cards.
*   Team Admins can navigate to Users/Sprints management pages using the sidebar.
