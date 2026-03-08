# Implementation Plan - Distinct User Removal vs Deletion

## 1. 🔍 Analysis & Context
*   **Objective:** Clearly distinguish between "removing a user from a team" (Team Admin) and "deleting a user from the database" (Super Admin / GDPR).
*   **Affected Files:**
    *   **Backend:**
        *   `api/NikoNiko.Api/Controllers/TeamsController.cs`
        *   `api/NikoNiko.Api/Controllers/UsersController.cs`
        *   `api/NikoNiko.Services/TeamService.cs`
        *   `api/NikoNiko.Services/UserService.cs`
    *   **Frontend:**
        *   `app/frontend/src/components/TeamMemberList.tsx` (to be identified)
        *   `app/frontend/src/i18n/locales/en.json` & `fr.json`
*   **Key Dependencies:** Authorization Policies (Backend), Material UI (Frontend).
*   **Risks/Unknowns:** Permissions overlap in older implementations; need to check if Team Admins are mistakenly granted Super Admin scopes.

## 2. 📋 Checklist
- [x] Step 1: Secure API Delete Endpoints
- [x] Step 2: Implement Team User Removal Service Logic
- [x] Step 3: Restrict Global Users View & Actions
- [x] Step 4: Update Frontend Wording & UI Actions
- [x] Step 5: Add Permissions & Data Integrity Tests
- [x] Verification: End-to-end check

## 3. 📝 Step-by-Step Implementation Details

### 🚀 Execution Rules
1. **Interactive Flow**: Execute ONLY ONE step at a time.
2. **Atomic Updates**: Before and after each step, you MUST use `replace` to update the checklist in section 2 and the status in section 3.
3. **Status Vocabulary**: Use `[x]` (Done), `[>]` (In Progress), `[-]` (Skipped/N.A.), `[!]` (Failed/Blocked).
4. **User Confirmation**: After updating the file, stop and wait for explicit user confirmation before proceeding to the next step.
5. **No Stealth Actions**: NEVER execute code or modify files without having first updated the plan to reflect that you are about to do so.

### Step 1: Secure API Delete Endpoints [x]
*   **Goal:** Restrict global user deletion to Super Admins only.
*   **Action:**
    *   Verified `UsersController.cs`: `DeleteUser` is decorated with `[Authorize(Policy = "SuperAdmin")]`.
    *   Verified `Program.cs`: `SuperAdmin` policy requires `is_super_admin` claim.
*   **Verification:** `dotnet test`.

### Step 2: Implement Team User Removal Service Logic [x]
*   **Goal:** Provide an endpoint to remove a user from a team without deleting them.
*   **Action:**
    *   Verified `TeamsController.RemoveUserFromTeam`: Exists and is protected by `IsTeamAdmin`.
    *   Verified `TeamService.RemoveUserFromTeamAsync`: Correctly removes relationship and handles mood deletion.
*   **Verification:** Manual check using a REST client or unit test.

### Step 3: Restrict Global Users View & Actions [x]
*   **Goal:** Prevent Team Admins from accessing global deletion via the Users view.
*   **Action:**
    *   Modified `AdminUsersPage.tsx`: Added `isSuperAdmin` check from `useAuth()`.
    *   Wrapped global delete `IconButton` with `{isSuperAdmin && ...}`.
*   **Verification:** Login as Team Admin and confirm the Delete button is gone from the Users list.

### Step 4: Update Frontend Wording & UI Actions [x]
*   **Goal:** Clarify UI actions for Team Admins in the Team view.
*   **Action:**
    *   Modified `AdminTeamListItem.tsx`: Replaced `DeleteIcon` with `PersonRemoveIcon` for team member removal.
    *   Updated `aria-label` to "remove member".
*   **Verification:** Visual check in the browser.

### Step 5: Add Permissions & Data Integrity Tests [x]
*   **Goal:** Prevent regressions and ensure user persistence.
*   **Action:**
    *   Verified existing tests in `TeamsControllerRemoveUserTests.cs`.
    *   Ran full backend test suite (80 tests).
*   **Verification:** `dotnet test`.

## 4. 🧪 Testing Strategy
*   **Unit Tests:** Verify `TeamService` logic.
*   **Integration Tests:**
    *   Success: Team Admin removes member (member exists globally).
    *   Failure: Team Admin tries global deletion (403).
    *   Success: Super Admin deletes user (user removed globally).
*   **Manual Verification:** Walkthrough as a Team Admin and as a Super Admin.

## 5. ✅ Success Criteria
*   Team Admins can only remove users from their teams.
*   User deletion is restricted to Super Admins and the user themselves (GDPR).
*   UI labels are distinct and explicit about the action's scope.

## 4. 🧪 Testing Strategy
*   **Unit Tests:** Verify `TeamService` logic.
*   **Integration Tests:**
    *   Success: Team Admin removes member (member exists globally).
    *   Failure: Team Admin tries global deletion (403).
    *   Success: Super Admin deletes user (user removed globally).
*   **Manual Verification:** Walkthrough as a Team Admin and as a Super Admin.

## 5. ✅ Success Criteria
*   Team Admins can only remove users from their teams.
*   User deletion is restricted to Super Admins and the user themselves (GDPR).
*   UI labels are distinct and explicit about the action's scope.
