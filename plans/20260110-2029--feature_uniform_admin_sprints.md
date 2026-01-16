# Implementation Plan - Uniform Admin Sprints Page

## 1. 🔍 Analysis & Context
*   **Objective:** Uniformize the "Admin Sprints" page layout to match other admin pages (like "Admin Teams"), and optimize the create form and list for compactness (inline inputs, single-line list items).
*   **Affected Files:**
    *   `app/frontend/src/pages/AdminSprintsPage.tsx`
    *   `app/frontend/src/components/AdminCreateSprintForm.tsx`
*   **Key Dependencies:** Material UI (`Grid`, `Box`, `Typography`, `ListItem`, `Chip`, etc.), `date-fns` or native `Date` (for formatting).
*   **Risks/Unknowns:** Extremely long sprint names might break the single-line list layout (mitigated by `noWrap` and responsive design).

## 2. 📋 Checklist
- [ ] Step 1: Refactor `AdminCreateSprintForm` to be inline/responsive.
- [ ] Step 2: Refactor `AdminSprintsPage` structure (remove Container, flatten layout).
- [ ] Step 3: Redesign the Sprints List for single-line compactness.
- [ ] Verification

## 3. 📝 Step-by-Step Implementation Details

### Step 1: Refactor `AdminCreateSprintForm`
*   **Goal:** Change the form from a vertical stack to a responsive inline grid.
*   **Action:**
    *   Modify `app/frontend/src/components/AdminCreateSprintForm.tsx`.
    *   Replace the main `<Stack spacing={2}>` with a `<Grid container spacing={2} alignItems="center">`.
    *   Distribute fields as follows (Desktop `md` / Mobile `xs`):
        *   **Team Select**: `md={3} xs={12}`.
        *   **Sprint Name**: `md={3} xs={12}`.
        *   **Start Date**: `md={2} xs={6}`.
        *   **End Date**: `md={2} xs={6}`.
        *   **Submit Button**: `md={2} xs={12}`.
    *   Ensure the "Submit" button has a height that matches the inputs (e.g., `sx={{ height: '56px' }}` or strict alignment).

### Step 2: Refactor `AdminSprintsPage` Structure
*   **Goal:** Remove inconsistent margins/containers and align the page title with the Admin standard.
*   **Action:**
    *   Modify `app/frontend/src/pages/AdminSprintsPage.tsx`.
    *   Replace the root `<Container maxWidth="lg" sx={{ mt: 4 }}>` with a simple `<Box>`.
    *   Update the page title `<Typography>`:
        *   Add `component="h1"`.
        *   Ensure `variant="h4"` and `gutterBottom` are present.
    *   Remove the wrapping `<Grid container>` that created the 2-column layout.
    *   Place `<AdminCreateSprintForm>` directly under the title (wrapped in a Box if needed for spacing).
    *   Add `<Divider sx={{ my: 4 }} />` after the form.
    *   Add a secondary title `<Typography variant="h5" component="h2" gutterBottom>Manage Sprints</Typography>` before the list.

### Step 3: Redesign Sprints List
*   **Goal:** Display sprints in a compact, single-line format.
*   **Action:**
    *   Modify the list rendering in `app/frontend/src/pages/AdminSprintsPage.tsx`.
    *   Inside the `.map()` loop, replace the existing `ListItemText` content with a horizontal layout:
        ```tsx
        <ListItem divider>
            <Box sx={{ display: 'flex', width: '100%', alignItems: 'center', gap: 2 }}>
                {/* Sprint Name */}
                <Typography variant="subtitle1" component="span" sx={{ fontWeight: 'bold', width: '25%', minWidth: '150px' }} noWrap>
                    {sprint.name}
                </Typography>

                {/* Team Name */}
                <Chip label={getTeamName(sprint.teamId)} size="small" variant="outlined" />

                {/* Dates (Flexible space) */}
                <Typography variant="body2" color="text.secondary" sx={{ flexGrow: 1, textAlign: 'center', display: { xs: 'none', sm: 'block' } }}>
                    {new Date(sprint.startDate).toLocaleDateString()} — {new Date(sprint.endDate).toLocaleDateString()}
                </Typography>

                {/* Mobile Dates fallback (optional, or keep hidden) */}
                
                {/* Delete Button (handled by secondaryAction or inline) */}
                <IconButton onClick={() => handleDeleteClick(sprint)} color="error" size="small">
                    <DeleteIcon />
                </IconButton>
            </Box>
        </ListItem>
        ```
    *   Remove the old `Paper` wrapper if it feels redundant with the new clean layout (or keep it if `AdminTeamsPage` uses it, but `AdminTeamsPage` seems to use `AdminTeamListItem` directly).

## 4. 🧪 Testing Strategy
*   **Manual Verification:**
    *   Navigate to `/admin/sprints`.
    *   **Layout Check:** Confirm title is top-left aligned, same vertical position as "Admin Teams".
    *   **Form Check:** Verify inputs are inline on large screens and stacked on mobile. Create a sprint to ensure functionality persists.
    *   **List Check:** Verify sprints appear as single rows. Check text truncation for long names. Check delete button functionality.

## 5. ✅ Success Criteria
*   Page title aligns perfectly with `/admin/teams`.
*   Sprint creation form uses horizontal space efficiently on desktop.
*   Sprint list is compact (one line per item).
