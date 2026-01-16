# Implementation Plan - UI/UX Improvements (Login Icons & Feedback Link)

## 1. 🔍 Analysis & Context
*   **Objective:** Add provider icons to login buttons and conditionally display a bug report link in the sidebar based on an environment variable.
*   **Affected Files:**
    *   `app/frontend/src/pages/LoginPage.tsx`
    *   `app/frontend/src/components/layout/Sidebar.tsx`
*   **Key Dependencies:** `@mui/icons-material` (already installed).
*   **Risks/Unknowns:** Ensure the environment variable is correctly prefixed with `VITE_` to be exposed to the frontend.

## 2. 📋 Checklist
- [ ] Step 1: Add icons to Login Page buttons.
- [ ] Step 2: Add conditional feedback link to Sidebar.
- [ ] Verification

## 3. 📝 Step-by-Step Implementation Details

### Step 1: Add icons to Login Page buttons
*   **Goal:** Improve visual recognition of login providers.
*   **Action:**
    *   Modify `app/frontend/src/pages/LoginPage.tsx`.
    *   Import `GitHub` and `Google` icons from `@mui/icons-material`.
    *   Update `Button` components to use the `startIcon` prop:
        *   GitHub button: `startIcon={<GitHubIcon />}`
        *   Google button: `startIcon={<GoogleIcon />}`
*   **Verification:** Visually inspect the login page to ensure icons are displayed correctly next to the text.

### Step 2: Add conditional feedback link to Sidebar
*   **Goal:** Allow users to report bugs if a repository URL is configured.
*   **Action:**
    *   Modify `app/frontend/src/components/layout/Sidebar.tsx`.
    *   Import `BugReportIcon` from `@mui/icons-material`.
    *   Read the environment variable: `const repoUrl = import.meta.env.VITE_GITHUB_REPO_URL;`.
    *   In the `navItems` (or the generic list at the bottom), add a conditional check:
        ```tsx
        {repoUrl && (
          <ListItem disablePadding sx={{ display: 'block' }}>
            <ListItemButton
              component="a"
              href={repoUrl}
              target="_blank"
              rel="noopener noreferrer"
              sx={{
                 // ... keep existing styling consistency
              }}
            >
              <ListItemIcon sx={{ ... }}>
                <BugReportIcon />
              </ListItemIcon>
              <ListItemText primary="Report a Bug" sx={{ opacity: open ? 1 : 0 }} />
            </ListItemButton>
          </ListItem>
        )}
        ```
    *   Place this item appropriately (e.g., before the Logout/Theme toggle section or in the main navigation).
*   **Verification:**
    *   Start the app without the env var -> No link.
    *   Start the app with `VITE_GITHUB_REPO_URL` set -> Link appears and opens the URL in a new tab.

## 4. 🧪 Testing Strategy
*   **Manual Verification:**
    *   Go to `/login` and check icons.
    *   Set `VITE_GITHUB_REPO_URL=https://github.com/my/repo` in `.env`.
    *   Restart frontend (`npm run dev` might need a restart to pick up env vars).
    *   Check Sidebar for the "Report a Bug" link.
    *   Click the link to verify it opens the correct URL.

## 5. ✅ Success Criteria
*   Login buttons have correct GitHub and Google icons.
*   Feedback link appears ONLY when `VITE_GITHUB_REPO_URL` is defined.
*   Feedback link opens the target URL in a new tab.
