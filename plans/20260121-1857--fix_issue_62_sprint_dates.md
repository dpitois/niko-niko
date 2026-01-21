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
*   **Action:**
    *   Modify `api/NikoNiko.Services/SprintService.cs`:
        *   In `CreateSprintAsync`:
            Replace:
            ```csharp
            StartDate = createSprintDto.StartDate.ToUniversalTime(),
            EndDate = createSprintDto.EndDate.ToUniversalTime(),
            ```
            With:
            ```csharp
            StartDate = DateTime.SpecifyKind(createSprintDto.StartDate.Date, DateTimeKind.Utc),
            EndDate = DateTime.SpecifyKind(createSprintDto.EndDate.Date, DateTimeKind.Utc),
            ```
        *   In `UpdateSprintAsync`:
            Replace:
            ```csharp
            var newStart = updateSprintDto.StartDate.ToUniversalTime();
            var newEnd = updateSprintDto.EndDate.ToUniversalTime();
            ```
            With:
            ```csharp
            var newStart = DateTime.SpecifyKind(updateSprintDto.StartDate.Date, DateTimeKind.Utc);
            var newEnd = DateTime.SpecifyKind(updateSprintDto.EndDate.Date, DateTimeKind.Utc);
            ```
*   **Verification:** `dotnet test api/NikoNiko.Api.IntegrationTests`

### Step 2: Document Date Handling Standards
*   **Goal:** Prevent regression.
*   **Action:**
    *   Modify `GEMINI.md`.
    *   Add section:
        ```markdown
        ## Date Handling Standards
        - **Calendar Dates (Sprints, Birthdays):** Store as `DateTime` at Midnight UTC (`DateTime.SpecifyKind(date, DateTimeKind.Utc)`). Do NOT use `.ToUniversalTime()` as it shifts based on server time.
        - **Point-in-Time (Logs, Events):** Store as True UTC (`DateTime.UtcNow`).
        ```
*   **Verification:** Read file.

## 4. 🧪 Testing Strategy
*   **Integration Tests:** Verify `SprintsControllerTests` passes. Since tests might run on a UTC machine (CI), they might not catch the regression natively unless the test explicitly sets a non-UTC kind input.
*   **Manual Verification (Recommended):**
    1.  Create a Sprint for "2026-01-21".
    2.  Check DB/API response. It should be `...T00:00:00Z`.
    3.  If code used `ToUniversalTime()` on a UTC+1 machine, it would have been `Jan 20 23:00`.

## 5. ✅ Success Criteria
*   Sprint Start/End dates remain exactly as input by the user (Calendar Date preserved).
