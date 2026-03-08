# Implementation Plan - Limit Team Creation to 2 per Regular User

Fixes issue #85 and implements a team creation quota (SaaS-like restriction). Regular users are restricted to being administrators of a maximum of 2 teams. SuperAdmins bypass this restriction.

## 1. 🔍 Analysis & Context
*   **Objective:** Allow regular users to create teams while preventing them from owning more than 2 teams. This preventsorphaned teams and controls growth.
*   **Affected Files:**
    *   Backend: `api/NikoNiko.Api/Controllers/TeamsController.cs`, `api/NikoNiko.Services/TeamService.cs`, `api/NikoNiko.Core/Interfaces/ITeamService.cs`, `api/NikoNiko.Api/appsettings.json`, `api/NikoNiko.Api.IntegrationTests/TeamsControllerTests.cs`
    *   Frontend: `app/frontend/src/App.tsx`, `app/frontend/src/components/layout/Sidebar.tsx`, `app/frontend/src/pages/AdminTeamsPage.tsx`
*   **Key Dependencies:** ASP.NET Core Configuration, Authorization Policies, React Router, Material UI.
*   **Risks/Unknowns:** Ensure users invited to join teams are not blocked from creating their own teams as long as they aren't the administrator of more than the limit. Membership doesn't count, only ownership (`AdminId`).

## 2. 📋 Checklist
- [x] Step 1: Add Configuration for Team Limit (Backend)
- [x] Step 2: Implement Team Limit Enforcement in Service (Backend)
- [x] Step 3: Remove SuperAdmin restriction and handle exception in Controller (Backend)
- [x] Step 4: Add Integration Tests for the Limit (Backend)
- [x] Step 5: Update Route Permissions (Frontend)
- [x] Step 6: Update Sidebar Visibility (Frontend)
- [x] Step 7: Show Limit Message in AdminTeamsPage (Frontend)
- [x] Verification

## 3. 📝 Step-by-Step Implementation Details

### 🚀 Execution Rules
1. **Interactive Flow**: Execute ONLY ONE step at a time.
2. **Atomic Updates**: Before and after each step, you MUST use `replace` to update the checklist in section 2 and the status in section 3.
3. **Status Vocabulary**: Use `[x]` (Done), `[>]` (In Progress), `[-]` (Skipped/N.A.), `[!]` (Failed/Blocked).
4. **User Confirmation**: After updating the file, stop and wait for explicit user confirmation before proceeding to the next step.
5. **No Stealth Actions**: NEVER execute code or modify files without having first updated the plan to reflect that you are about to do so.

### Step 1: Add Configuration for Team Limit (Backend) [x]
*   **Goal:** Provide a configurable limit for team creation.
*   **Action:**
    *   Modify `api/NikoNiko.Api/appsettings.json`: Add `"MAX_TEAMS_PER_USER": 2` under a configuration section.
*   **Verification:** Confirm the value exists in the configuration file.

### Step 2: Implement Team Limit Enforcement in Service (Backend) [x]
*   **Goal:** Ensure `CreateTeamAsync` counts the number of teams the user already admins.
*   **Action:**
    *   Modify `api/NikoNiko.Core/Interfaces/ITeamService.cs`: Add `bool isSuperAdmin` parameter to `CreateTeamAsync`.
    *   Modify `api/NikoNiko.Services/TeamService.cs`:
        *   Inject `IConfiguration`.
        *   In `CreateTeamAsync`, if `!isSuperAdmin`, count `_context.Teams.CountAsync(t => t.AdminId == adminId)`.
        *   If the count is >= limit, throw an `InvalidOperationException` with a clear message like "You have reached the maximum number of teams (2).".
*   **Verification:** The API project should compile after these changes.

### Step 3: Remove SuperAdmin restriction and handle exception in Controller (Backend) [x]
*   **Goal:** Allow all users to call the endpoint and handle the limit error.
*   **Action:**
    *   Modify `api/NikoNiko.Api/Controllers/TeamsController.cs`:
        *   Remove `[Authorize(Policy = "SuperAdmin")]` from `CreateTeam`.
        *   Pass `isSuperAdmin` claim to `_teamService.CreateTeamAsync`.
        *   Catch `InvalidOperationException` and return `BadRequest(ex.Message)`.
*   **Verification:** Rebuild the API project.

### Step 4: Add Integration Tests for the Limit (Backend) [x]
*   **Goal:** Verify the limit is respected for regular users and ignored for SuperAdmins.
*   **Action:**
    *   Modify `api/NikoNiko.Api.IntegrationTests/TeamsControllerTests.cs`:
        *   Change `CreateTeam_AsRegularUser_ReturnsForbidden` to `CreateTeam_AsRegularUser_ReturnsCreated`.
        *   Update the assertion from `HttpStatusCode.Forbidden` to `HttpStatusCode.Created`.
        *   Verify the created team properties.
*   **Verification:** Run the integration tests: `dotnet test api/NikoNiko.Api.IntegrationTests/NikoNiko.Api.IntegrationTests.csproj --filter FullyQualifiedName~TeamsControllerTests`.

### Step 5: Update Route Permissions (Frontend) [x]
*   **Goal:** Allow non-admins to access the `/admin/teams` route.
*   **Action:**
    *   Modify `app/frontend/src/App.tsx`: Remove `requiredAnyAdmin={true}` from the `ProtectedRoute` wrapping `AdminTeamsPage`.
*   **Verification:** Attempt to navigate to `/admin/teams` as a non-admin user.

### Step 6: Update Sidebar Visibility (Frontend) [x]
*   **Goal:** Ensure the "Admin" menu and "Teams" link are visible to all users.
*   **Action:**
    *   Modify `app/frontend/src/components/layout/Sidebar.tsx`:
        *   Update `showAdminMenu` logic to be `true` for any authenticated user (`const showAdminMenu = !!user;`).
        *   Ensure the "Teams" sub-item is visible without the `isSuperAdmin || isAnyTeamAdmin` check.
*   **Verification:** Log in as a non-admin user and check if the sidebar shows the "Admin" menu and "Teams" link.

### Step 7: Show Limit Message in AdminTeamsPage (Frontend) [>]
*   **Goal:** Inform the user when the limit is reached and hide the creation form.
*   **Action:**
    *   Modify `app/frontend/src/pages/AdminTeamsPage.tsx`:
        *   Count the number of teams the user is an admin of: `const adminedTeamsCount = teams?.filter(t => t.adminId === user?.sub).length || 0;`.
        *   If `adminedTeamsCount >= 2` and `!isSuperAdmin`, show an informative `Alert` message and hide `CreateTeamForm`.
*   **Verification:** Visit `/admin/teams` as a user with 2 teams and verify the message is shown.

### Verification [x]
*   **Goal:** Ensure the entire feature works correctly and is properly internationalized.
*   **Action:**
    *   Backend: Run integration tests (`TeamsControllerTests`).
    *   Frontend: Run `npm run build` to check for compilation errors.
    *   I18n: Add `adminTeams.quotaReached` to `en.json` and `fr.json`.
*   **Verification:** All tests passed, frontend build is successful, and translations are correctly applied.

## 4. 🧪 Testing Strategy
*   **Unit Tests:** N/A (Service logic covered by Integration tests).
*   **Integration Tests:**
    *   Run `TeamsControllerTests` to verify limit enforcement and SuperAdmin bypass.
*   **Manual Verification:**
    1.  Login as a user with no teams (e.g., a new invitee).
    2.  Create 2 teams successfully.
    3.  Try to create a 3rd team via the UI (form should be hidden) and verify the limit message.
    4.  Verify a SuperAdmin can create more than 2 teams.

## 5. ✅ Success Criteria
*   Regular users can create their own teams but are capped at 2 owned teams.
*   SuperAdmins are not subject to this cap.
*   The UI provides clear feedback to the user when the quota is reached.
*   Users invited to other teams are not penalized in their own creation quota.
