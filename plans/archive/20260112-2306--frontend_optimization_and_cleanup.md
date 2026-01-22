# Implementation Plan - Frontend Optimization & Cleanup

## 1. 🔍 Analysis & Context
*   **Objective:** Reduce the frontend bundle size (currently ~900kB, warnings issued) by optimizing Material UI imports and complete the refactoring of relative imports to use path aliases (`@/`).
*   **Affected Files:** `package.json`, `vite.config.ts`, `app/frontend/src/**/*` (especially components using MUI icons and relative imports).
*   **Key Dependencies:** `rollup-plugin-visualizer` (new dev dependency), `@mui/icons-material`.
*   **Risks/Unknowns:** Tree-shaking might already be effective, so manual import optimization might yield minimal gains. Changing imports across many files carries a small risk of breaking references if not done precisely.

## 2. 📋 Checklist
- [ ] Step 1: Install and configure Bundle Visualizer
- [ ] Step 2: Refactor Material UI Icon imports (Barrel -> Path)
- [ ] Step 3: Fix remaining relative imports (`../../`)
- [ ] Step 4: Verification & Comparison

## 3. 📝 Step-by-Step Implementation Details

### Step 1: Install and configure Bundle Visualizer
*   **Goal:** Establish a precise baseline of what consumes space in the bundle.
*   **Action:**
    *   Run `npm install -D rollup-plugin-visualizer` in `app/frontend`.
    *   Modify `app/frontend/vite.config.ts`:
        ```typescript
        import { visualizer } from "rollup-plugin-visualizer";
        // ... in plugins array:
        visualizer({
            open: false,
            gzipSize: true,
            brotliSize: true,
            filename: "stats.html", 
        }),
        ```
    *   Run `npm run build` to generate `stats.html`.
*   **Verification:** Check if `app/frontend/stats.html` exists and open it (or check console output for file creation).

### Step 2: Refactor Material UI Icon imports
*   **Goal:** Ensure only used icons are included in the bundle, bypassing potential tree-shaking misses with barrel files.
*   **Action:**
    *   Replace imports like:
        `import { Face, Mood } from '@mui/icons-material';`
    *   With:
        ```typescript
        import Face from '@mui/icons-material/Face';
        import Mood from '@mui/icons-material/Mood';
        ```
    *   Use a script/tool (`jscodeshift` or complex `sed`/regex) to automate this safely across `src/`.
*   **Verification:** Build should succeed. Bundle size *might* decrease.

### Step 3: Fix remaining relative imports
*   **Goal:** Complete the transition to `@/` aliases for imports that were deeper than one level (`../../`).
*   **Action:**
    *   Execute a command to find and replace imports starting with `../..` targeting the core folders:
        `sed -i -E "s|from ['"]\.\./\.\./(components\|context\|hooks\|models\|pages\|services\|theme\|utils)|from '@/\1|g"` (and deeper levels if necessary).
    *   Run `npm run lint -- --fix` to ensure order.
*   **Verification:** `grep -r "\.\./\.\./" src` should return no results for the targeted folders.

### Step 4: Verification & Comparison
*   **Goal:** Validate the effectiveness of the changes.
*   **Action:**
    *   Run `npm run build` again.
    *   Compare the new bundle size with the baseline (~904kB).
    *   Check `stats.html` to see if `@mui/icons-material` footprint has reduced.
*   **Verification:** The build must pass without errors, and the app must load correctly in the browser.

## 4. 🧪 Testing Strategy
*   **Unit Tests:** Existing tests must pass (`npm test` if available).
*   **Integration Tests:** Verify that icons still appear correctly in the UI (Dashboard, Mood Grid).
*   **Manual Verification:** Check the Dashboard page to ensure the Mood Grid renders icons and fetches data (verifying the aliased imports for hooks/services work).

## 5. ✅ Success Criteria
*   Frontend build passes successfully.
*   No more `../../` relative imports for core folders in `src/`.
*   Bundle size is analyzed, and impact of MUI refactoring is documented (even if the gain is small, the "why" is understood).
