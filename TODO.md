# Niko Niko Calendar Project - Next Steps

## Current Task: Refactoring Admin Users Page to a Tabbed Interface

### Objectives:
*   Transform the `/admin/users` page into a central hub for user management.
*   Implement a tabbed interface for clear separation of concerns (User Management vs. Invitations).
*   Utilize Material-UI `Table` for user listing for better presentation and functionality.
*   Implement confirmation `Dialogs` for critical actions like deletion.
*   Integrate `notistack` for comprehensive user feedback (success/error notifications).

### Detailed Plan of Action

#### Pré-requis (Initial Page Setup)

1.  **Modify `AdminUsersPage.tsx`**:
    *   Set up the basic structure using Material-UI `Tabs`, `Tab`, and `TabPanel` components.
    *   Create two main tabs: "User Management" and "Invitations".
    *   Manage the selected tab's state using `React.useState`.

#### Partie 1: Tab "User Management"

1.  **Create User Service and Hook**:
    *   Create `app/frontend/src/services/userService.ts` (if not existing) with the following functions:
        *   `getUsers(): Promise<User[]>`: Fetches a list of all users (for super-admins).
        *   `deleteUser(userId: string): Promise<void>`: Deletes a user (super-admin only).
    *   Create the hook `app/frontend/src/hooks/useUsers.ts` that uses SWR and `getUsers` to fetch and cache the list of users.

2.  **Implement User Table Display**:
    *   Within the "User Management" `TabPanel`, use the `useUsers` hook to fetch user data.
    *   Display users in a Material-UI `Table` component for a professional and data-dense presentation.
    *   Table columns will include: `Avatar`, `Name`, `Email`, `Created At`, and `Actions`.
    *   The `Actions` column will contain an `IconButton` with a `DeleteIcon`.

3.  **Implement Delete User Functionality with Confirmation**:
    *   Add state to the `AdminUsersPage` component to manage the confirmation dialog's open/close state and to store the ID of the user to be deleted (e.g., `const [userToDelete, setUserToDelete] = useState<string | null>(null)`).
    *   Clicking the "Delete" button for a user will update this state to open the confirmation dialog.
    *   Create a Material-UI `Dialog` component that appears when `userToDelete` is not null, asking for confirmation.
    *   If the admin confirms deletion, a `handleConfirmDeleteUser` function will be called. This function will:
        1.  Call `userService.deleteUser(userToDelete)`.
        2.  Display a success or error notification using `notistack.enqueueSnackbar`.
        3.  Call `mutate()` from the `useUsers` hook to refresh the user table.
        4.  Close the dialog.

#### Partie 2: Tab "Invitations"

1.  **Transfer Invitation Management Logic**:
    *   Move all the relevant logic and components from `app/frontend/src/pages/TeamInvitationsPage.tsx` into the "Invitations" `TabPanel` of `AdminUsersPage.tsx`.
    *   This includes: the team selection dropdown, the `CreateTeamInvitationForm` component, the list of existing invitations, and the logic for deleting invitations.

2.  **Adapt Invitation Deletion with Confirmation**:
    *   Similar to user deletion, the deletion of an invitation will trigger a confirmation `Dialog` for improved safety. The existing `handleDeleteInvitation` function will be adapted to incorporate this dialog.

3.  **Enhance Error Feedback (Replace `console.error`)**:
    *   Replace all existing `console.error` calls within `catch` blocks (especially for invitation creation and deletion) with calls to `notistack.enqueueSnackbar` using `variant: 'error'` to provide clear user feedback.

#### Partie 3: Cleanup

1.  **Update `Sidebar.tsx`**:
    *   Ensure the "Users" link in the Admin group points to `/admin/users`.
    *   Remove the "Invitations" link, as its functionality has been consolidated.

2.  **Delete `TeamInvitationsPage.tsx`**:
    *   Once all functionality has been successfully migrated and verified, delete the file `app/frontend/src/pages/TeamInvitationsPage.tsx` from the project.
