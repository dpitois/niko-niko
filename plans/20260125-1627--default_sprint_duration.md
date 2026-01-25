# Implementation Plan - Default Sprint Duration

## 1. 🔍 Analysis & Context
*   **Objective:** Allow Team Admins to configure a default sprint duration. This duration will be used to pre-fill the end date when creating a new sprint. Additionally, the new sprint's start date should default to the day after the last sprint's end date.
*   **Affected Files:**
    *   `api/NikoNiko.Core/Models/Team.cs`
    *   `api/NikoNiko.Core/DTOs/Team/TeamDto.cs`
    *   `api/NikoNiko.Core/DTOs/Team/UpdateTeamDto.cs`
    *   `api/NikoNiko.Core/DTOs/Team/CreateTeamDto.cs`
    *   `api/NikoNiko.Services/TeamService.cs`
    *   `app/frontend/src/models/Team.ts` (and related interfaces)
    *   `app/frontend/src/services/teamService.ts`
    *   `app/frontend/src/components/EditTeamDialog.tsx`
    *   `app/frontend/src/components/AdminCreateSprintForm.tsx`
    *   `app/frontend/src/components/CreateTeamForm.tsx`
*   **Key Dependencies:** Entity Framework Core (Migrations), React, Material UI, dayjs.
*   **Risks/Unknowns:** Ensuring date calculations account for timezones correctly (using `dayjs` and ISO strings should mitigate this). Ensuring `AdminCreateSprintForm` has access to the full team object with sprints.

## 2. 📋 Checklist
- [x] Step 1: Backend - Update Team Model & Create Migration
- [x] Step 2: Backend - Update DTOs & Service Logic (Mapping, Persistence, Validation)
- [x] Step 3: Frontend - Update Models & Team Service
- [x] Step 4: Frontend - Update Edit Team Dialog & Create Team Form
- [x] Step 5: Frontend - Implement Auto-fill in Sprint Creation
- [x] Verification

## 3. 📝 Step-by-Step Implementation Details

### 🚀 Execution Rules
1. **Interactive Flow**: Execute ONLY ONE step at a time.
2. **Atomic Updates**: Before and after each step, you MUST use `replace` to update the checklist in section 2 and the status in section 3.
3. **Status Vocabulary**: Use `[x]` (Done), `[>]` (In Progress), `[-]` (Skipped/N.A.), `[!]` (Failed/Blocked).
4. **User Confirmation**: After updating the file, stop and wait for explicit user confirmation before proceeding to the next step.
5. **No Stealth Actions**: NEVER execute code or modify files without having first updated the plan to reflect that you are about to do so.

### Step 1: Backend - Update Team Model & Create Migration
*   **Goal:** Add `DefaultSprintDuration` to the database schema.
*   **Action:**
    *   Modify `api/NikoNiko.Core/Models/Team.cs`: Added `public int? DefaultSprintDuration { get; set; }`.
    *   Run migration command: `dotnet ef migrations add AddDefaultSprintDurationToTeam --project ../NikoNiko.Data --startup-project .`.
*   **Verification:** Migration applied.

### Step 2: Backend - Update DTOs & Service Logic
*   **Goal:** Expose and allow updating of the new field via API.
*   **Action:**
    *   Modify `api/NikoNiko.Core/DTOs/Team/TeamDto.cs`: Added `DefaultSprintDuration`.
    *   Modify `api/NikoNiko.Core/DTOs/Team/UpdateTeamDto.cs`: Added `DefaultSprintDuration`.
    *   Modify `api/NikoNiko.Core/DTOs/Team/CreateTeamDto.cs`: Added `DefaultSprintDuration`.
    *   Modify `api/NikoNiko.Services/TeamService.cs`: Updated mapping (including `GetTeamByIdAsync` and `CreateTeamAsync`) and persistence.
    *   Modify `api/NikoNiko.Api/Validators/Team/*.cs`: Added validation (`> 0`).
*   **Verification:** Backend tests passed (60 tests).

### Step 3: Frontend - Update Models & Team Service
*   **Goal:** Update frontend types and service to handle the new field.
*   **Action:**
    *   Modify `app/frontend/src/models/Team.ts`: Added `defaultSprintDuration`.
    *   Modify `app/frontend/src/models/CreateTeam.ts`: Added `defaultSprintDuration`.
    *   Modify `app/frontend/src/services/teamService.ts`: Updated `updateTeam` signature.
*   **Verification:** Compilation passed.

### Step 4: Frontend - Update Edit Team Dialog & Create Team Form
*   **Goal:** Allow admins to set the default sprint duration.
*   **Action:**
    *   Modify `app/frontend/src/components/EditTeamDialog.tsx`: Added input field.
    *   Modify `app/frontend/src/components/CreateTeamForm.tsx`: Added input field.
    *   Updated `en.json` and `fr.json` translations (including validation errors).
*   **Verification:** Forms updated and localized.

### Step 5: Frontend - Implement Auto-fill in Sprint Creation
*   **Goal:** Auto-fill start and end dates when creating a sprint.
*   **Action:**
    *   Modify `app/frontend/src/components/AdminCreateSprintForm.tsx`: Implemented logic in `handleTeamChange`.
*   **Verification:** Build passed.

## 4. 🧪 Testing Strategy
*   **Manual Verification:**
    1.  Go to **Admin > Teams**.
    2.  Edit a team, set "Default Sprint Duration" to **14** days. Save.
    3.  Go to **Admin > Sprints**.
    4.  Click "Create Sprint".
    5.  Select the edited team.
    6.  **Check:** Start Date should be (Last Sprint End + 1 day) or Today.
    7.  **Check:** End Date should be (Start Date + 14 days).
    8.  Change the team back to one without default duration.
    9.  **Check:** End Date should not be auto-filled (or stick to previous logic).

## 5. ✅ Success Criteria
*   Team Admin can save a default sprint duration.
*   Creating a sprint automatically suggests logical Start and End dates based on history and configuration.
*   No regression in existing team editing or sprint creation.

# Status: Completed
All steps executed.
- Creation flow supported.
- Validation added (backend rules + frontend error display).
- Localization (EN/FR) completed.
- Mapping fixes in TeamService completed.
- Lint and Format passed for both Backend and Frontend.
- Backend integration tests passed.
- Frontend production build passed.