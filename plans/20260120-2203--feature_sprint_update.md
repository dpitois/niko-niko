# Implementation Plan - Feature: Sprint Update with Date Validation

## 1. 🔍 Analysis & Context
*   **Objective:** Allow team admins to update sprint start/end dates. Ensure validation prevents excluding existing mood entries (Issue #33).
*   **Affected Files:**
    *   Backend: `api/NikoNiko.Core/DTOs/Sprint/UpdateSprintDto.cs` (New), `api/NikoNiko.Api/Controllers/SprintsController.cs`
    *   Frontend: `app/frontend/src/services/sprintService.ts`, `app/frontend/src/models/UpdateSprint.ts` (New), `app/frontend/src/pages/AdminSprintsPage.tsx`, `app/frontend/src/components/AdminEditSprintDialog.tsx` (New/Refactor)
*   **Key Dependencies:** `ApplicationDbContext` (for checking MoodEntries).
*   **Risks/Unknowns:** Timezone handling when comparing dates. Ensuring generic generic `Dialog` components are reusable or create a new one.

## 2. 📋 Checklist
- [x] Step 1: Create `UpdateSprintDto` in Backend.
- [x] Step 2: Implement `PUT` endpoint in `SprintsController` with validation logic.
- [x] Step 3: Create Integration Tests for Sprint Update validation.
- [x] Step 4: Update Frontend `Sprint` models and `sprintService`.
- [x] Step 5: Implement `AdminEditSprintDialog` component.
- [x] Step 6: Integrate Edit button in `AdminSprintsPage`.
- [x] Verification

## 3. 📝 Step-by-Step Implementation Details

### 🚀 Execution Rules
1. **Interactive Flow**: Execute ONLY ONE step at a time.
2. **Atomic Updates**: Before and after each step, you MUST use `replace` to update the checklist in section 2 and the status in section 3.
3. **Status Vocabulary**: Use `[x]` (Done), `[>]` (In Progress), `[-]` (Skipped/N.A.), `[!]` (Failed/Blocked).
4. **User Confirmation**: After updating the file, stop and wait for explicit user confirmation before proceeding to the next step.
5. **No Stealth Actions**: NEVER execute code or modify files without having first updated the plan to reflect that you are about to do so.

### Step 1: Create UpdateSprintDto
*   **Goal:** Define the data structure for updating a sprint.
*   **Status:** Done
*   **Action:**
    *   Create `api/NikoNiko.Core/DTOs/Sprint/UpdateSprintDto.cs`.
    *   Properties: `Name` (string), `StartDate` (DateTime), `EndDate` (DateTime).
*   **Verification:** File exists and compiles.

### Step 2: Implement PUT Endpoint in SprintsController
*   **Goal:** Add the API capability to update a sprint with strict validation.
*   **Status:** Done
*   **Action:**
    *   Modify `api/NikoNiko.Api/Controllers/SprintsController.cs`.
    *   Add `[HttpPut("{sprintId}")]` method.
    *   **Logic:**
        1.  Retrieve sprint by ID including `MoodEntries`.
        2.  Check permissions (Team Admin or Super Admin).
        3.  Check `EndDate > StartDate`.
        4.  **Crucial:** If `sprint.MoodEntries` is not empty:
            *   Get `minDate = sprint.MoodEntries.Min(m => m.Date)`
            *   Get `maxDate = sprint.MoodEntries.Max(m => m.Date)`
            *   Validate `dto.StartDate <= minDate` AND `dto.EndDate >= maxDate`.
            *   Return `BadRequest` if invalid.
        5.  Update fields and `SaveChangesAsync`.
*   **Verification:** Compile backend.

### Step 3: Integration Tests
*   **Goal:** Verify the validation rules automatically.
*   **Status:** Done
*   **Action:**
    *   Create `api/NikoNiko.Api.IntegrationTests/SprintsControllerUpdateTests.cs`.
    *   Test cases:
        *   `UpdateSprint_WithNoMoods_ShouldSuccess`
        *   `UpdateSprint_WithMoods_ValidDates_ShouldSuccess`
        *   `UpdateSprint_WithNoMoods_ShouldSuccess`
        *   `UpdateSprint_WithMoods_InvalidStartDate_ShouldFail`
        *   `UpdateSprint_WithMoods_InvalidEndDate_ShouldFail`
*   **Verification:** Run `dotnet test api/NikoNiko.Api.IntegrationTests`.

### Step 4: Frontend Service & Models
*   **Goal:** Prepare frontend data layer.
*   **Status:** Done
*   **Action:**
    *   Create `app/frontend/src/models/UpdateSprint.ts`.
    *   Update `app/frontend/src/services/sprintService.ts`: export `updateSprint(id: string, data: UpdateSprint)`.
*   **Verification:** check types.

### Step 5: AdminEditSprintDialog Component
*   **Goal:** UI for editing.
*   **Status:** Done
*   **Action:**
    *   Create `app/frontend/src/components/AdminEditSprintDialog.tsx`.
    *   Use `Dialog` from MUI.
    *   Inputs: Name, StartDate, EndDate (using standard date pickers or text fields as per existing Create form).
    *   Handle form submission -> call `updateSprint`.
    *   Display error message if API returns 400 (validation error).
*   **Verification:** Component renders.

### Step 6: Integrate into AdminSprintsPage
*   **Goal:** Connect the UI.
*   **Status:** Done
*   **Action:**
    *   Modify `app/frontend/src/pages/AdminSprintsPage.tsx`.
    *   Import `AdminEditSprintDialog`.
    *   Add "Edit" `IconButton` (using `EditIcon`) in the list item.
    *   Manage `selectedSprint` state to open the dialog.
    *   Refresh list on success.
*   **Verification:** Launch app and test flow.

### Step 7: Final Verification
*   **Goal:** Ensure overall quality.
*   **Status:** Done
*   **Action:**
    *   Run frontend build: `npm run build`.
    *   Run backend tests again.
*   **Verification:** All pass.
*   **Action:**
    *   Modify `app/frontend/src/pages/AdminSprintsPage.tsx`.
    *   Import `AdminEditSprintDialog`.
    *   Add "Edit" `IconButton` (using `EditIcon`) in the list item.
    *   Manage `selectedSprint` state to open the dialog.
    *   Refresh list on success.
*   **Verification:** Launch app and test flow.

## 4. 🧪 Testing Strategy
*   **Unit/Integration:** Backend integration tests are the source of truth for the logic.
*   **Manual:**
    1.  Login as Admin.
    2.  Create Sprint A (Empty). Edit dates -> OK.
    3.  Enter Mood for today in Sprint A.
    4.  Edit Sprint A: Try to set EndDate to yesterday -> Error.
    5.  Edit Sprint A: Extend EndDate -> OK.

## 5. ✅ Success Criteria
*   Backend rejects invalid date ranges when moods exist.
*   Frontend displays a clear error when modification is rejected.
*   Valid updates are persisted.