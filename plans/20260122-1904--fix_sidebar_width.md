# Implementation Plan - Fix Sidebar Width on Collapse

## 1. 🔍 Analysis & Context
*   **Objective:** Fix the display bug where collapsing the sidebar does not adjust the main content width nor the navigation container width.
*   **Affected Files:** `app/frontend/src/components/layout/AppLayout.tsx`
*   **Key Dependencies:** `@mui/material` (useTheme, styled)
*   **Risks/Unknowns:** Ensure the transition is smooth and synchronized with the internal Drawer animation.

## 2. 📋 Checklist
- [x] Step 1: Update AppLayout to use Flexbox and dynamic container widths

## 3. 📝 Step-by-Step Implementation Details

### 🚀 Execution Rules
1. **Interactive Flow**: Execute ONLY ONE step at a time.
2. **Atomic Updates**: Before and after each step, you MUST use `replace` to update the checklist in section 2 and the status in section 3.
3. **Status Vocabulary**: Use `[x]` (Done), `[>]` (In Progress), `[-]` (Skipped/N.A.), `[!]` (Failed/Blocked).
4. **User Confirmation**: After updating the file, stop and wait for explicit user confirmation before proceeding to the next step.
5. **No Stealth Actions**: NEVER execute code or modify files without having first updated the plan to reflect that you are about to do so.

### Step 1: Update AppLayout to use Flexbox and dynamic container widths [x]
*   **Goal:** Allow the Main content to naturally fill the remaining space using Flexbox (`flex-grow`) instead of manual calculations, and ensure the Nav container resizes dynamically.
*   **Action:**
    *   Modify `app/frontend/src/components/layout/AppLayout.tsx`.
    *   **Refactor `nav` Box:**
        *   Remove the static width `width: { sm: drawerWidth }`.
        *   Apply the same width logic (open vs closed) and transition as the `DesktopDrawer` to this wrapper Box so it occupies the correct amount of space in the flex flow.
    *   **Refactor `main` Box:**
        *   Remove the manual `width` calculation (`width: { sm: calc(100% - ...) }`).
        *   Keep `flexGrow: 1`.
        *   Add `minWidth: 0` to prevent flex overflow issues.
*   **Code Snippet Strategy:**
    ```ts
    // Define transition constant to reuse or use theme.transitions directly
    
    // 1. Calculate dynamic width for the Nav Box (mimicking closedMixin/openedMixin logic)
    const navWidth = open 
      ? drawerWidth 
      : `calc(${theme.spacing(7)} + 1px)`; // Default for mobile/closed
    // Note: Need to handle the sm breakpoint adjustment for closed state if strictly following closedMixin:
    // width: `calc(${theme.spacing(8)} + 1px)` on sm up.
    
    // Better approach: Apply sx that mirrors the mixins or use a styled component for the NavWrapper if complex.
    // Simple sx approach:
    /*
    <Box
      component="nav"
      sx={{
        width: { sm: open ? drawerWidth : `calc(${theme.spacing(8)} + 1px)` },
        flexShrink: { sm: 0 },
        transition: theme.transitions.create('width', {
          easing: theme.transitions.easing.sharp,
          duration: theme.transitions.duration.enteringScreen, // or leavingScreen based on state ideally, but entering is often fine for both
        }),
      }} 
    >
    */

    // 2. Cleanup Main Box
    /*
    <Box
      component="main"
      sx={{
        flexGrow: 1,
        p: 3,
        // width: REMOVED
        minWidth: 0, // ADDED
        mt: { xs: 7, sm: 0 },
        overflowX: 'hidden',
      }}
    >
    */
    ```
*   **Verification:**
    *   Verify that the main content expands automatically when the sidebar collapses.
    *   Check that the transition of the "content moving left" matches the sidebar closing animation.

## 4. 🧪 Testing Strategy
*   Manual Verification: Launch the application (`npm run dev`), log in, and toggle the sidebar.
    *   **Scenario 1:** Sidebar Open -> Main content should be pushed 240px.
    *   **Scenario 2:** Sidebar Closed -> Main content should be pushed only by the mini-variant width (~65px).
    *   **Scenario 3:** Mobile View -> Behavior should remain unchanged (temporary drawer).

## 5. ✅ Success Criteria
*   The layout uses standard Flexbox mechanics (`flex-grow`).
*   No hardcoded `calc(100% - X)` logic for the main container.
*   The sidebar animation is smooth and affects the layout immediately.
