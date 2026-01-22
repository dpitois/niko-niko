# Implementation Plan - Fix Discord Consent with Fallback Strategy

## 1. 🔍 Analysis & Context
*   **Objective:** Implement a "Check First, Then Ask" flow. Attempt silent login (`prompt=none`) first. If Discord requires interaction (new user or revoked), catch the failure and redirect to the consent screen (`prompt=consent`).
*   **Affected Files:** `api/NikoNiko.Api/Program.cs`
*   **Key Dependencies:** `AspNet.Security.OAuth.Discord`
*   **Risks/Unknowns:** Correctly identifying the `interaction_required` error from the `context.Failure` object or properties.

## 2. 📋 Checklist
- [ ] Step 1: Update `AuthController.cs` to handle `prompt` parameter
- [x] Step 2: Configure `Prompt = "none"` and implement Fallback Logic in `Program.cs`
- [x] Verification

## 3. 📝 Step-by-Step Implementation Details

### 🚀 Execution Rules
1. **Interactive Flow**: Execute ONLY ONE step at a time.
2. **Atomic Updates**: Before and after each step, you MUST use `replace` to update the checklist in section 2 and the status in section 3.
3. **Status Vocabulary**: Use `[x]` (Done), `[>]` (In Progress), `[-]` (Skipped/N.A.), `[!]` (Failed/Blocked).
4. **User Confirmation**: After updating the file, stop and wait for explicit user confirmation before proceeding to the next step.
5. **No Stealth Actions**: NEVER execute code or modify files without having first updated the plan to reflect that you are about to do so.

### Step 1: Update `AuthController.cs` to handle `prompt` parameter
*   **Goal:** Allow the `LoginDiscord` endpoint to accept a `prompt` query parameter and pass it to the authentication properties.
*   **Status:** Done
*   **Action:**
    *   Modify `api/NikoNiko.Api/Controllers/AuthController.cs`.
    *   Update `LoginDiscord` signature: `public IActionResult LoginDiscord(string? invitationToken = null, string? prompt = null)`
    *   If `prompt` is present (e.g., "consent"), add it to `properties.Items["prompt"]` or use `properties.SetParameter("prompt", prompt)`.
        *   *Note:* The Discord handler needs to know to look for this property. If using `options.Prompt`, it's global.
        *   To override per request, we usually need the `OnRedirectToAuthorizationEndpoint` to look at `context.Properties.Items`.
        *   Actually, `AuthenticationProperties.Parameters` (if supported) or `Items` are accessible in the redirect event.
        *   Let's simply pass it in `Items` and update `Program.cs` to respect it.
*   **Verification:** Compile.

### Step 2: Configure `Prompt = "none"` and implement Fallback Logic in `Program.cs`
*   **Goal:** Set up the primary silent flow and the error handler to retry with consent.
*   **Status:** Done

### Step 3: Add Debug Logging
*   **Goal:** Trace the execution flow to identify where the request hangs or fails.
*   **Status:** In Progress
*   **Action:**
    *   Add `Console.WriteLine` in `AuthController.LoginDiscord`.
    *   Add `Console.WriteLine` in `Program.cs` events (`OnRedirectToAuthorizationEndpoint`, `OnRemoteFailure`).
*   **Verification:** Check `docker logs` after reproducing the issue.
*   **Action:**
    *   Modify `api/NikoNiko.Api/Program.cs`.
    *   Set `options.Prompt = "none";`.
    *   Remove the previous `OnRedirectToAuthorizationEndpoint` (Regex logic is no longer needed if we use properties).
    *   Update `OnRemoteFailure`:
        ```csharp
        options.Events.OnRemoteFailure = context =>
        {
            if (context.Failure?.Message?.Contains("interaction_required") == true || 
                context.Request.Query["error"] == "interaction_required")
            {
                // Fallback: User needs to consent. Redirect to authorization endpoint with prompt=consent.
                var challengeUrl = "/api/auth/login-discord?prompt=consent"; 
                // Wait, we can't easily restart the middleware flow from here without losing state/correlation cookies.
                // Better approach: Redirect to the provider MANUALLY or construct a new challenge.
                
                // Simpler: Redirect the user to a special endpoint or just retry the Challenge with a property?
                // Problem: Infinite loop if we just challenge again with defaults.
                
                // Solution: We need to distinguish the retry.
                // But wait, the standard OAuth2 error comes in the QueryString as 'error=interaction_required'.
                // ASP.NET Core middleware catches this and throws an exception/event.
                
                // Let's redirect to the original Challenge endpoint but add a flag 'force_consent=true' 
                // and handle that in AuthController to set prompt=consent property?
                
                context.Response.Redirect("/api/auth/login-discord?prompt=consent"); // We need to update AuthController to handle this param
                context.HandleResponse();
                return Task.CompletedTask;
            }

            var failureMessage = Uri.EscapeDataString(context.Failure?.Message ?? "Unknown error");
            context.Response.Redirect(config["Authentication:FrontendRedirectUrl"] + "/login?error=" + failureMessage);
            context.HandleResponse();
            return Task.CompletedTask;
        };
        ```
    *   We also need to update `AuthController.cs` to accept `prompt` or `force_consent` param and apply it to AuthenticationProperties.
*   **Verification:** Compile and test.

## 4. 🧪 Testing Strategy
*   Manual Verification:
    1.  Login (authorized) -> Silent.
    2.  Revoke app in Discord -> Login -> Silent fails -> Redirects to Consent -> Authorize -> Success.
