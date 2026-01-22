# Implementation Plan - Notification UX Improvements

## 1. 🔍 Analysis & Context
*   **Objective:** Enhance the notification system to limit the number of visible notifications (stacking), prevent duplicates, and allow users to manually dismiss them.
*   **Affected Files:** `app/frontend/src/App.tsx`
*   **Key Dependencies:** `notistack` (SnackbarProvider), `@mui/material` (IconButton, Icons).
*   **Risks/Unknowns:** Correctly accessing `closeSnackbar` from outside the context (using Ref) to implement the global close button.

## 2. 📋 Checklist
- [ ] Step 1: Create a Ref for the SnackbarProvider in `App.tsx`.
- [ ] Step 2: Implement the global `dismissAction` (Close button).
- [ ] Step 3: Update `SnackbarProvider` configuration (maxSnack, anchorOrigin, preventDuplicate, action).
- [ ] Verification.

## 3. 📝 Step-by-Step Implementation Details

### Step 1 & 2: Ref and Dismiss Action
*   **Goal:** Create the logic to close a specific snackbar programmatically via a button.
*   **Action:**
    *   Modify `app/frontend/src/App.tsx`:
        *   Import `useRef` (or `createRef`) from `react`.
        *   Import `IconButton` from `@mui/material` and `Close` from `@mui/icons-material`.
        *   Instantiate `const notistackRef = useRef<SnackbarProvider>(null);` inside the `App` component (or outside if using createRef, but useRef inside is safer for HMR).
        *   Create `const onClickDismiss = (key: any) => () => { notistackRef.current?.closeSnackbar(key); };`

### Step 3: Update SnackbarProvider Config
*   **Goal:** Apply the limits and the new close action.
*   **Action:**
    *   Modify `app/frontend/src/App.tsx`:
        *   Update `<SnackbarProvider>` props:
            *   `ref={notistackRef}`
            *   `maxSnack={5}` (User requested 3-5).
            *   `preventDuplicate` (Boolean prop).
            *   `anchorOrigin={{ vertical: 'bottom', horizontal: 'left' }}` (Standard position).
            *   `action={(key) => (<IconButton onClick={onClickDismiss(key)} color="inherit"><Close /></IconButton>)}`

## 4. 🧪 Testing Strategy
*   **Manual Verification:**
    *   Trigger multiple notifications rapidly (e.g., via clicking Mood or Login errors).
    *   Verify only 5 appear stacked.
    *   Verify the Close (X) button is present on each.
    *   Click the X and verify the notification disappears instantly.

## 5. ✅ Success Criteria
*   Max 5 notifications visible.
*   Notifications are manually dismissible.
*   Duplicate notifications are suppressed (if identical text).
