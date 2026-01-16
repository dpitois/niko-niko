# Implementation Plan - Feature: Provider Tracking

## 1. 🔍 Analysis & Context
*   **Objective:** Store the authentication provider (GitHub, Google, Discord) in the User entity upon login/creation and display this information in the Admin Users list on the frontend.
*   **Affected Files:**
    *   `api/NikoNiko.Core/Models/User.cs`
    *   `api/NikoNiko.Data/Migrations/*` (New Migration)
    *   `api/NikoNiko.Api/Controllers/AuthController.cs`
    *   `app/frontend/src/models/User.ts`
    *   `app/frontend/src/pages/AdminUsersPage.tsx`
*   **Key Dependencies:** Entity Framework Core, Material UI Icons (Frontend).
*   **Risks/Unknowns:** Existing users will have a null provider initially. Frontend must handle null values gracefully.

## 2. 📋 Checklist
- [x] Step 1: Update Backend Model (User.cs)
- [x] Step 2: Create Database Migration
- [x] Step 3: Update AuthController Logic
- [x] Step 4: Update Frontend Model (User.ts)
- [>] Step 5: Update AdminUsersPage UI
- [x] Verification

## 3. 📝 Step-by-Step Implementation Details

### 🚀 Execution Rules
1. **Interactive Flow**: Execute ONLY ONE step at a time.
2. **Atomic Updates**: Before and after each step, you MUST use `replace` to update the checklist in section 2 and the status in section 3.
3. **Status Vocabulary**: Use `[x]` (Done), `[>]` (In Progress), `[-]` (Skipped/N.A.), `[!]` (Failed/Blocked).
4. **User Confirmation**: After updating the file, stop and wait for explicit user confirmation before proceeding to the next step.
5. **No Stealth Actions**: NEVER execute code or modify files without having first updated the plan to reflect that you are about to do so.

### Step 1: Update Backend Model (User.cs)
*   **Goal:** Add the `Provider` property to the User entity.
*   **Status:** [x]
*   **Action:**
    *   Modify `api/NikoNiko.Core/Models/User.cs`: Add `public string? Provider { get; set; }`.
*   **Verification:** Backend compiles.

### Step 2: Create Database Migration
*   **Goal:** Generate the SQL migration script.
*   **Status:** [x]
*   **Action:**
    *   Run `dotnet ef migrations add AddProviderToUser --project ../NikoNiko.Data --startup-project .` locally in `api/NikoNiko.Api`.
    *   **Note:** The migration will be applied automatically when the user restarts the docker container later.
*   **Verification:** Migration file created in `api/NikoNiko.Data/Migrations`.

### Step 3: Update AuthController Logic
*   **Goal:** Populate the Provider property during login/registration.
*   **Status:** [x]
*   **Action:**

### Step 4: Update Frontend Model (User.ts)
*   **Goal:** Reflect the API change in the frontend type definition.
*   **Status:** [x]
*   **Action:**

### Step 5: Update AdminUsersPage UI
*   **Goal:** Display the provider icon in the users list.
*   **Status:** [>]
*   **Action:**
    *   Modify `app/frontend/src/pages/AdminUsersPage.tsx`:
        *   Import icons: `GitHub`, `Google`, `QuestionMark` (or similar).
        *   Create a helper function `getProviderIcon(provider?: string)`.
        *   Add a column "Provider" to the table.
        *   Render the icon in the user row.
    *   **Note:** Need to add Discord icon support (custom SVG or find if MUI has one, likely reuse the SVG from Login page or use a generic one if unavailable in MUI standard). *MUI does not have Discord icon by default, will use a generic 'Game' icon or just text/chip if custom SVG is too verbose for this file, OR reuse the SvgIcon if exported.* -> *Decision: Use a Chip with text or a generic icon for simplicity unless we export the DiscordIcon component.* -> *Better: Copy the DiscordIcon definition locally or into a shared component.*
*   **Verification:** Build frontend.

## 4. 🧪 Testing Strategy
*   **Manual Verification:**
    1.  Restart Docker (to apply migration).
    2.  Login with Discord/GitHub.
    3.  Go to Admin > Users.
    4.  Verify the new column shows the correct provider icon.

## 5. ✅ Success Criteria
*   User table contains a Provider column.
*   New users have their provider saved in DB.
*   Existing users have their provider updated upon login.
