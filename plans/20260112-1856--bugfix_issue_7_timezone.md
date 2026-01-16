# Implementation Plan - Fix Mood Entry Timezone Issue (with Day.js)

## 1. 🔍 Analysis & Context
*   **Objective:** Validate mood entry dates based on the user's *local* time, not the server's UTC time. Allow "tomorrow" UTC if it is "today" for the user.
*   **Affected Files:**
    *   `api/NikoNiko.Core/DTOs/Mood/CreateMoodEntryDto.cs`
    *   `api/NikoNiko.Api/Controllers/MoodEntriesController.cs`
    *   `app/frontend/src/services/moodService.ts`
    *   `api/NikoNiko.Api.IntegrationTests/MoodEntriesControllerTests.cs`
*   **Key Dependencies:** `dayjs` (Frontend), .NET `DateTime` (Backend).
*   **Risks/Unknowns:** Frontend testing infrastructure might be missing.

## 2. 📋 Checklist
- [ ] Step 0: Create Git Branch `fix/issue-7-timezone`
- [ ] Step 1: Update Backend DTO and Controller logic.
- [ ] Step 2: Update Frontend Service to send `dayjs().utcOffset()`.
- [ ] Step 3: Add Backend Integration Tests.
- [ ] Step 4: Frontend Tests (Feasibility Check).
- [ ] Verification: Run tests.

## 3. 📝 Step-by-Step Implementation Details

### Step 0: Create Git Branch
*   **Goal:** Isolate changes.
*   **Action:**
    *   `git checkout -b fix/issue-7-timezone`

### Step 1: Backend - Timezone Aware Validation
*   **Goal:** Enable server to calculate user's local date.
*   **Action:**
    *   Modify `api/NikoNiko.Core/DTOs/Mood/CreateMoodEntryDto.cs`:
        *   Add `public int TimezoneOffset { get; set; }`
    *   Modify `api/NikoNiko.Api/Controllers/MoodEntriesController.cs`:
        *   In `CreateMoodEntry`, calculate `userLocalNow = DateTime.UtcNow.AddMinutes(dto.TimezoneOffset)`.
        *   Validate: `if (entryDate > userLocalNow.Date) return BadRequest(...)`

### Step 2: Frontend - Send Timezone Offset
*   **Goal:** Pass client context.
*   **Action:**
    *   Modify `app/frontend/src/services/moodService.ts`:
        *   Import `dayjs`.
        *   In `createMoodEntry`, add `timezoneOffset: dayjs().utcOffset()` to payload.

### Step 3: Backend Integration Tests
*   **Goal:** Verify strict boundary conditions.
*   **Action:**
    *   Modify `api/NikoNiko.Api.IntegrationTests/MoodEntriesControllerTests.cs`.
    *   Add `CreateMoodEntry_AheadOfUtc_Allowed()` (Simulate Tokyo UTC+9).
    *   Add `CreateMoodEntry_BehindUtc_Blocked()` (Simulate New York UTC-5).

### Step 4: Frontend Tests (Feasibility Check)
*   **Goal:** Verify frontend logic if possible.
*   **Action:**
    *   Check `app/frontend/package.json` for test scripts (e.g., `vitest`, `jest`).
    *   **Condition:**
        *   If **present**: Add a unit test for `moodService` to ensure offset is sent.
        *   If **absent**: **SKIP** to avoid setting up a full test environment for a hotfix.

## 4. 🧪 Testing Strategy
*   **Backend:** Integration tests in Step 3.
*   **Frontend:** Conditional unit test (Step 4).
*   **Manual:** Verify network payload contains correct `timezoneOffset`.

## 5. ✅ Success Criteria
*   Backend correctly interprets `TimezoneOffset`.
*   Users can submit mood entries up to their local midnight.