# Implementation Plan - Improve Light Theme Contrast

## 1. 🔍 Analysis & Context
*   **Objective:** Enhance the visual hierarchy of the light theme by introducing subtle grey backgrounds to distinguish the page background from content surfaces (Sidebar, Cards).
*   **Affected Files:**
    *   `app/frontend/src/theme.ts`
    *   `app/frontend/src/components/layout/Sidebar.tsx` (Audit for hardcoded colors)
*   **Key Dependencies:** Material UI v7 (MUI)
*   **Risks/Unknowns:** Hardcoded white backgrounds in custom components might bypass theme changes.

## 2. 📋 Checklist
- [ ] Step 1: Update Light Mode palette in `theme.ts`
- [ ] Step 2: Refine component overrides for Light Mode
- [ ] Step 3: Audit Layout components for color consistency
- [ ] Verification: Visual check and accessibility audit

## 3. 📝 Step-by-Step Implementation Details

### Step 1: Update Light Mode palette in `theme.ts`
*   **Goal:** Define distinct colors for `default` background and `paper` surfaces in Light Mode.
*   **Action:**
    *   Modify `app/frontend/src/theme.ts`.
    *   Set `palette.background.default` to `#f5f7f9` (or similar light grey) for light mode.
    *   Explicitly set `palette.background.paper` to `#ffffff` for light mode.
*   **Verification:** Check if the main background behind the Sidebar and content area turns light grey.

### Step 2: Refine component overrides for Light Mode
*   **Goal:** Ensure components like Cards and Dividers have appropriate contrast.
*   **Action:**
    *   Update `MuiDivider` style overrides if necessary to be more subtle in light mode.
    *   Ensure `MuiAppBar` in light mode (if used as white) has a subtle border or shadow to detach from the grey background.
*   **Verification:** Cards should clearly "pop" against the new grey background.

### Step 3: Audit Layout components for color consistency
*   **Goal:** Remove any hardcoded background colors that conflict with the theme.
*   **Action:**
    *   Review `Sidebar.tsx` and `AppLayout.tsx`.
    *   Ensure they use `theme.palette.background.paper` instead of literal `'white'` or `'#ffffff'`.
*   **Verification:** Ensure no "white-on-white" areas remain where contrast was intended.

## 4. 🧪 Testing Strategy
*   **Unit Tests:** N/A (Visual changes).
*   **Integration Tests:** N/A.
*   **Manual Verification:**
    1.  Switch to Light Mode.
    2.  Navigate to Dashboard: The Sidebar and the main content area should be visually separated.
    3.  Check Dialogs and Menus: They should remain readable and properly elevated.
    4.  Switch to Dark Mode: Ensure no changes were leaked to the dark theme.

## 5. ✅ Success Criteria
*   The application background in Light Mode is a subtle grey (`#f5f7f9`).
*   The Sidebar and Cards in Light Mode remain white (`#ffffff`).
*   Visual separation between navigation and content is significantly improved.
*   Dark Mode remains untouched and fully functional.
