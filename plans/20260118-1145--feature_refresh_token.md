# Implementation Plan - Refresh Token Mechanism

## 1. 🔍 Analysis & Context
*   **Objective:** Implement a secure JWT refresh token mechanism using HttpOnly cookies to shorten Access Token lifespan and improve security.
*   **Affected Files:**
    *   `api/NikoNiko.Core/Models/User.cs` (Add RefreshTokens relationship)
    *   `api/NikoNiko.Core/Models/RefreshToken.cs` (New entity)
    *   `api/NikoNiko.Data/ApplicationDbContext.cs` (Add DbSet)
    *   `api/NikoNiko.Services/ITokenService.cs` & `TokenService.cs` (Add Refresh Token logic)
    *   `api/NikoNiko.Api/Controllers/AuthController.cs` (New endpoints)
    *   `app/frontend/src/context/AuthContext.tsx` (Handle token logic)
    *   `app/frontend/src/services/api.ts` (New file or update existing for Axios interceptors)
*   **Key Dependencies:** `Microsoft.AspNetCore.Authentication.JwtBearer`, `axios`, `jwt-decode`.
*   **Risks/Unknowns:** CORS configuration for HttpOnly cookies across frontend/backend, database migration handling, SignalR connection re-authentication.

## 2. 📋 Checklist
- [x] Step 1: Data Model - Create RefreshToken entity
- [x] Step 2: Database Migration - Add RefreshToken table
- [x] Step 3: Service Layer - Update TokenService for Access & Refresh tokens
- [x] Step 4: Controller Layer - Implement Refresh and Logout endpoints
- [x] Step 5: Frontend - Axios Interceptor for 401 handling
- [x] Step 6: Frontend - AuthContext updates for silent refresh
- [x] Verification

## 3. 📝 Step-by-Step Implementation Details

### 🚀 Execution Rules
1. **Interactive Flow**: Execute ONLY ONE step at a time.
2. **Atomic Updates**: Before and after each step, you MUST use `replace` to update the checklist in section 2 and the status in section 3.
3. **Status Vocabulary**: Use `[x]` (Done), `[>]` (In Progress), `[-]` (Skipped/N.A.), `[!]` (Failed/Blocked).
4. **User Confirmation**: After updating the file, stop and wait for explicit user confirmation before proceeding to the next step.
5. **No Stealth Actions**: NEVER execute code or modify files without having first updated the plan to reflect that you are about to do so.

### Step 1: Data Model - Create RefreshToken entity
*   **Goal:** Define the database structure for storing refresh tokens.
*   **Status:** [Done]
*   **Action:**
    *   Create `api/NikoNiko.Core/Models/RefreshToken.cs`.
    *   Update `api/NikoNiko.Core/Models/User.cs` with `public List<RefreshToken> RefreshTokens { get; set; } = new();`.
*   **Verification:** Project builds successfully.

### Step 2: Database Migration - Add RefreshToken table
*   **Goal:** Apply schema changes to the database.
*   **Status:** [Done]
*   **Action:**
    *   Add `DbSet<RefreshToken>` to `ApplicationDbContext.cs`.
    *   Run `dotnet ef migrations add AddRefreshToken --project ../NikoNiko.Data --startup-project .` from `api/NikoNiko.Api`.
*   **Verification:** Inspect generated migration file.

### Step 3: Service Layer - Update TokenService
*   **Goal:** Handle the generation of short-lived Access Tokens and secure Refresh Tokens.
*   **Status:** [Done]
*   **Action:**
    *   Modify `TokenService.cs`: Set Access Token expiration to 15 minutes.
    *   Add `GenerateRefreshToken()` method.
*   **Verification:** Unit test for token expiration.

### Step 4: Controller Layer - Implement Refresh and Logout endpoints
*   **Goal:** Expose endpoints for the frontend to renew tokens.
*   **Status:** [Done]
*   **Action:**
    *   Modify `AuthController.cs`: Add `POST /refresh-token` and `POST /logout`.
    *   Set/Clear HttpOnly cookie `refreshToken`.
*   **Verification:** Test with Postman/cURL to see `Set-Cookie` header.

### Step 5: Frontend - Axios Interceptor
*   **Goal:** Automatically handle 401 errors by calling the refresh endpoint.
*   **Status:** [Done]
*   **Action:**
    *   Create or update an Axios instance with an interceptor that waits for refresh if a 401 occurs.
*   **Verification:** Log statement in console when a refresh is attempted.

### Step 6: Frontend - AuthContext updates
*   **Goal:** Synchronize state with new token mechanism.
*   **Status:** [Done]
*   **Action:**
    *   Update `AuthContext.tsx` to handle the new login response and logout logic.
*   **Verification:** Full user flow: Login -> Wait for expiration -> Action -> Token Refreshed automatically.

## 4. 🧪 Testing Strategy
*   **Unit Tests:** Verify `TokenService` correctly sets the 15m expiration.
*   **Integration Tests:** Verify that providing an expired Access Token with a valid Refresh Token cookie returns a new Access Token.
*   **Manual Verification:** Check browser's "Application" tab for the `refreshToken` cookie (should be HttpOnly).

## 5. ✅ Success Criteria
*   Access tokens expire in 15 minutes.
*   Users stay logged in as long as they are active (Refresh Token rotation).
*   Refresh tokens are stored securely (HttpOnly, SameSite=Lax/Strict).
*   Logout invalidates tokens both on client and server.
