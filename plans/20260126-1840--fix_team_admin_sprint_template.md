# Implementation Plan - Fix Team Admin Sprint Template Edit

## 1. 🔍 Analysis & Context
*   **Objective:** Fix the bug where Team Admins cannot edit the `SprintNameTemplate` from the Dashboard (CurrentSprintsPage), and ensure the Edit Dialog correctly reflects the current state when opened.
*   **Root Causes:**
    1.  `CurrentSprintsPage.tsx` fails to pass `currentSprintNameTemplate` to the `EditTeamDialog`.
    2.  `CurrentSprintsPage.tsx`'s `handleUpdateName` function ignores the `newTemplate` argument, failing to send it to the backend.
    3.  `EditTeamDialog.tsx` initializes its local state only once on mount. If the dialog is reused (kept mounted) or props change while hidden, the state becomes stale.
*   **Affected Files:**
    *   `app/frontend/src/components/EditTeamDialog.tsx`
    *   `app/frontend/src/pages/CurrentSprintsPage.tsx`
*   **Risks:** Minimal. Standard React state synchronization and prop passing fixes.

## 2. 📋 Checklist
- [x] Step 1: Fix `EditTeamDialog` state synchronization
- [x] Step 2: Fix `CurrentSprintsPage` props and handler
- [ ] Verification

## 3. 📝 Step-by-Step Implementation Details

### 🚀 Execution Rules
1. **Interactive Flow**: Execute ONLY ONE step at a time.
2. **Atomic Updates**: Before and after each step, you MUST use `replace` to update the checklist in section 2 and the status in section 3.
3. **Status Vocabulary**: Use `[x]` (Done), `[>]` (In Progress), `[-]` (Skipped/N.A.), `[!]` (Failed/Blocked).
4. **User Confirmation**: After updating the file, stop and wait for explicit user confirmation before proceeding to the next step.
5. **No Stealth Actions**: NEVER execute code or modify files without having first updated the plan to reflect that you are about to do so.

### Step 1: Fix `EditTeamDialog` state synchronization
*   **Goal:** Ensure the dialog's form fields always match the `current*` props when the dialog is opened.
*   **Action:**
    *   Modify `app/frontend/src/components/EditTeamDialog.tsx`:
        *   Import `useEffect`.
        *   Add a `useEffect` hook that resets `name`, `defaultSprintDuration`, and `sprintNameTemplate` whenever `open` becomes `true`.
*   **Code Snippet:**
    ```typescript
    useEffect(() => {
      if (open) {
        setName(currentName);
        setDefaultSprintDuration(currentDefaultSprintDuration?.toString() || '');
        setSprintNameTemplate(currentSprintNameTemplate || '');
      }
    }, [open, currentName, currentDefaultSprintDuration, currentSprintNameTemplate]);
    ```

### Step 2: Fix `CurrentSprintsPage` props and handler
*   **Goal:** Allow Team Admins to see the current template and save changes to it from the Dashboard.
*   **Action:**
    *   Modify `app/frontend/src/pages/CurrentSprintsPage.tsx`:
        *   Update `handleUpdateName` signature to accept `newTemplate?: string`.
        *   Update `handleUpdateName` implementation to pass `sprintNameTemplate: newTemplate` to `updateTeam`.
        *   Update `<EditTeamDialog />` usage to include `currentSprintNameTemplate={team.sprintNameTemplate}`.

## 4. 🧪 Testing Strategy
*   **Manual Verification (Code Review):**
    *   Check that `EditTeamDialog` has the `useEffect` hook.
    *   Check that `CurrentSprintsPage` passes the template prop.
    *   Check that `handleUpdateName` sends the template to the API.

## 5. ✅ Success Criteria
*   Team Admin can open the Edit Dialog on the Dashboard and see the existing Sprint Name Template.
*   Team Admin can modify the template and save it.
*   The change persists (backend is called with the correct data).
