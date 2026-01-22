# Implementation Plan - Mobile Responsiveness

## 1. 🔍 Analysis & Context
*   **Objective:** Transform the frontend layout to be fully responsive, implementing a collapsible navigation drawer for mobile devices and ensuring complex data grids are usable on small screens.
*   **Affected Files:**
    *   `app/frontend/src/components/layout/AppLayout.tsx`
    *   `app/frontend/src/components/layout/Sidebar.tsx`
    *   `app/frontend/src/components/sprints/SprintMoodGrid.tsx`
*   **Key Dependencies:** Material UI (`useMediaQuery`, `useTheme`, `Drawer`, `AppBar`, `Toolbar`, `IconButton`).
*   **Risks/Unknowns:** Navigation state management (syncing the "open" state between the hamburger button and the drawer execution context).

## 2. 📋 Checklist
- [ ] Step 1: Implement Responsive Drawer logic in `AppLayout` and `Sidebar`.
- [ ] Step 2: Add Mobile AppBar (Hamburger Menu).
- [ ] Step 3: Optimize `SprintMoodGrid` for mobile scrolling.
- [ ] Verification.

## 3. 📝 Step-by-Step Implementation Details

### Step 1: Implement Responsive Drawer Logic
*   **Goal:** Make the Sidebar permanent on desktop but temporary (overlay) on mobile.
*   **Action:**
    *   Modify `app/frontend/src/components/layout/AppLayout.tsx`:
        *   Import `useTheme`, `useMediaQuery`, `AppBar`, `Toolbar`, `IconButton`, `MenuIcon`.
        *   Use `const theme = useTheme();` and `const isMobile = useMediaQuery(theme.breakpoints.down('sm'));`.
        *   Change the `open` state logic: On mobile, it should default to `false`.
        *   Pass `variant={isMobile ? 'temporary' : 'permanent'}` to the `Sidebar` (Drawer wrapper).
        *   Adjust the main content `Box` margin/width logic so it doesn't leave empty space for a hidden sidebar on mobile.
    *   Modify `app/frontend/src/components/layout/Sidebar.tsx`:
        *   Update `SidebarProps` to accept `variant` and `onClose` (for clicking the backdrop on mobile).
        *   Ensure the `Drawer` component respects these new props.

### Step 2: Add Mobile AppBar
*   **Goal:** Provide a way to open the navigation menu on mobile devices.
*   **Action:**
    *   Modify `app/frontend/src/components/layout/AppLayout.tsx`:
        *   Insert an `<AppBar position="fixed" sx={{ display: { sm: 'none' } }}>` inside the root Box, before the Drawer.
        *   Add a `<Toolbar>` with an `<IconButton>` (Hamburger icon) that triggers `handleDrawerOpen`.
        *   Add a top margin (`mt`) to the main content area *only on mobile* to account for the AppBar height (usually 56px or 64px).

### Step 3: Optimize `SprintMoodGrid` Scrolling
*   **Goal:** Prevent the wide mood grid from breaking the mobile layout by enabling contained horizontal scrolling.
*   **Action:**
    *   Modify `app/frontend/src/components/sprints/SprintMoodGrid.tsx`:
        *   Wrap the returning root `<Box>` in a parent `<Box>` (or utilize the existing root).
        *   Apply `sx={{ overflowX: 'auto', maxWidth: '100%', pb: 1 }}` to this container.
        *   Ensure `position: 'sticky'` on the first column (Member names) works within this scrolling context (it usually requires the container to *not* have `overflow: hidden`).

## 4. 🧪 Testing Strategy
*   **Manual Verification (Desktop):** Verify the sidebar is still visible and collapsible/expandable as before.
*   **Manual Verification (Mobile):**
    *   Resize browser window to < 600px.
    *   Confirm Sidebar disappears and AppBar (Hamburger) appears.
    *   Click Hamburger: Sidebar should slide in (Overlay).
    *   Click outside Sidebar: Sidebar should close.
    *   Navigate to a Sprint Grid: Verify horizontal scrolling works and member names stay visible (sticky).

## 5. ✅ Success Criteria
*   No horizontal scroll on the `<body>` element.
*   Sidebar is accessible on mobile via Hamburger menu.
*   Sidebar is permanent on desktop.
*   Mood Grid is readable on mobile via scrolling.
