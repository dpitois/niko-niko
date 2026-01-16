# Implementation Plan - Enable Team Management for Team Admins

## 1. 🔍 Analysis & Context
*   **Objective:** Empower "Team Admins" to manage their own teams (create sprints, invite members) directly from the Dashboard, bypassing the restricted "Admin" menu which is reserved for Super Admins.
*   **Problem:** Currently, team management features are only accessible via the `/admin` routes (hidden from non-SuperAdmins) or lack UI entry points for Team Admins, despite the Backend allowing these actions.
*   **Affected Files:**
    *   `app/frontend/src/pages/DashboardPage.tsx` (Main entry point for users)
    *   `app/frontend/src/components/CreateTeamInvitationForm.tsx` (To be reused)
*   **Key Dependencies:** `usePermissions` hook, Material UI Dialog/Modal components.
*   **Risks/Unknowns:** Ensure `CreateTeamInvitationForm` works correctly when embedded in a modal within the Dashboard context (e.g., proper props passing).

## 2. 📋 Checklist
- [ ] Step 1: Update `DashboardPage` imports and state management.
- [ ] Step 2: Implement "Team Admin Actions" UI in `TeamDashboardSection`.
- [ ] Step 3: Implement "Invite Member" Modal logic reusing `CreateTeamInvitationForm`.
- [ ] Verification

## 3. 📝 Step-by-Step Implementation Details

### Step 1: Update DashboardPage Structure
*   **Goal:** Prepare the component to handle the "Invite Member" modal state and permission checks.
*   **Action:**
    *   In `app/frontend/src/pages/DashboardPage.tsx`:
    *   Import `usePermissions` from `../hooks/usePermissions`.
    *   Import `useNavigate` from `react-router-dom`.
    *   Import `Dialog`, `DialogTitle`, `DialogContent`, `Button`, `Stack` from `@mui/material`.
    *   Import `CreateTeamInvitationForm` from `../components/CreateTeamInvitationForm`.
    *   Add state `openInviteModal` (boolean) and `selectedTeamIdForInvite` (string | null).

### Step 2: Implement Admin Actions UI
*   **Goal:** specific buttons for Team Admins on their team card.
*   **Action:**
    *   Inside `TeamDashboardSection` component:
        *   Get `isTeamAdmin` from `usePermissions`.
        *   Check `isTeamAdmin(team.id)`.
        *   If true, render a `Stack` (row) of buttons at the top or bottom of the `Card`:
            *   **Button 1:** "Create Sprint" -> `onClick={() => navigate('/sprint/create/' + team.id)}`
            *   **Button 2:** "Invite Member" -> `onClick={() => onOpenInviteModal(team.id)}` (Need to pass this handler from parent or manage local state).

*   *Refactoring Note:* It might be cleaner to lift the Modal state to the parent `DashboardPage` and pass an `onInvite` callback to `TeamDashboardSection`.

### Step 3: Integrate Invite Modal
*   **Goal:** Allow sending invitations without leaving the page.
*   **Action:**
    *   In `DashboardPage` (Parent):
        *   Render the `Dialog` component conditionally `open={openInviteModal}`.
        *   Inside `DialogContent`, render `<CreateTeamInvitationForm teamId={selectedTeamIdForInvite} onSuccess={() => setOpenInviteModal(false)} />`.
        *   *Note:* `CreateTeamInvitationForm` likely needs an `onSuccess` prop. If it doesn't have one, we might need to modify it or wrap it. (Based on previous analysis, check if it accepts a callback or redirects).
        *   *Check:* If `CreateTeamInvitationForm` redirects after success, we might want to prevent that or adjust it. *Self-correction:* Let's check `CreateTeamInvitationForm.tsx` content again during implementation. If strict redirect, we might need a small adjustment.

## 4. 🧪 Testing Strategy
*   **Manual Verification (Team Admin):**
    1.  Login as a user who is Admin of a Team (but NOT Super Admin).
    2.  Go to Dashboard (`/my-teams`).
    3.  Verify "Create Sprint" and "Invite Member" buttons appear on the team card.
    4.  Click "Create Sprint" -> Should go to creation page -> Create -> Redirect back.
    5.  Click "Invite Member" -> Modal opens -> Send Invite -> Success message -> Modal closes.
*   **Manual Verification (Regular Member):**
    1.  Login as a regular member.
    2.  Verify NO admin buttons appear on the team card.

## 5. ✅ Success Criteria
*   Team Admins can create sprints for their team from the Dashboard.
*   Team Admins can invite new users to their team from the Dashboard.
*   No "Super Admin" menu access is required for these tasks.
