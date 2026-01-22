# Implementation Plan - Fix Sprint Date Shift (Issue #62)

## 1. 🔍 Analysis & Context
*   **Objective:** Fix the issue where Sprint Dates are shifted by one day due to Timezone conversion on the server. Ensure that a date selected as "Jan 21" is stored as "Jan 21 00:00:00 UTC" regardless of the server's local timezone.
*   **Affected Files:**
    *   `api/NikoNiko.Services/SprintService.cs`
    *   `GEMINI.md`
*   **Key Dependencies:** .NET `DateTime`, `DateTimeKind`.
*   **Risks/Unknowns:** None. This is a standard handling for "Calendar Dates" (floating dates).

## 2. 📋 Checklist
- [x] Step 1: Fix `SprintService.cs` (Create & Update) to use `SpecifyKind` instead of `ToUniversalTime`.
- [x] Step 2: Update `GEMINI.md` with "Calendar Date" handling conventions.
- [x] Step 3: Fix `SprintService.cs` (GetSprints & GetSprintById) to force UTC Kind on return.
- [x] Step 4: Fix Frontend `AdminEditSprintDialog.tsx` to avoid timezone shift during date formatting.
- [ ] Verification: Run `SprintsController` tests.

## 3. 📝 Step-by-Step Implementation Details

### 🚀 Execution Rules
1. **Interactive Flow**: Execute ONLY ONE step at a time.
2. **Atomic Updates**: Before and after each step, you MUST use `replace` to update the checklist in section 2 and the status in section 3.
3. **Status Vocabulary**: Use `[x]` (Done), `[>]` (In Progress), `[-]` (Skipped/N.A.), `[!]` (Failed/Blocked).
4. **User Confirmation**: After updating the file, stop and wait for explicit user confirmation before proceeding to the next step.
5. **No Stealth Actions**: NEVER execute code or modify files without having first updated the plan to reflect that you are about to do so.

### Step 1: Fix `SprintService.cs` Date Handling
*   **Goal:** Ensure dates are stored as Midnight UTC without timezone shifting.
*   **Action:** (Completed)

### Step 2: Document Date Handling Standards
*   **Goal:** Prevent regression.
*   **Action:** (Completed)

### Step 3: Fix `SprintService.cs` Retrieval Date Handling
*   **Goal:** Ensure dates returned by the API have `DateTimeKind.Utc` explicitly set so JSON serialization includes the 'Z' suffix, preventing browser timezone interpretation issues.
*   **Action:**
    *   Modify `api/NikoNiko.Services/SprintService.cs`:
        *   In `GetSprintsAsync`: Retrieve data then project using `SpecifyKind`.
        *   In `GetSprintByIdAsync`: Retrieve data then project using `SpecifyKind`.
        *   Note: Since `SpecifyKind` cannot be translated to SQL, we must materialize the query first (e.g. to list) or do client-side evaluation.
*   **Verification:** Manual check of API response or Integration Test.

## 4. 🧪 Testing Strategy
*   **Integration Tests:** Verify `SprintsControllerTests` passes.
*   **Manual Verification:** Check API JSON output for 'Z' suffix (e.g., `2026-01-01T00:00:00Z`).

## 5. ✅ Success Criteria
*   Sprint Start/End dates remain exactly as input by the user (Calendar Date preserved).
*   Frontend receives explicit UTC dates.