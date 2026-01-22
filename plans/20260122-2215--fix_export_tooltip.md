# Implementation Plan - Fix User Export Tooltip

## 1. 🔍 Analysis & Context
*   **Objective:** Remove the "Feature coming soon" tooltip from the "Export My Data" button on the profile page, as the feature is fully implemented in the backend and frontend.
*   **Affected Files:**
    *   `app/frontend/src/pages/ProfilePage.tsx`
    *   `app/frontend/src/i18n/locales/en.json`
    *   `app/frontend/src/i18n/locales/fr.json`
*   **Key Dependencies:** None.
*   **Risks/Unknowns:** None. The feature `UsersController.ExportData` is implemented in the backend.

## 2. 📋 Checklist
- [x] Step 1: Remove Tooltip wrapper in ProfilePage
- [x] Step 2: Clean up i18n keys
- [x] Verification

## 3. 📝 Step-by-Step Implementation Details

### 🚀 Execution Rules
1. **Interactive Flow**: Execute ONLY ONE step at a time.
2. **Atomic Updates**: Before and after each step, you MUST use `replace` to update the checklist in section 2 and the status in section 3.
3. **Status Vocabulary**: Use `[x]` (Done), `[>]` (In Progress), `[-]` (Skipped/N.A.), `[!]` (Failed/Blocked).
4. **User Confirmation**: After updating the file, stop and wait for explicit user confirmation before proceeding to the next step.
5. **No Stealth Actions**: NEVER execute code or modify files without having first updated the plan to reflect that you are about to do so.

### Step 1: Remove Tooltip wrapper in ProfilePage
*   **Status:** [x] Done
*   **Goal:** Make the "Export Data" button directly accessible without the misleading tooltip.
*   **Action:**
    *   Modify `app/frontend/src/pages/ProfilePage.tsx`:
        *   Remove `<Tooltip title={t('profile.general.exportTooltip')}>` and its closing tag.
        *   Keep the inner `LoadingButton` (or `Button`).
*   **Verification:** Review file content.

### Step 2: Clean up i18n keys
*   **Status:** [x] Done
*   **Goal:** Remove the unused `exportTooltip` translation key.
*   **Action:**
    *   Modify `app/frontend/src/i18n/locales/en.json`: Remove `"exportTooltip": "Feature coming soon",`.
    *   Modify `app/frontend/src/i18n/locales/fr.json`: Remove `"exportTooltip": "Fonctionnalité à venir",`.
*   **Verification:** Review file content.

## 4. 🧪 Testing Strategy
*   **Manual Verification:**
    *   Go to Profile Page.
    *   Verify "Export Data" button is visible and not wrapped in a "Coming soon" tooltip.
    *   (Optional) Click the button to ensure the download starts (relies on backend).

## 5. ✅ Success Criteria
*   The "Export Data" button no longer shows "Feature coming soon".
*   The codebase is clean of unused translation keys.
