# Implementation Plan - Handle Null Email/Name in JWT & Fix Null Warnings

## 1. 🔍 Analysis & Context
*   **Objective:** 
    1. Fix `ArgumentNullException` in `TokenService` when generating tokens for users with null Email or Name.
    2. Address all .NET compiler warnings (`CS8601`, `CS8604`, etc.) related to potential null values, ensuring the codebase correctly handles the optional nature of `Email`.
*   **Issue:** Crash during Discord login (null email) and multiple build warnings in Controllers.
*   **Affected Files:** 
    *   `api/NikoNiko.Services/TokenService.cs`
    *   `api/NikoNiko.Api/Controllers/UsersController.cs`
    *   `api/NikoNiko.Api/Controllers/TeamsController.cs`
    *   `api/NikoNiko.Api/Controllers/MoodEntriesController.cs` (to verify)

## 2. 📋 Checklist
- [x] Step 1: Update `TokenService.cs` to conditionally add claims (Fixes Crash).
- [x] Step 2: Fix Null Reference Warnings in API Controllers (Fixes Build Warnings).
- [x] Step 3: Create a unit/integration test to verify the token generation fix.

## 3. 📝 Step-by-Step Implementation Details

### 🚀 Execution Rules
1. **Interactive Flow**: Execute ONLY ONE step at a time.
2. **Atomic Updates**: Before and after each step, you MUST use `replace` to update the checklist in section 2 and the status in section 3.
3. **Status Vocabulary**: Use `[x]` (Done), `[>]` (In Progress), `[-]` (Skipped/N.A.), `[!]` (Failed/Blocked).
4. **User Confirmation**: After updating the file, stop and wait for explicit user confirmation before proceeding to the next step.

### Step 1: Update TokenService
*   **Goal:** Modify `CreateToken` to handle null user properties safely.
*   **Status:** [x] Done
*   **Action:**
    *   In `api/NikoNiko.Services/TokenService.cs`:
    *   Initialize the claims list with just the `Sub` claim.
    *   Add `Email` claim only `if (!string.IsNullOrEmpty(user.Email))`.
    *   Add `Name` claim only `if (!string.IsNullOrEmpty(user.Name))`.
*   **Code Snippet:**
    ```csharp
    var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString())
    };

    if (!string.IsNullOrEmpty(user.Email))
    {
        claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
    }

    if (!string.IsNullOrEmpty(user.Name))
    {
        claims.Add(new Claim(JwtRegisteredClaimNames.Name, user.Name));
    }
    ```

### Step 2: Fix Null Warnings in Controllers
*   **Goal:** Resolve `CS8601` (Possible null reference assignment) and related warnings.
*   **Status:** [x] Done
*   **Action:**
    *   Run `dotnet build` to list current warnings.
    *   **UsersController.cs**: Fix assignments where `User.Email` or `User.Name` might be null. Use `?? ""` or appropriate handling for DTOs.
    *   **TeamsController.cs**: Fix `TeamUser` mapping where `User.Email` is accessed.
    *   Ensure DTOs (`UserDto`, `TeamUserDto`) allow nulls where appropriate, or provide fallback values in the mapping.
*   **Verification:** Run `dotnet build` and ensure 0 warnings in `NikoNiko.Api`.

### Step 3: Add Regression Test
*   **Goal:** Ensure `CreateToken` does not throw for partial user data.
*   **Status:** [x] Done
*   **Action:**
    *   Create `api/NikoNiko.Api.IntegrationTests/TokenServiceTests.cs`.
    *   Test case: `CreateToken_ShouldNotThrow_WhenEmailOrNameIsNull`.
*   **Verification:** Run `dotnet test`.

### Step 4: Fix Null Warnings in Tests
*   **Goal:** Resolve `CS8602` (Dereference of a possibly null reference) in Integration Tests.
*   **Status:** [x] Done
*   **Action:**
    *   Added `Assert.NotNull(...)` checks in `TeamsControllerAdminTransferTests.cs` and `MoodEntriesControllerTests.cs` before accessing nullable properties of objects deserialized from JSON or fetched from DB.
*   **Verification:** Run `dotnet test` (Passed with 0 warnings).

## 4. 🧪 Testing Strategy
*   **Unit/Integration Test:** Execute the new test case.
*   **Build Verification:** `dotnet build` must be clean (0 warnings).
*   **Manual Verification:** Retry login flow with Discord user (no email).

## 5. ✅ Success Criteria
*   Login works for users without email.
*   Backend compiles without warnings.