# Implementation Plan - Centralize Data Models in `src/models`

## 1. 🔍 Analysis & Context
*   **Objective:** Refactor the codebase to ensure all shared data models and types are defined exclusively in `app/frontend/src/models`. Component-specific types (like Props or local State) remain allowed in components.
*   **Affected Files:** 
    *   `app/frontend/src/context/AuthContext.tsx` (Primary target)
    *   New file: `app/frontend/src/models/Auth.ts`
*   **Key Dependencies:** `jwt-decode` (used in AuthContext, defines the shape of the token).
*   **Risks/Unknowns:** 
    *   Potential naming conflicts if we try to unify `DecodedToken` with the existing `User` model too aggressively. Strategy is to keep them separate but co-located in `models`.
    *   Breaking imports if other files relied on the types exported from `AuthContext` (though `DecodedToken` was not exported, checking exports is crucial).

## 2. 📋 Checklist
- [ ] Step 1: Create `src/models/Auth.ts`
- [ ] Step 2: Refactor `AuthContext.tsx` to use new models
- [ ] Verification

## 3. 📝 Step-by-Step Implementation Details

### Step 1: Create `src/models/Auth.ts`
*   **Goal:** Centralize authentication-related types that are currently trapped in `AuthContext.tsx`.
*   **Action:**
    *   Create file `app/frontend/src/models/Auth.ts`.
    *   Extract `DecodedToken` interface from `AuthContext.tsx` into this file.
    *   Extract `TeamRole` interface from `AuthContext.tsx` into this file.
    *   Ensure they are exported (`export interface ...`).
    *   *Note:* Keep the exact property names (like `avatar_url`) to avoid breaking existing usages in components (like Sidebar).

### Step 2: Refactor `AuthContext.tsx`
*   **Goal:** Remove local type definitions and import them from the centralized location.
*   **Action:**
    *   Modify `app/frontend/src/context/AuthContext.tsx`.
    *   Add import: `import type { DecodedToken, TeamRole } from '../models/Auth';`.
    *   Delete the local `interface DecodedToken { ... }` definition.
    *   Delete the local `interface TeamRole { ... }` definition.
    *   Ensure `AuthContextType` (which is a context-specific type, effectively "Props" for the context value) uses the imported types.
*   **Verification:**
    *   Check that `AuthContext.tsx` no longer contains the model definitions.
    *   Ensure IDE/Linter shows no errors regarding missing types.

## 4. 🧪 Testing Strategy
*   **Manual Verification:**
    *   Run `npm run build` in `app/frontend` to ensure TypeScript compilation passes.
    *   Start the application and verify the login flow.
    *   Check the Sidebar to ensure the user avatar and name still appear (validating `DecodedToken` structure is preserved).
    *   Check "My Teams" page to ensure role-based logic (Admin/Member) still works (validating `TeamRole` usage).

## 5. ✅ Success Criteria
*   `app/frontend/src/context/AuthContext.tsx` contains NO data model interfaces (`DecodedToken`, `TeamRole`).
*   `app/frontend/src/models/Auth.ts` exists and contains these definitions.
*   The application builds and runs without TypeScript errors.
