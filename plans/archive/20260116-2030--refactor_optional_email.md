# Implementation Plan - Refactor: Optional Email

## 1. 🔍 Analysis & Context
*   **Objective:** Remove the strict dependency on user email addresses, allowing authentication via providers that do not supply an email (e.g., Discord without email scope) and decoupling user identity from email.
*   **Affected Files:**
    *   `api/NikoNiko.Core/Models/User.cs`
    *   `api/NikoNiko.Data/Migrations/*` (New Migration)
    *   `api/NikoNiko.Api/Controllers/AuthController.cs`
    *   `api/NikoNiko.Api/Program.cs`
    *   `app/frontend/src/models/User.ts` (Frontend types)
    *   `app/frontend/src/pages/admin/AdminUsersPage.tsx` (UI display)
*   **Key Dependencies:** Entity Framework Core (Migrations), OAuth Providers configuration.
*   **Risks/Unknowns:**
    *   Unique constraints on the Email column in the database might need adjustment to allow multiple NULLs (standard in Postgres/SQLite but critical to verify).
    *   Ensure existing users are not affected.
    *   Super Admin logic relies on email matching from env vars; users without email cannot be auto-promoted via this specific method (acceptable limitation).

## 2. 📋 Checklist
- [x] Step 1: Update Data Model (Make Email Nullable)
- [x] Step 2: Create and Apply Database Migrations
- [x] Step 3: Update Authentication Logic (AuthController)
- [x] Step 4: Update Frontend Models and UI
- [x] Step 5: Remove Email Scope from OAuth Providers
- [x] Verification

## 3. 📝 Step-by-Step Implementation Details

### 🚀 Execution Rules
1. **Interactive Flow**: Execute ONLY ONE step at a time.
2. **Atomic Updates**: Before and after each step, you MUST use `replace` to update the checklist in section 2 and the status in section 3.
3. **Status Vocabulary**: Use `[x]` (Done), `[>]` (In Progress), `[-]` (Skipped/N.A.), `[!]` (Failed/Blocked).
4. **User Confirmation**: After updating the file, stop and wait for explicit user confirmation before proceeding to the next step.
5. **No Stealth Actions**: NEVER execute code or modify files without having first updated the plan to reflect that you are about to do so.

### Step 1: Update Data Model (Make Email Nullable)
*   **Goal:** Update the `User` entity to allow null values for the `Email` property.
*   **Status:** [x]
*   **Action:**
    *   Modify `api/NikoNiko.Core/Models/User.cs`: Change `public string Email { get; set; }` to `public string? Email { get; set; }`.
    *   Review `api/NikoNiko.Data/ApplicationDbContext.cs` or entity configurations to ensure `IsRequired(false)` is applied or inferred correctly for Email.
*   **Verification:** Code compiles.

### Step 2: Create and Apply Database Migrations
*   **Goal:** Reflect the schema change in the database.
*   **Status:** [x]
*   **Action:**
    *   Run `dotnet ef migrations add MakeEmailOptional --project ../NikoNiko.Data --startup-project .` locally in the `api/NikoNiko.Api` directory.
    *   Restart the backend service (`docker compose restart backend`) to apply the migration automatically on startup.
*   **Verification:** Migration file exists in `api/NikoNiko.Data/Migrations/` and contains `nullable: true` for the Email column.

### Step 3: Update Authentication Logic (AuthController)
*   **Goal:** Modify the login flow to handle missing emails gracefully.
*   **Status:** [x] 
*   **Action:**
    *   Modify `api/NikoNiko.Api/Controllers/AuthController.cs`:
        *   In `HandleSignIn`, remove the check `if (string.IsNullOrEmpty(email)) throw ...`.
        *   Ensure user lookup logic prioritizes `OAuthId` + `Provider`.
        *   If creating a new user, allow `Email` to be null.
    *   Modify `api/NikoNiko.Api/Program.cs`:
        *   Update `SeedAndSyncSuperAdminRoles`: Ensure it handles users with null emails safely (e.g., `.Where(u => u.Email != null && ...)`).
*   **Verification:** Can login with a mock user having no email (unit test or manual verification).

### Step 4: Update Frontend Models and UI
*   **Goal:** Ensure the frontend doesn't crash when receiving a null email.
*   **Status:** [x]
*   **Action:**
    *   Modify `app/frontend/src/models/User.ts`: Update `User` interface to make `email` optional (`email?: string` or `email: string | null`).
    *   Modify `app/frontend/src/pages/admin/AdminUsersPage.tsx`: Handle null email display (e.g., render "N/A" or empty string).
    *   Check other components displaying user info (Sidebar, Profile, etc.).
*   **Verification:** Frontend builds without TypeScript errors.

### Step 5: Remove Email Scope from OAuth Providers

*   **Goal:** Configure OAuth providers (Discord specifically) to stop requesting the email scope, proving the system works without it.

*   **Status:** [x]

*   **Action:**

    *   Modify `api/NikoNiko.Api/Program.cs`:

        *   Locate the `AddDiscord` configuration.

        *   Remove `options.Scope.Add("email");`.

*   **Verification:** Restart backend, login with Discord, verify in logs/DB that user is created/logged in even if email is not retrieved (or if user denies access).

## 4. 🧪 Testing Strategy
*   **Unit Tests:** Update `AuthControllerTests` to simulate a login payload with null email.
*   **Manual Verification:**
    1.  Delete local DB (or use a fresh user).
    2.  Login via Discord (after Step 5).
    3.  Check DB: User created with `Email = NULL`.
    4.  Check UI: Dashboard works, "My Profile" shows no email or placeholder.

## 5. ✅ Success Criteria
*   Backend builds and runs.
*   Database schema accepts NULL emails.
*   Login via Discord works without the "Email is required" error.
*   Existing users with emails are unaffected.
