# Implementation Plan - Sprint Maximum Duration Validation (Calendar Months)

## 1. 🔍 Analysis & Context
*   **Objective:** Limit sprint duration to a maximum of 2 calendar months using robust date logic (e.g., `AddMonths(2)`).
*   **Affected Files:**
    *   **Backend:**
        *   `api/NikoNiko.Api/Validators/Sprint/CreateSprintDtoValidator.cs`
        *   `api/NikoNiko.Api/Validators/Sprint/UpdateSprintDtoValidator.cs`
        *   `api/NikoNiko.Api.IntegrationTests/SprintsControllerValidationTests.cs`
    *   **Frontend:**
        *   `app/frontend/src/components/AdminCreateSprintForm.tsx`
        *   `app/frontend/src/components/AdminEditSprintDialog.tsx`
*   **Key Dependencies:** FluentValidation (Backend), dayjs (Frontend).
*   **Risks/Unknowns:** Differences in edge cases (e.g. Feb 29) between dayjs and .NET.

## 2. 📋 Checklist
- [x] Step 1: Update Backend Validators to use `AddMonths(2)`
- [x] Step 2: Update Frontend Forms to use `.add(2, 'month')`
- [x] Step 3: Update and add tests for calendar month boundaries
- [x] Verification: Final system check

## 3. 📝 Step-by-Step Implementation Details

### 🚀 Execution Rules
1. **Interactive Flow**: Execute ONLY ONE step at a time.
2. **Atomic Updates**: Before and after each step, you MUST use `replace` to update the checklist in section 2 and the status in section 3.
3. **Status Vocabulary**: Use `[x]` (Done), `[>]` (In Progress), `[-]` (Skipped/N.A.), `[!]` (Failed/Blocked).
4. **User Confirmation**: After updating the file, stop and wait for explicit user confirmation before proceeding to the next step.
5. **No Stealth Actions**: NEVER execute code or modify files without having first updated the plan to reflect that you are about to do so.

### Step 1: Update Backend Validators to use calendar months [x]
*   **Goal:** Switch from day counting to `AddMonths(2)` logic.
*   **Action:**
    *   Modify `CreateSprintDtoValidator.cs` and `UpdateSprintDtoValidator.cs`.
    *   Replace fixed 62 days check with `x.EndDate <= x.StartDate.AddMonths(2)`.
*   **Verification:** `dotnet test` (expected failures in integration tests).

### Step 2: Update Frontend Forms to use calendar months [x]
*   **Goal:** Align UI validation with Backend logic.
*   **Action:**
    *   In `AdminCreateSprintForm.tsx` and `AdminEditSprintDialog.tsx`, replace `.diff(..., 'day') > 62` with `.isAfter(dayjs(startDate).add(2, 'month'))`.
*   **Verification:** Manual check in UI with Dec 1st -> Feb 1st (should pass).

### Step 3: Update and add tests for calendar month boundaries [x]
*   **Goal:** Validate the new logic with specific dates (Dec-Jan vs Jan-Mar).
*   **Action:**
    *   Modify `SprintsControllerValidationTests.cs`.
    *   Update `CreateSprint_WithDurationTooLong_ReturnsBadRequest` to use a truly invalid date (e.g., Jan 1 -> April 1).
    *   Add a test case for a valid "long" 2-month period (e.g., Dec 1 -> Jan 31).
*   **Verification:** `dotnet test`.

## 4. 🧪 Testing Strategy
*   **Unit Tests:** Verify that Dec 1st to Jan 31st (62 days) is now accepted.
*   **Manual Verification:** Check that the error message `validation.sprintTooLong` still appears for 3-month durations.

## 5. ✅ Success Criteria
*   Sprints spanning exactly 2 calendar months (even if > 60 days) are accepted.
*   Sprints exceeding 2 calendar months are rejected.
*   Validation is consistent between Backend and Frontend.
