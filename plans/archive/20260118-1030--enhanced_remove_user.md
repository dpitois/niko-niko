# Implementation Plan - Enhanced Remove User from Team (Backend & Frontend)

## 1. 🔍 Analysis & Context
*   **Objective:** Enhance `RemoveUserFromTeam` to cascade delete mood entries for that team and auto-create a default team if the user becomes an orphan. Expose this functionality in the Frontend UI.
*   **Affected Files:**
    *   **Backend:**
        *   `api/NikoNiko.Services/TeamService.cs` (New)
        *   `api/NikoNiko.Services/ITeamService.cs` (New)
        *   `api/NikoNiko.Api/Program.cs` (DI Registration)
        *   `api/NikoNiko.Api/Controllers/AuthController.cs` (Refactor)
        *   `api/NikoNiko.Api/Controllers/TeamsController.cs` (Update logic)
    *   **Frontend:**
        *   `app/frontend/src/services/teamService.ts` (Add API call)
        *   `app/frontend/src/components/AdminTeamListItem.tsx` (Add UI button)
        *   `app/frontend/src/i18n/locales/fr.json` & `en.json` (Add translations)
*   **Key Dependencies:** `ApplicationDbContext`, `ITeamService`, `useTeams` (SWR).
*   **Risks/Unknowns:** Ensure transactional integrity and proper UI refresh after member removal.

## 2. 📋 Checklist
- [x] Step 1: Create `TeamService` and extract default team logic
- [x] Step 2: Refactor `AuthController` to use `TeamService`
- [x] Step 3: Implement Enhanced `RemoveUserFromTeam` logic
- [x] Step 4: Create Integration Tests for Backend
- [x] Step 5: Update Frontend Service (`teamService.ts`)
- [x] Step 6: Add Remove Member UI in `AdminTeamListItem`
- [x] Verification

## 3. 📝 Step-by-Step Implementation Details

### 🚀 Execution Rules
1. **Interactive Flow**: Execute ONLY ONE step at a time.
2. **Atomic Updates**: Before and after each step, you MUST use `replace` to update the checklist in section 2 and the status in section 3.
3. **Status Vocabulary**: Use `[x]` (Done), `[>]` (In Progress), `[-]` (Skipped/N.A.), `[!]` (Failed/Blocked).
4. **User Confirmation**: After updating the file, stop and wait for explicit user confirmation before proceeding to the next step.
5. **No Stealth Actions**: NEVER execute code or modify files without having first updated the plan to reflect that you are about to do so.

### Step 1: Create `TeamService` and extract default team logic
*   **Status:** [x] Done (Implemented in previous turns)

### Step 2: Refactor `AuthController` to use `TeamService`
*   **Status:** [x] Done (Implemented in previous turns)

### Step 3: Implement Enhanced `RemoveUserFromTeam` logic
*   **Status:** [x] Done (Implemented in previous turns)

### Step 4: Create Integration Tests for Backend
*   **Goal:** Verify mood deletion and orphan handling.
*   **Status:** [x] Done (Implemented and Verified)

### Step 5: Update Frontend Service (`teamService.ts`)
*   **Goal:** Add the API call to remove a user from a team.
*   **Status:** [x] Done
*   **Action:**
    *   Add `export const removeUserFromTeam = async (teamId: string, userId: string): Promise<void> => { ... };` to `app/frontend/src/services/teamService.ts`.
*   **Verification:** Build check.

### Step 6: Add Remove Member UI in `AdminTeamListItem`
*   **Goal:** Provide a button to remove members from the team list.
*   **Status:** [x] Done
*   **Action:**
    *   Modify `app/frontend/src/components/AdminTeamListItem.tsx`:
        *   Add `handleRemoveMember` function.
        *   Add `secondaryAction` to the `ListItem` in the members loop.
        *   The action should be an `IconButton` with `DeleteIcon`.
        *   **Condition**: `member.id !== team.adminId`.
    *   Update `i18n` files with:
        ```json
        "adminTeams": {
          "removeMemberConfirm": "Êtes-vous sûr de vouloir retirer ce membre de l'équipe ?",
          "removeMemberSuccess": "Membre retiré avec succès.",
          "removeMemberError": "Échec du retrait du membre."
        }
        ```
*   **Verification:** Manual test in UI.

## 4. 🧪 Testing Strategy
*   **Backend:** Integration tests for orphan and non-orphan cases.
*   **Frontend:** Verify button visibility and SWR list refresh.

## 5. ✅ Success Criteria
*   Removing a user cleans up their mood history for that specific team.
*   Orphaned users automatically get a new default team.
*   Team admins can easily remove members via the UI.