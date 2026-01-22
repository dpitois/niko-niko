# Implementation Plan - Setup Release Flow (CalVer + Changelog)

## 1. 🔍 Analysis & Context
*   **Objective:** Establish a release workflow using a `main` branch (production) and `develop` (dev). Releases will use Calendar Versioning (CalVer), generate a Conventional Changelog, and avoid git tags if possible.
*   **Affected Files:**
    *   `.git/` (Branch structure)
    *   `package.json` (Root - to be created for tooling)
    *   `CHANGELOG.md` (New file)
    *   `scripts/release.sh` (New release script)
*   **Key Dependencies:** `conventional-changelog-cli` (Node.js tool), `git`.
*   **Risks/Unknowns:**
    *   **"No Tag" constraint:** Most changelog tools rely on tags to define the "Since last release" range.
    *   **Strategy:** We will use the git history of `main` before the merge as the reference point (`--from`) to generate the changelog.
    *   **Mixed Stack:** We need to clarify where the "Version" number is officially stored. For now, we will store it in a generic `VERSION` file and/or the root `package.json`.

## 2. 📋 Checklist
- [x] Step 1: Initialize Root Node.js context (for tooling).
- [x] Step 2: Install `conventional-changelog-cli`.
- [x] Step 3: Git Branch Setup (`main` & `develop`).
- [x] Step 4: Update `.dockerignore`.
- [x] Step 5: Create Release Script (`scripts/release.sh`).
- [ ] Verification: Dry-run a release.

## 3. 📝 Step-by-Step Implementation Details

### 🚀 Execution Rules
1. **Interactive Flow**: Execute ONLY ONE step at a time.
2. **Atomic Updates**: Before and after each step, you MUST use `replace` to update the checklist in section 2 and the status in section 3.
3. **Status Vocabulary**: Use `[x]` (Done), `[>]` (In Progress), `[-]` (Skipped/N.A.), `[!]` (Failed/Blocked).
4. **User Confirmation**: After updating the file, stop and wait for explicit user confirmation before proceeding to the next step.
5. **No Stealth Actions**: NEVER execute code or modify files without having first updated the plan to reflect that you are about to do so.

### Step 1: Initialize Root Node.js context [x]
*   **Goal:** Create a `package.json` at the root to manage the release tools (since the project is mixed .NET/JS, the root is the best place for orchestration scripts).
*   **Action:**
    *   Run `npm init -y` in root.
    *   Mark as `private: true`.
*   **Verification:** `test -f package.json`

### Step 2: Install Tooling [x]
*   **Goal:** Install the changelog generator.
*   **Action:**
    *   Run `npm install --save-dev conventional-changelog-cli`.
*   **Verification:** `npx conventional-changelog --help`

### Step 3: Git Branch Setup [x]
*   **Goal:** Ensure `develop` and `main` exist and are correctly positioned.
*   **Action:**
    *   Ensure we are on `develop`.
    *   Create `main` branch starting from `develop` (if it doesn't exist) or ensure it's tracked.
    *   *Note:* The first merge will likely have no changelog or a full history changelog.
*   **Verification:** `git branch` shows both.

### Step 4: Update .dockerignore [x]
*   **Goal:** Ensure development artifacts (Plans, Script) are not included in the production Docker image, addressing the user's desire to keep `main` (production context) clean, without causing Git merge conflicts by deleting files.
*   **Action:**
    *   Append `plans/` to `.dockerignore`.
    *   Append `scripts/` to `.dockerignore`.
    *   Append `package.json` and `package-lock.json` (root) to `.dockerignore` (only needed for release script, not app runtime).
*   **Verification:** `cat .dockerignore`

### Step 5: Create Release Script [x]
*   **Goal:** Automate the "Merge -> CalVer -> Changelog" flow.
*   **Action:**
    *   Create `scripts/release.sh`.
    *   **Logic:**
        1.  **Check:** Ensure clear working directory.
        2.  **Context:** Store current `main` HEAD hash (as `PREVIOUS_HEAD`).
        3.  **Merge:** Checkout `main`, merge `develop`.
        4.  **Version:** Generate CalVer (Format: `YY.M.D` or `YYYY.MM.DD`). Update `package.json` (or `VERSION` file).
        5.  **Changelog:** 
            *   *First Run Condition:* If `CHANGELOG.md` does not exist, run with `--first-release` (or `-r 0`) to include **ALL** history.
            *   *Standard Run:* Run `conventional-changelog -p angular --from $PREVIOUS_HEAD --to HEAD`.
        6.  **Commit:** Add `CHANGELOG.md` and version file. Commit as `chore(release): [version]`.
*   **Verification:** Executable script exists.

## 4. 🧪 Testing Strategy
*   **Manual Verification:**
    1.  Create a dummy `feat:` commit on `develop`.
    2.  Run `scripts/release.sh`.
    3.  Check if `main` has the merge + the release commit.
    4.  Check if `CHANGELOG.md` contains the `feat`.

## 5. ✅ Success Criteria
*   A `main` branch exists.
*   Running the release script merges `develop` into `main`.
*   A `CHANGELOG.md` is generated/updated without requiring git tags.
*   The version follows CalVer (Date based).
