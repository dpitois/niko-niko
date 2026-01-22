# Implementation Plan - Auto Team Creation for New Users

## 1. 🔍 Analysis & Context
*   **Objective:** Automatically create a team for new users who sign up without an invitation token, making them the admin of that team.
*   **Affected Files:**
    *   `api/NikoNiko.Api/Controllers/AuthController.cs`
    *   `api/NikoNiko.Api.IntegrationTests/AuthControllerTests.cs`
*   **Key Dependencies:** `ApplicationDbContext`, `Team`, `TeamUser` models, `TestAuthenticationHandler`.
*   **Risks/Unknowns:** Ensure strictly that this only happens on *creation* (first login), not subsequent logins, and avoids conflict with invitation flows.

## 2. 📋 Checklist
- [ ] Step 1: Implement Auto-Team Creation Logic in `AuthController.cs`.
- [ ] Step 2: Implement Integration Tests in `AuthControllerTests.cs`.
- [ ] Verification

## 3. 📝 Step-by-Step Implementation Details

### Step 1: Implement Auto-Team Creation Logic
*   **Goal:** Modify the `HandleSignIn` method to detect new users without invitations and provision a team.
*   **Action:**
    *   Open `api/NikoNiko.Api/Controllers/AuthController.cs`.
    *   Locate the `HandleSignIn` method.
    *   Inside the block where a new user is created (`if (user == null)`):
        *   Check if `invitationToken` is null or empty.
        *   If true, instantiate a new `Team`:
            *   `Name`: `"{user.Name ?? user.Email}'s Team"` (use a fallback like "My Team" if name is null).
            *   `AdminId`: `user.Id`.
            *   `CreatedAt`: `DateTime.UtcNow`.
        *   Instantiate a new `TeamUser`:
            *   `Team`: The new team.
            *   `UserId`: `user.Id`.
        *   Add both entities to the `_context`.
        *   Ensure `_context.SaveChangesAsync()` is called *after* adding these entities to persist them along with the user.

### Step 2: Implement Integration Tests
*   **Goal:** Verify the logic with automated integration tests covering three key scenarios.
*   **Action:**
    *   Open `api/NikoNiko.Api.IntegrationTests/AuthControllerTests.cs`.
    *   Add a new test method `SignIn_NewUser_NoInvitation_CreatesTeam`:
        *   **Arrange:** Ensure `newtestuser@example.com` does NOT exist in DB.
        *   **Act:** Call `GET /api/auth/signin-github` (mimicking the callback).
        *   **Assert:** Verify response is a Redirect. Retrieve `newtestuser` from DB. Verify a Team exists where `AdminId` is this user's ID. Verify `TeamUser` entry exists.
    *   Add a new test method `SignIn_NewUser_WithInvitation_NoTeamCreated`:
        *   **Arrange:** Create a "Team A", create an invitation for it.
        *   **Act:** Call `GET /api/auth/signin-github?invitationToken={token}`.
        *   **Assert:** Verify user is created. Verify user is member of "Team A". Verify user is NOT admin of any team (count of teams where AdminId == User.Id is 0).
    *   Add a new test method `SignIn_ExistingUser_NoNewTeam`:
        *   **Arrange:** Pre-seed `newtestuser@example.com` in the DB.
        *   **Act:** Call `GET /api/auth/signin-github`.
        *   **Assert:** Verify no *new* team is created (check total team count or user's admin team count remains unchanged).

## 4. 🧪 Testing Strategy
*   **Unit Tests:** N/A (Logic is in Controller, better suited for Integration Tests).
*   **Integration Tests:** The 3 scenarios outlined in Step 2.
*   **Manual Verification:**
    1.  Login with a fresh GitHub account (or clear DB).
    2.  Check "My Teams" page.
    3.  Verify Admin status.

## 5. ✅ Success Criteria
*   A new user logging in without an invite is immediately an admin of a new team.
*   A new user logging in *with* an invite joins the invited team and *does not* get a generated team.
*   Existing users are unaffected by this change.
