# Implementation Plan - Feature Onboarding Consent

## 1. 🔍 Analysis & Context
*   **Objective:** Implement a mandatory onboarding screen to collect GDPR consent (Terms of Service, Privacy Policy) from users upon their first login (or if they haven't consented yet).
*   **Affected Files:**
    *   `api/NikoNiko.Core/Models/User.cs`
    *   `api/NikoNiko.Services/TokenService.cs`
    *   `api/NikoNiko.Api/Controllers/UsersController.cs` (or `AuthController.cs`)
    *   `app/frontend/src/models/User.ts`
    *   `app/frontend/src/models/Auth.ts`
    *   `app/frontend/src/context/AuthContext.tsx`
    *   `app/frontend/src/components/ProtectedRoute.tsx`
    *   `app/frontend/src/App.tsx`
    *   `app/frontend/src/pages/OnboardingPage.tsx` (New)
*   **Key Dependencies:** `jwt-decode`, `react-router-dom`, `API Authentication (JWT)`.
*   **Risks/Unknowns:**
    *   **Token Refresh:** When a user accepts the consent, their current JWT token has `is_onboarded=false`. The API must return a new token with `is_onboarded=true` immediately to avoid a re-login requirement.
    *   **Existing Users:** Migration strategy for existing users. They should likely be forced to onboard as well if they haven't consented to the new terms. Default `IsOnboarded` to `false` covers this.

## 2. 📋 Checklist
- [x] Step 1: Backend - Update User Model & Migration
- [x] Step 2: Backend - Update TokenService & DTOs
- [x] Step 3: Backend - Create Consent Endpoint
- [x] Step 4: Frontend - Update Types & Context
- [x] Step 5: Frontend - Create Onboarding Page
- [x] Step 6: Frontend - Implement Route Guarding
- [ ] Verification

## 3. 📝 Step-by-Step Implementation Details

### 🚀 Execution Rules
1. **Interactive Flow**: Execute ONLY ONE step at a time.
2. **Atomic Updates**: Before and after each step, you MUST use `replace` to update the checklist in section 2 and the status in section 3.
3. **Status Vocabulary**: Use `[x]` (Done), `[>]` (In Progress), `[-]` (Skipped/N.A.), `[!]` (Failed/Blocked).
4. **User Confirmation**: After updating the file, stop and wait for explicit user confirmation before proceeding to the next step.
5. **No Stealth Actions**: NEVER execute code or modify files without having first updated the plan to reflect that you are about to do so.

### Step 1: Backend - Update User Model & Migration
*   **Goal:** Add persistence for consent tracking.
*   **Status:** Done
*   **Action:**
    *   Modify `api/NikoNiko.Core/Models/User.cs`:
        ```csharp
        public bool IsOnboarded { get; set; } = false;
        public DateTime? ConsentAt { get; set; }
        public string? ConsentVersion { get; set; }
        ```
    *   Run migration command: `dotnet ef migrations add AddUserOnboarding --project ../NikoNiko.Data --startup-project .` (from `api/NikoNiko.Api` folder).
*   **Verification:** Check the generated migration file.

### Step 2: Backend - Update TokenService & DTOs
*   **Goal:** Include onboarding status in the JWT so the frontend knows immediately.
*   **Status:** Done
*   **Action:**
    *   Modify `api/NikoNiko.Services/TokenService.cs`:
        *   Add claim `is_onboarded` (value "true" or "false") in `CreateToken`.
*   **Verification:** Run backend tests (existing) to ensure no breakage.

### Step 3: Backend - Create Consent Endpoint
*   **Goal:** Allow users to submit their consent.
*   **Status:** Done
*   **Action:**
    *   Create DTO `api/NikoNiko.Core/DTOs/User/UserConsentDto.cs`: `{ string ConsentVersion }`.
    *   Modify `api/NikoNiko.Api/Controllers/UsersController.cs`:
        *   Add endpoint `POST /api/users/consent`.
        *   Implementation: Update `User` in DB (`IsOnboarded = true`, `ConsentAt = Now`, `Version`).
        *   **Crucial:** Return a *new* JWT token in the response so the client updates its state.
*   **Verification:** Manual API call via Swagger or `.http` file.

### Step 4: Frontend - Update Types & Context
*   **Goal:** Reflect backend changes in frontend types.
*   **Status:** Done
*   **Action:**
    *   Modify `app/frontend/src/models/Auth.ts`: Add `is_onboarded?: string` to `DecodedToken`.
    *   Modify `app/frontend/src/models/User.ts`: Add `isOnboarded: boolean`.
    *   Modify `app/frontend/src/context/AuthContext.tsx`:
        *   Add `isOnboarded` boolean to context state.
        *   Update `login` and `useEffect` to parse this claim from the token.
*   **Verification:** TypeScript check.

### Step 5: Frontend - Create Onboarding Page
*   **Goal:** UI for the consent.
*   **Status:** Done
*   **Action:**
    *   Create `app/frontend/src/pages/OnboardingPage.tsx`.
        *   UI: "Welcome", Terms check (Checkbox), Submit button.
        *   Logic: On submit, call `POST /api/users/consent`, receive new token, call `login(newToken)`, redirect to `/my-teams`.
*   **Verification:** Visual check (navigate manually).

### Step 6: Frontend - Implement Route Guarding
*   **Goal:** Force users to onboarding if not done.
*   **Status:** Done
*   **Action:**
    *   Modify `app/frontend/src/components/ProtectedRoute.tsx`:
        *   If `user && !user.isOnboarded`, redirect to `/onboarding`.
    *   Modify `app/frontend/src/App.tsx`:
        *   Add route `/onboarding` element `<OnboardingPage />`.
        *   Ensure `/onboarding` is *authenticated* but *not* guarded by the onboarding check itself (to prevent infinite loop).
*   **Verification:** Try to access dashboard with a non-onboarded user.

## 4. 🧪 Testing Strategy
*   **Unit Tests:**
    *   `TokenServiceTests.cs`: Verify `is_onboarded` claim is present.
    *   `UsersControllerTests.cs`: Verify `SubmitConsent` updates DB and returns token.
*   **Integration Tests:**
    *   Create `UserOnboardingTests.cs` in `NikoNiko.Api.IntegrationTests`:
        *   Test flow: Login (non-onboarded) -> Access Secured API (should work, API doesn't block, Frontend does) -> Submit Consent -> Check DB.
*   **Manual Verification:**
    1.  Register new user via GitHub.
    2.  Observe redirection to `/onboarding`.
    3.  Try to change URL to `/my-teams` -> Should redirect back.
    4.  Accept terms.
    5.  Observe redirection to `/my-teams`.

## 5. ✅ Success Criteria
*   New users are blocked from the dashboard until they accept terms.
*   Consent date and version are stored in the database.
*   Existing users are prompted on their next login (due to default `false`).
