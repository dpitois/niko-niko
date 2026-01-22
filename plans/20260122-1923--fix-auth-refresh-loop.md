# Implementation Plan - Fix Auth Refresh Loop (Zombie Session)

## 1. 🔍 Analysis & Context
*   **Objective:** Fix the infinite loop and deadlock occurring when a user has an expired/invalid JWT in local storage but is missing the `refresh_token` cookie (legacy session state from before the feature deployment).
*   **Affected Files:**
    *   `app/frontend/src/services/api.ts`
    *   `app/frontend/src/context/AuthContext.tsx`
*   **Key Dependencies:** `axios`
*   **Risks/Unknowns:** Ensure exclusions in the interceptor do not break legitimate token refresh for other endpoints.

## 2. 📋 Checklist
- [x] Step 1: Prevent Axios interceptor recursion on Auth endpoints
- [x] Step 2: Make Logout cleanup robust and optimistic
- [x] Verification

## 3. 📝 Step-by-Step Implementation Details

### 🚀 Execution Rules
1. **Interactive Flow**: Execute ONLY ONE step at a time.
2. **Atomic Updates**: Before and after each step, you MUST use `replace` to update the checklist in section 2 and the status in section 3.
3. **Status Vocabulary**: Use `[x]` (Done), `[>]` (In Progress), `[-]` (Skipped/N.A.), `[!]` (Failed/Blocked).
4. **User Confirmation**: After updating the file, stop and wait for explicit user confirmation before proceeding to the next step.
5. **No Stealth Actions**: NEVER execute code or modify files without having first updated the plan to reflect that you are about to do so.

### Step 1: Prevent Axios interceptor recursion on Auth endpoints
*   **Goal:** Prevent a 401 error on `/auth/refresh-token` (or login/logout) from triggering a recursive refresh attempt, which causes a deadlock.
*   **Status:** `[x]`
*   **Action:**
    *   Modify `app/frontend/src/services/api.ts`.
    *   Add a condition in the response interceptor to immediately reject the promise if the original request URL involves authentication routes.
*   **Verification:** Verify that the application no longer freezes when the cookie is missing.

### Step 2: Make Logout cleanup robust and optimistic
*   **Goal:** Ensure `logout()` clears `localStorage` and React state *even if* the `/auth/logout` API call fails (e.g., with a 401).
*   **Status:** `[x]`
*   **Action:**
    *   Modify `app/frontend/src/context/AuthContext.tsx`.
    *   Move cleanup logic (`localStorage.removeItem`, `setUser(null)`) to a `finally` block to ensure it runs regardless of the API call result.
*   **Verification:** Simulate an error during the logout call and verify the user is still logged out on the client side.

## 4. 🧪 Testing Strategy
*   **Unit Tests:** N/A (Logic tied to interceptors and context).
*   **Manual Verification:**
    1.  Open the application and be logged in.
    2.  Manually delete the `refreshToken` cookie via DevTools.
    3.  Modify the JWT in `localStorage` to make it invalid.
    4.  Refresh the page.
    5.  The user should be redirected to the Login page without the application freezing.

## 5. ✅ Success Criteria
*   Users in "zombie" state are redirected to the login page smoothly.
*   No browser freezes or infinite network loops.
*   "Token is required" error from backend correctly triggers a client-side logout.