# Implementation Plan - Fix Discord Avatar

## 1. 🔍 Analysis & Context
*   **Objective:** Ensure user avatars are correctly retrieved and stored when users log in via Discord.
*   **Affected Files:** `api/NikoNiko.Api/Program.cs`, `api/NikoNiko.Api/Controllers/AuthController.cs`
*   **Key Dependencies:** `Microsoft.AspNetCore.Authentication.Discord`
*   **Risks/Unknowns:** Handling users with default avatars (null hash). Ensuring `ClaimTypes.NameIdentifier` matches the Discord ID used for the avatar URL.

## 2. 📋 Checklist
- [x] Step 1: Map Discord Avatar Hash Claim
- [x] Step 2: Implement Avatar URL Construction Logic
- [x] Verification

## 3. 📝 Step-by-Step Implementation Details

### 🚀 Execution Rules
1. **Interactive Flow**: Execute ONLY ONE step at a time.
2. **Atomic Updates**: Before and after each step, you MUST use `replace` to update the checklist in section 2 and the status in section 3.
3. **Status Vocabulary**: Use `[x]` (Done), `[>]` (In Progress), `[-]` (Skipped/N.A.), `[!]` (Failed/Blocked).
4. **User Confirmation**: After updating the file, stop and wait for explicit user confirmation before proceeding to the next step.
5. **No Stealth Actions**: NEVER execute code or modify files without having first updated the plan to reflect that you are about to do so.

### Step 1: Map Discord Avatar Hash Claim
*   **Goal:** Configure the Discord authentication options to map the `avatar` field from the JSON response to a claim named `urn:discord:avatar:hash`.
*   **Status:** Done
*   **Action:**
    *   Modify `api/NikoNiko.Api/Program.cs` inside the `AddDiscord` configuration block.
    *   Add: `options.ClaimActions.MapJsonKey("urn:discord:avatar:hash", "avatar");`
*   **Verification:** Review `Program.cs` to ensure the mapping is added correctly within the Discord options.

### Step 2: Implement Avatar URL Construction Logic
*   **Goal:** Update the authentication controller to construct the full Discord CDN URL using the hash claim.
*   **Status:** Done
*   **Action:**
    *   Modify `api/NikoNiko.Api/Controllers/AuthController.cs` in the `HandleSignIn` method.
    *   Locate the Discord specific logic block.
    *   Implement logic:
        ```csharp
        if (string.IsNullOrEmpty(avatar) && provider == DiscordAuthenticationDefaults.AuthenticationScheme)
        {
            var avatarHash = claims.FirstOrDefault(c => c.Type == "urn:discord:avatar:hash")?.Value;
            var userId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            
            if (!string.IsNullOrEmpty(avatarHash) && !string.IsNullOrEmpty(userId)) 
            {
                avatar = $"https://cdn.discordapp.com/avatars/{userId}/{avatarHash}.png";
            }
        }
        ```
*   **Verification:** Verify code compilation. Ideally, perform a login test (requires manual verification).

## 4. 🧪 Testing Strategy
*   Unit Tests: Not applicable (Auth flow is hard to unit test without mocking `HttpContext`).
*   Integration Tests: Difficult to test OAuth providers in integration tests without secrets.
*   Manual Verification:
    1.  Start the application.
    2.  Login with Discord.
    3.  Check the "My Profile" or Dashboard to see if the avatar appears.
    4.  Check the database `Users` table to confirm `AvatarUrl` is populated.

## 5. ✅ Success Criteria
*   New users logging in with Discord have their `AvatarUrl` populated.
*   Existing users logging in with Discord have their `AvatarUrl` updated.
*   No regression for GitHub or Google login.