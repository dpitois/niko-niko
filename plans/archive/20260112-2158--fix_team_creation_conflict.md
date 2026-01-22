# Implementation Plan - Reconciliation of Super Admin Restriction & Auto-Team Creation

## 1. 🔍 Analysis & Context
*   **Objective:** Allow the system to automatically create a team for new users during signup (onboarding) while maintaining the restriction that only Super Admins can manually create teams via the API.
*   **Affected Files:**
    *   `api/NikoNiko.Api/Controllers/TeamsController.cs` (Permission enforcement point)
    *   `api/NikoNiko.Api/Controllers/AuthController.cs` (Trigger point for onboarding)
    *   `api/NikoNiko.Services/TeamService.cs` (Execution logic)
    *   `api/NikoNiko.Services/ITeamService.cs`
*   **Key Dependencies:** `IsSuperAdminRequirement` (Policy), `IUserContext`.
*   **Risks/Unknowns:** If `TeamService` internally checks for `IsSuperAdmin` using `IUserContext`, this check must be bypassed or removed for the onboarding flow.

## 2. 📋 Checklist
- [ ] Step 1: Enforce Super Admin policy on `TeamsController`.
- [ ] Step 2: Ensure `TeamService` allows creation logic without role checks (or allows bypass).
- [ ] Step 3: Implement auto-creation logic in `AuthController` for new users.
- [ ] Verification

## 3. 📝 Step-by-Step Implementation Details

### Step 1: Enforce Restriction on API Layer
*   **Goal:** Prevent regular users from manually creating teams via HTTP.
*   **Action:**
    *   In `api/NikoNiko.Api/Controllers/TeamsController.cs`, locate the `CreateTeam` method (POST).
    *   Add or verify the presence of `[Authorize(Policy = "IsSuperAdmin")]` (or equivalent role check).
    *   *Note:* Do not place this restriction on the `TeamService` class itself, only on the Controller.

### Step 2: Implement "System" Creation in TeamService
*   **Goal:** Ensure the service layer can create a team purely based on logic, decoupled from the HTTP User Context permissions regarding the "Creation" right.
*   **Action:**
    *   Inspect `api/NikoNiko.Services/TeamService.cs`.
    *   If `CreateTeamAsync` checks `user.IsSuperAdmin`, remove this check. The Service should assume that if it's called, the caller (Controller or System) has already authorized the intent.
    *   Ensure `CreateTeamAsync` takes the necessary arguments (name, ownerId) to create the team properly.

### Step 3: Implement Auto-Creation in AuthController
*   **Goal:** Trigger team creation when a new user signs in without pending invitations.
*   **Action:**
    *   In `api/NikoNiko.Api/Controllers/AuthController.cs`, inside the Login/Callback flow (where the user is created or retrieved):
    *   Identify the block where a **new** user is created.
    *   Check if the user has any accepted/pending invitations (if logic exists).
    *   If no invitations and it's a new user:
        *   Instantiate a `CreateTeamDto` (e.g., Name = $"{User.Name}'s Team").
        *   Call `_teamService.CreateTeamAsync(...)`.
    *   Ensure this logic runs *after* the user is persisted but *before* the final OK response is returned.

## 4. 🧪 Testing Strategy
*   **Integration Tests (`TeamsControllerTests.cs`):**
    *   `CreateTeam_AsNonAdmin_ShouldReturnForbidden`: Verify that calling `POST /api/teams` with a regular user token fails.
    *   `CreateTeam_AsSuperAdmin_ShouldSucceed`: Verify that a Super Admin can still create teams manually.
*   **Integration Tests (`AuthControllerTests.cs`):**
    *   `Login_NewUser_NoInvitation_ShouldCreateTeam`: Mock a new user login. Assert that after login, `dbContext.Teams` contains a team owned by this user.
    *   `Login_NewUser_WithInvitation_ShouldNotCreateTeam`: Mock a new user who accepts an invitation. Assert that NO new team is created (optional, depending on strict requirement).

## 5. ✅ Success Criteria
*   Regular users receive `403 Forbidden` when trying to create a team via API.
*   New users automatically have 1 team created for them upon first login.
*   No "Cyclic Dependency" or "Permission Denied" errors occur during the login process for a new user.
