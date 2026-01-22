# Implementation Plan - Dark Mode Support

## 1. 🔍 Analysis & Context
*   **Objective:** Implement a user-selectable Light/Dark theme that persists across sessions.
*   **Affected Files:** `app/frontend/src/App.tsx`, `app/frontend/src/components/layout/Sidebar.tsx`.
*   **New Files:** `app/frontend/src/theme.ts`, `app/frontend/src/context/ColorModeContext.tsx`.
*   **Key Dependencies:** `@mui/material`, `react`, `localStorage`.
*   **Risks/Unknowns:** Ensure text contrast is sufficient in Dark Mode for existing components.

## 2. 📋 Checklist
- [ ] Step 1: Extract and Configure Theme Logic
- [ ] Step 2: Create ColorMode Context & Provider
- [ ] Step 3: Integrate Provider in App Root
- [ ] Step 4: Add Theme Toggle in Sidebar
- [ ] Verification

## 3. 📝 Step-by-Step Implementation Details

### Step 1: Extract and Configure Theme Logic
*   **Goal:** Centralize theme definition and support dynamic modes.
*   **Action:**
    *   Create `app/frontend/src/theme.ts`.
    *   Move `createTheme` logic from `App.tsx` to this file.
    *   Export a function `getTheme(mode: 'light' | 'dark')` that returns the MUI theme.
    *   Define palette for both modes (Light: existing; Dark: standard dark palette with adjusted primary).

### Step 2: Create ColorMode Context & Provider
*   **Goal:** Manage theme state and persistence.
*   **Action:**
    *   Create `app/frontend/src/context/ColorModeContext.tsx`.
    *   Define `ColorModeContext` with `{ toggleColorMode: () => void, mode: 'light' | 'dark' }`.
    *   Create `ColorModeProvider` component:
        *   State `mode` initialized from `localStorage.getItem('theme_mode')` or default to 'light'.
        *   `toggleColorMode` function that updates state and `localStorage`.
        *   Memoize the theme using `getTheme(mode)`.
        *   Render `ColorModeContext.Provider` -> `ThemeProvider` (MUI) -> `children`.
    *   Export `useColorMode` hook.

### Step 3: Integrate Provider in App Root
*   **Goal:** Apply the context to the application tree.
*   **Action:**
    *   Modify `app/frontend/src/App.tsx`.
    *   Remove local `createTheme` and `theme` variable.
    *   Import `ColorModeProvider` from `context/ColorModeContext`.
    *   Wrap the `Routes` and `SnackbarProvider` with `<ColorModeProvider>`.
    *   **Note:** `CssBaseline` should be inside the `ColorModeProvider` (implied by wrapping).

### Step 4: Add Theme Toggle in Sidebar
*   **Goal:** Allow user to switch modes.
*   **Action:**
    *   Modify `app/frontend/src/components/layout/Sidebar.tsx`.
    *   Import `useColorMode` hook from `../../context/ColorModeContext`.
    *   Import icons: `Brightness4`, `Brightness7` from `@mui/icons-material`.
    *   Add a `ListItem` with an `IconButton` to the navigation list (e.g., in the user section or bottom of nav).
    *   Bind `onClick` to `toggleColorMode`.

## 4. 🧪 Testing Strategy
*   **Manual Verification:**
    1.  Click the new Theme Toggle button.
    2.  Verify background turns dark and text turns light.
    3.  Refresh the page to verify persistence.
    4.  Logout and Login again to ensure independence from Auth state.

## 5. ✅ Success Criteria
*   User can switch between Light and Dark modes.
*   Preference is saved in LocalStorage and applied on reload.
*   UI elements remain legible in both modes.
