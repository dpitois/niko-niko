# Implementation Plan - Fix OAuth Consent Cancellation Crash

## 1. 🔍 Analysis & Context
*   **Objective:** Prevent the `AuthenticationFailureException` when a user cancels the OAuth flow (specifically Discord) and redirect them gracefully to the frontend.
*   **Affected Files:** `api/NikoNiko.Api/Program.cs`, `app/frontend/src/pages/Login.tsx` (or equivalent)
*   **Key Dependencies:** `Microsoft.AspNetCore.Authentication`, `Microsoft.AspNetCore.Authentication.OAuth`
*   **Risks/Unknowns:** We must ensure we don't suppress critical configuration errors (like invalid ClientSecret) by treating all failures as user cancellations.

## 2. 📋 Checklist
- [x] Step 1: Analyze `Program.cs` configuration
- [x] Step 2: Implement `OnRemoteFailure` event handler in `Program.cs`
- [x] Step 3: Display error message on Frontend Login page
- [x] Step 4: Refactor `DiscordIcon` into a reusable component
- [x] Step 5: Fix Redirect URL in `Program.cs`
- [x] Verification

## 3. 📝 Step-by-Step Implementation Details

### 🚀 Execution Rules
1. **Interactive Flow**: Execute ONLY ONE step at a time.
2. **Atomic Updates**: Before and after each step, you MUST use `replace` to update the checklist in section 2 and the status in section 3.
3. **Status Vocabulary**: Use `[x]` (Done), `[>]` (In Progress), `[-]` (Skipped/N.A.), `[!]` (Failed/Blocked).
4. **User Confirmation**: After updating the file, stop and wait for explicit user confirmation before proceeding to the next step.
5. **No Stealth Actions**: NEVER execute code or modify files without having first updated the plan to reflect that you are about to do so.

### Step 1: Analyze `Program.cs` configuration
*   **Goal:** Confirm the current configuration of Discord, Google, and GitHub providers to identify where to inject the failure handler.
*   **Status:** Done
*   **Action:**
    *   Read `api/NikoNiko.Api/Program.cs`.
    *   Identify the `AddDiscord` block.
    *   Check if generic error handling can be applied to verify consistency across providers.
*   **Verification:** Visual inspection of the code.

### Step 2: Implement `OnRemoteFailure` event handler
*   **Goal:** Add the `OnRemoteFailure` event to the OAuth options.
*   **Status:** Done
*   **Action:**
    *   Modify `api/NikoNiko.Api/Program.cs`.
    *   Inside `.AddDiscord(...)`:
        ```csharp
        options.Events.OnRemoteFailure = context =>
        {
            var failureMessage = Uri.EscapeDataString(context.Failure?.Message ?? "Unknown error");
            context.Response.Redirect(config["Authentication:FrontendRedirectUrl"] + "/auth/login?error=" + failureMessage);
            context.HandleResponse();
            return Task.CompletedTask;
        };
        ```
    *   *Note:* Ensure the redirect URL matches the frontend route (e.g., `/auth/login` or just `/login`).
*   **Verification:** Compile the project.

### Step 3: Display error message on Frontend Login page
*   **Goal:** Update the React login page to read the `error` query parameter and display a user-friendly alert.
*   **Status:** Done

### Step 4: Refactor `DiscordIcon` into a reusable component
*   **Goal:** Extract the Discord SVG icon into a shared component to ensure consistency and remove duplication.
*   **Status:** Done

### Step 5: Fix Redirect URL in `Program.cs`
*   **Goal:** Update the redirect URL in the `OnRemoteFailure` handler to match the frontend route (`/login` instead of `/auth/login`).
*   **Status:** Done
*   **Action:**
    *   Modify `api/NikoNiko.Api/Program.cs`.
    *   Change `/auth/login?error=` to `/login?error=` in all 3 providers.
*   **Verification:** Compile and manual test (cancel login again).
*   **Action:**
    *   Create `app/frontend/src/components/icons/DiscordIcon.tsx`.
    *   Use the path from `LoginPage.tsx` (assuming it's the correct/more complete one).
    *   Update `LoginPage.tsx` and `AdminUsersPage.tsx` to import this new component.
*   **Verification:** Check both pages to ensure the icon renders correctly. Lint the project.
*   **Action:**
    *   Locate the Login page file (likely `app/frontend/src/pages/Login.tsx`).
    *   Use `useSearchParams` from `react-router-dom` to retrieve the `error` param.
    *   Add a conditional rendering block (e.g., MUI `Alert`) to show the message if present.
*   **Verification:** Verify code compilation and linting.

## 4. 🧪 Testing Strategy
*   Manual Verification:
    1.  Start the app.
    2.  Click "Login with Discord".
    3.  On the Discord consent screen, click "Cancel" (or "Refuser").
    4.  Verify that the browser redirects to the frontend (e.g., `http://localhost:3000/login?error=...`) instead of showing the ASP.NET Core developer exception page.

## 5. ✅ Success Criteria
*   No 500/Unhandled Exception page when canceling OAuth.
*   User is redirected to the frontend.