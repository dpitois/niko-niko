# Implementation Plan - Fix Sprint Overlap Validation (Backend & Frontend)

## 1. 🔍 Analysis & Context
*   **Objective:** Prevent sprint date overlaps for the same team during both creation and update operations on both backend and frontend.
*   **Affected Files:**
    *   **Backend:**
        *   `api/NikoNiko.Services/SprintService.cs` (Validation logic)
        *   `api/NikoNiko.Api/Controllers/SprintsController.cs` (Error handling)
        *   `api/NikoNiko.Services.UnitTests/SprintServiceTests.cs` (Unit tests)
    *   **Frontend:**
        *   `app/frontend/src/i18n/locales/en.json` & `fr.json` (Error messages)
        *   `app/frontend/src/components/AdminCreateSprintForm.tsx` (Validation UI)
        *   `app/frontend/src/components/AdminEditSprintDialog.tsx` (Validation UI)
        *   `app/frontend/src/pages/AdminSprintsPage.tsx` (Data passing)
*   **Key Dependencies:** Entity Framework Core (Backend), dayjs (Frontend).
*   **Risks/Unknowns:** Existing data in the database might already overlap, potentially blocking updates to those sprints unless fixed.

## 2. 📋 Checklist
- [x] Step 1: Add backend regression tests
- [x] Step 2: Implement centralized backend overlap validation
- [x] Step 3: Update SprintsController for 400 responses
- [x] Step 4: Add frontend i18n keys for overlap error
- [x] Step 5: Implement frontend validation in Create Form
- [x] Step 6: Implement frontend validation in Edit Dialog
- [x] Step 7: Verification

## 3. 📝 Step-by-Step Implementation Details
*Note: Be extremely specific. Include file paths and code snippets/signatures.*

### 🚀 Execution Rules
1. **Interactive Flow**: Execute ONLY ONE step at a time.
2. **Atomic Updates**: Before and after each step, you MUST use `replace` to update the checklist in section 2 and the status in section 3.
3. **Status Vocabulary**: Use `[x]` (Done), `[>]` (In Progress), `[-]` (Skipped/N.A.), `[!]` (Failed/Blocked).
4. **User Confirmation**: After updating the file, stop and wait for explicit user confirmation before proceeding to the next step.
5. **No Stealth Actions**: NEVER execute code or modify files without having first updated the plan to reflect that you are about to do so.

### Step 1: Add backend regression tests [x]
*   **Goal:** Define failure cases for overlapping and touching sprints.
*   **Action:**
    *   Modify `api/NikoNiko.Services.UnitTests/SprintServiceTests.cs`.
    *   Add `UpdateSprintAsync_ShouldThrowInvalidOperation_WhenDatesOverlapWithAnotherSprint`.
    *   Add `CreateSprintAsync_ShouldThrowInvalidOperation_WhenDatesTouch` (end date of existing == start date of new).
*   **Verification:** `dotnet test` should fail.

### Step 2: Implement centralized backend overlap validation [x]
*   **Goal:** Ensure consistent, inclusive overlap detection.
*   **Action:**
    *   In `api/NikoNiko.Services/SprintService.cs`, add private method `IsOverlapAsync(Guid teamId, DateTime start, DateTime end, Guid? excludeSprintId = null)`.
    *   Logic: `_context.Sprints.AnyAsync(s => s.TeamId == teamId && (excludeSprintId == null || s.Id != excludeSprintId) && s.StartDate.Date <= end.Date && start.Date <= s.EndDate.Date)`.
    *   Refactor `CreateSprintAsync` and `UpdateSprintAsync` to use this method.
*   **Verification:** `dotnet test` should pass.

### Step 3: Update SprintsController for 400 responses [x]
*   **Goal:** Provide meaningful API responses for overlaps.
*   **Action:**
    *   In `api/NikoNiko.Api/Controllers/SprintsController.cs`, catch `InvalidOperationException` in `CreateSprint` and return `BadRequest`.
    *   Ensure `UpdateSprint` also returns the correct error mapping for `ModelState`.
*   **Verification:** Manual API call (Swagger/Postman) with overlapping dates.

### Step 4: Add frontend i18n keys for overlap error [x]
*   **Goal:** Prepare localized messages for the UI.
*   **Action:**
    *   Modify `app/frontend/src/i18n/locales/en.json` and `fr.json`.
    *   Add `errorOverlap` to `adminSprints.createForm` and `adminSprints.editDialog`.
*   **Verification:** Inspect JSON files.

### Step 5: Implement frontend validation in Create Form [x]
*   **Goal:** Block overlap before calling the API in the creation form.
*   **Action:**
    *   Modify `app/frontend/src/components/AdminCreateSprintForm.tsx`.
    *   In `handleSubmit`, find the selected team's sprints and check for overlaps using `dayjs`.
    *   Show `errorOverlap` if a conflict is found.
*   **Verification:** Try to create an overlapping sprint in the UI.

### Step 6: Implement frontend validation in Edit Dialog [x]
*   **Goal:** Block overlap before calling the API in the edit dialog.
*   **Action:**
    *   Modify `app/frontend/src/pages/AdminSprintsPage.tsx` to pass all `sprints` to the edit dialog.
    *   Modify `app/frontend/src/components/AdminEditSprintDialog.tsx` to receive `allSprints: Sprint[]` prop.
    *   In `handleSubmit`, check for overlaps with other sprints of the same team.
*   **Verification:** Try to edit a sprint to overlap another one in the UI.

### Step 7: Verification [x]
*   **Goal:** Ensure the entire system (backend and frontend) is robust against sprint overlaps.
*   **Action:**
    *   Run all backend unit and integration tests.
    *   Perform a frontend build to verify type safety.
*   **Verification:** All tests pass and the logic is sound.

## 4. 🧪 Testing Strategy
*   **Unit Tests:** Verify all overlap scenarios in `SprintServiceTests.cs`.
*   **Frontend Manual Testing:** 
    1. Create a sprint [Mar 1 - Mar 14].
    2. Try creating another for the same team [Mar 14 - Mar 20] -> Fail.
    3. Try creating another [Mar 15 - Mar 28] -> Success.
    4. Try editing the second to start on [Mar 14] -> Fail.
*   **Integration Tests:** Run existing controller tests.

## 5. ✅ Success Criteria
*   No overlapping sprints can be created or updated (inclusive check).
*   Users receive immediate feedback in the frontend before the API call.
*   API returns 400 with a clear message if reached (double security).
*   Existing tests pass.
