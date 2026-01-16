# Implementation Plan - Frontend Framework Optimization

## 1. 🔍 Analysis & Context
*   **Objective:** Reduce frontend bundle size by replacing the heavy React 19 runtime. First try Preact (aggressive optimization), then fallback to React 18 (safe optimization).
*   **Affected Files:** `package.json`, `vite.config.ts`, `src/main.tsx` (potentially).
*   **Key Dependencies:** `react`, `react-dom`, `@mui/material`, `@mui/x-date-pickers`, `preact`, `@preact/preset-vite`.
*   **Risks/Unknowns:** Preact compatibility with MUI v7 complex components (DatePicker, DataGrid).

## 2. 📋 Checklist

### Phase 1: Option A - Preact Migration
- [ ] Create branch `feature/optimize-preact`.
- [ ] Uninstall React deps and install Preact deps.
- [ ] Configure Vite aliases (`react` -> `preact/compat`).
- [ ] Update `main.tsx` / `index.html` entry point if necessary.
- [ ] Verification: Build & Manual Test of UI components.

### Phase 2: Option B - React 18 Downgrade (Fallback)
- [ ] Create branch `feature/optimize-react18` (from develop).
- [ ] Uninstall React 19 deps.
- [ ] Install React 18 deps (`react@18`, `react-dom@18`) and types.
- [ ] Verification: Build & Manual Test.

## 3. 📝 Step-by-Step Implementation Details

### Phase 1: Preact Migration

#### Step 1: Swap Dependencies
*   **Goal:** Replace React with Preact.
*   **Action:**
    *   Run command: `npm uninstall react react-dom @types/react @types/react-dom @vitejs/plugin-react`
    *   Run command: `npm install preact @preact/preset-vite`
    *   *Note:* We rely on Preact's built-in types.

#### Step 2: Configure Vite for Compat
*   **Goal:** Trick libraries (MUI) into thinking they are using React.
*   **Action:**
    *   Modify `vite.config.ts`:
        ```typescript
        import { defineConfig } from 'vite';
        import preact from '@preact/preset-vite';
        import tsconfigPaths from 'vite-tsconfig-paths';
        import { visualizer } from 'rollup-plugin-visualizer';

        export default defineConfig({
          plugins: [
            preact(), // Replaces react()
            tsconfigPaths(),
            visualizer({ template: "raw-data", filename: "stats.json" }) // Keep visualizer
          ],
          resolve: {
            alias: {
              react: 'preact/compat',
              'react-dom/test-utils': 'preact/test-utils',
              'react-dom': 'preact/compat', // Must be below test-utils
              'react/jsx-runtime': 'preact/jsx-runtime',
            },
          },
        });
        ```

#### Step 3: Verification (Crucial)
*   **Action:**
    *   Run `npm run build` to check size.
    *   Run `npm run dev` and navigate to:
        1.  **Dashboard**: Do cards render?
        2.  **Mood Entry**: Does the DatePicker open and select a date? (High risk area).
    *   **Decision Point:** If DatePicker fails or errors abound, **ABORT Phase 1** and proceed to Phase 2.

---

### Phase 2: React 18 Downgrade (If Phase 1 fails)

#### Step 1: Downgrade Dependencies
*   **Goal:** Return to a lighter, stable React version.
*   **Action:**
    *   (If coming from Preact branch, discard changes or checkout new branch).
    *   Run command: `npm install react@18.3.1 react-dom@18.3.1`
    *   Run command: `npm install -D @types/react@18.3.3 @types/react-dom@18.3.0 @vitejs/plugin-react`

#### Step 2: Verify `main.tsx`
*   **Goal:** Ensure the entry point uses the correct API (React 18 introduced `createRoot`, React 19 kept it, so it should be compatible, but good to check imports).
*   **Action:**
    *   Check `src/main.tsx`. Ensure import is `import { createRoot } from 'react-dom/client';`.

#### Step 3: Final Verification
*   **Action:**
    *   Run `npm run build`.
    *   Compare bundle size (Expect `react-dom` to drop from ~500kb to ~130kb).

## 4. 🧪 Testing Strategy
*   **Unit Tests:** Run `npm run lint` (ensure types are recognized).
*   **Manual Verification:**
    *   **Preact:** Intensive testing of Material UI interactions (Ripple effects, Popovers, Selects, DatePickers).
    *   **React 18:** Standard regression testing (Auth flow, Dashboard load).

## 5. ✅ Success Criteria
*   **Preact Success:** Bundle size drops massively (< 400kb total), AND app is fully functional.
*   **React 18 Success:** Bundle size drops significantly (React chunk ~130kb), AND app is fully functional.
