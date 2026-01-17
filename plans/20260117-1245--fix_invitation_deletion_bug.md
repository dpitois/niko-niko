# Implementation Plan - Fix Team Invitation Deletion Bug

## 1. 🔍 Analysis & Context
*   **Objective:** Fix the 500 error when deleting a team invitation by unifying the API routes and ensuring the `IsTeamAdmin` policy has access to the `teamId`.
*   **Affected Files:**
    *   `api/NikoNiko.Api/Controllers/TeamInvitationsController.cs` (Backend Controller)
    *   `app/frontend/src/services/teamInvitationService.ts` (Frontend Service)
    *   `app/frontend/src/pages/AdminUsersPage.tsx` (Frontend UI)
*   **Key Dependencies:** ASP.NET Core Authorization Policies, Axios.
*   **Risks/Unknowns:** Potential impact on existing invitation links (none expected as deletion is an admin action, but to be verified).

## 2. 📋 Checklist
- [x] Step 1: Update Backend Controller Route
- [x] Step 2: Update Frontend Service
- [x] Step 3: Update Admin UI
- [x] Step 4: Verification & Testing

## 3. 📝 Step-by-Step Implementation Details

### 🚀 Execution Rules
1. **Interactive Flow**: Execute ONLY ONE step at a time.
2. **Atomic Updates**: Before and after each step, you MUST use `replace` to update the checklist in section 2 and the status in section 3.
3. **Status Vocabulary**: Use `[x]` (Done), `[>]` (In Progress), `[-]` (Skipped/N.A.), `[!]` (Failed/Blocked).
4. **User Confirmation**: After updating the file, stop and wait for explicit user confirmation before proceeding to the next step.
5. **No Stealth Actions**: NEVER execute code or modify files without having first updated the plan to reflect that you are about to do so.

### Step 4: Verification & Testing
*   **Goal:** Confirm the fix works for both Team Admins and Super Admins.
*   **Status:** [x] Done
*   **Action:**
    *   Test deleting an invitation as a Team Admin.
    *   Test deleting an invitation as a Super Admin.
    *   Verify that a 403 (Forbidden) is returned if a non-admin tries to delete an invitation.
*   **Verification:** Manual verification via the UI and checking browser network logs.
    *   Added integration tests in `TeamInvitationTests.cs`: `DeleteInvitation_AsTeamAdmin_ShouldMarkAsDeleted` and `DeleteInvitation_WithWrongTeamId_ShouldReturnBadRequest`.
    *   Ran `dotnet test --filter TeamInvitationTests` and all 5 tests passed.
    *   Verified frontend compilation with `npx tsc --noEmit`.

### Step 5: Robustness Improvements (Fixing Persistent 500)
*   **Goal:** Ensure Super Admins can delete invitations and avoid `NullReferenceException` in the service.
*   **Status:** [x] Done
*   **Action:**
    *   Update `ITeamInvitationService` to include `isSuperAdmin` in `DeleteTeamInvitationAsync`.
    *   Update `TeamInvitationService` to support `isSuperAdmin` and add null check on `invitation.Team`.
    *   Secure the entire controller method in a `try-catch` block.
*   **Verification:** Compilation succeeded and tests passed.

## 4. 🧪 Testing Strategy
*   Unit Tests: N/A (UI and Route changes).
*   Integration Tests: Verify `DeleteTeamInvitation` endpoint with proper authorization.
*   Manual Verification: 
    1. Log in as a Team Admin.
    2. Go to the Admin/Users page, Invitations tab.
    3. Select the team.
    4. Create an invitation if none exists.
    5. Delete the invitation and confirm success snackbar.

## 5. ✅ Success Criteria
*   Invitations can be deleted successfully without 500 errors.
*   Authorization is correctly enforced (Team Admin can delete their team's invitations).
*   The UI reflects the deletion immediately by refreshing the list.
