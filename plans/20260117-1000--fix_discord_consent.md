# Implementation Plan - Fix Discord Consent Loop

## 1. 🔍 Analysis & Context
*   **Objective:** Fix the issue where Discord OAuth forces user consent on every login.
*   **Issue:** [#39](https://github.com/dpitois/niko-niko/issues/39)
*   **Context:** Discord OAuth2 API forces re-consent if `prompt=consent` is present in the URL. We need to ensure this parameter is not sent to allow seamless login for already authorized users.
*   **Affected Files:** `api/NikoNiko.Api/Program.cs`
*   **Key Dependencies:** `AspNet.Security.OAuth.Discord`

## 2. 📋 Checklist
- [x] Step 1: Configure Discord OAuth events in `Program.cs` to sanitize the Redirect URI.

## 3. 📝 Step-by-Step Implementation Details

### 🚀 Execution Rules
1. **Interactive Flow**: Execute ONLY ONE step at a time.
2. **Atomic Updates**: Before and after each step, you MUST use `replace` to update the checklist in section 2 and the status in section 3.
3. **Status Vocabulary**: Use `[x]` (Done), `[>]` (In Progress), `[-]` (Skipped/N.A.), `[!]` (Failed/Blocked).
4. **User Confirmation**: After updating the file, stop and wait for explicit user confirmation before proceeding to the next step.

### Step 1: Configure Discord OAuth Options
*   **Goal:** Use the `OnRedirectToAuthorizationEndpoint` event to remove `prompt=consent` from the generated authorization URL.
*   **Status:** [x] Done
*   **Action:**
    *   In `api/NikoNiko.Api/Program.cs`, locate the `.AddDiscord(options => ...)` block.
    *   Add an event handler for `OnRedirectToAuthorizationEndpoint`.
    *   Inside the handler, modify `ctx.RedirectUri` to remove `prompt=consent`.
*   **Code Snippet Structure:**
    ```csharp
    options.Events.OnRedirectToAuthorizationEndpoint = context =>
    {
        context.RedirectUri = context.RedirectUri.Replace("&prompt=consent", "");
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
    ```
    *(Note: We will handle potential query string nuances like `?prompt=consent` vs `&prompt=consent` appropriately)*
*   **Verification:**
    *   Build the backend.
    *   (Manual) Verify Discord login flow.

## 4. 🧪 Testing Strategy
*   **Manual Verification:**
    1.  Start the application.
    2.  Log out if connected.
    3.  Click "Sign in with Discord".
    4.  **Expectation:** Redirects directly to app (if previously authorized) without showing the "Authorize NikoNiko" screen again.

## 5. ✅ Success Criteria
*   Discord login is seamless for returning users.
*   New users can still sign up and consent normally (default behavior).
