# Implementation Plan - Cleanup, Architecture Update & Language Standardization

## 1. 🔍 Analysis & Context
*   **Objective:** Clean up the project directory by removing obsolete files (`PLAN_Google.md`), archiving old implementation plans to declutter the root, updating the `Architecture.md` documentation to match the current codebase state (specifically .NET 10, React 19, and current directory structure), and enforcing a strict English-only policy for all future plans.
*   **Affected Files:** `PLAN_Google.md`, `plans/*.md`, `Architecture.md`, `GEMINI.md`.
*   **Key Dependencies:** None (file system operations only).
*   **Risks/Unknowns:** Minimal risk. Archiving preserves history. Architecture update requires accurate synchronization with `GEMINI.md`.

## 2. 📋 Checklist
- [x] Step 1: Enforce English-only policy in `GEMINI.md`.
- [x] Step 2: Delete obsolete file `PLAN_Google.md`.
- [x] Step 3: Create smart archiving script and clean up
- [x] Step 4: Analyze existing context (`Architecture.md` & `GEMINI.md`).
- [x] Step 5: Rewrite and update `Architecture.md`.
- [x] Verification

## 3. 📝 Step-by-Step Implementation Details

### 🚀 Execution Rules
1. **Interactive Flow**: Execute ONLY ONE step at a time.
2. **Atomic Updates**: Before and after each step, you MUST use `replace` to update the checklist in section 2 and the status in section 3.
3. **Status Vocabulary**: Use `[x]` (Done), `[>]` (In Progress), `[-]` (Skipped/N.A.), `[!]` (Failed/Blocked).
4. **User Confirmation**: After updating the file, stop and wait for explicit user confirmation before proceeding to the next step.
5. **No Stealth Actions**: NEVER execute code or modify files without having first updated the plan to reflect that you are about to do so.

### Step 1: Enforce English-only policy in `GEMINI.md`
*   **Goal:** Record the user's preference for English-only Markdown plans to ensure future consistency.
*   **Action:**
    *   Read `GEMINI.md`.
    *   Append a new memory/instruction: "All implementation plans must be written in Markdown and STRICTLY in English. No French or mixed languages in plans."
*   **Verification:** Read `GEMINI.md` to confirm the addition.

### Step 2: Delete obsolete file `PLAN_Google.md`
*   **Goal:** Remove the file identified as irrelevant by the user.
*   **Action:**
    *   Execute `rm PLAN_Google.md`.
*   **Verification:** Verify the file no longer exists using `ls PLAN_Google.md`.

### Step 3: Create smart archiving script and clean up
*   **Goal:** Create a robust, generic archiving script `plans/archive.sh` that uses `git mv` to preserve history when moving valid plan files (`YYYYMMDD-HHmm--*.md`) older than 3 days to `plans/archive/`.
*   **Action:**
    *   **Update `.gitignore`**: Add `!plans/archive/` to ensure the destination folder is tracked by Git (otherwise `git mv` might fail or files will be ignored).
    *   **Create `plans/archive.sh`**:
        *   Create `plans/archive/` directory.
        *   Calculate threshold date (Current Date - 3 days).
        *   Loop through files matching `plans/[0-9]{8}-[0-9]{4}--*.md`.
        *   Extract date from filename.
        *   If file date < threshold:
            *   Check if file is tracked (`git ls-files --error-unmatch`).
            *   If tracked: `git mv "$file" plans/archive/`.
            *   If not tracked: `mv "$file" plans/archive/`.
    *   Make executable: `chmod +x plans/archive.sh`.
    *   Execute: `./plans/archive.sh`.
*   **Verification:**
    *   Check `.gitignore` for the new exception.
    *   Verify `plans/archive.sh` handles both tracked and untracked files correctly.
    *   Verify `git status` shows renames (e.g., `renamed: plans/old.md -> plans/archive/old.md`) preserving history.
    *   Verify recent plans (<= 3 days) remain in root.

### Step 4: Analyze existing context
*   **Goal:** Retrieve the current content of `Architecture.md` and the structural facts from `GEMINI.md`.
*   **Action:**
    *   `read_file Architecture.md`
    *   `read_file GEMINI.md` (Focus on Project Overview, Architecture, Directory Structure).
*   **Verification:** Confirm content is loaded into context for the next step.

### Step 5: Rewrite and update `Architecture.md`
*   **Goal:** Update `Architecture.md` to be the technical source of truth.
*   **Action:**
    *   Use `write_file` to overwrite `Architecture.md` with consolidated content.
    *   Content must include:
        *   **Overview**: Distributed App (Docker, API .NET, Frontend React).
        *   **Technologies**: .NET 10, React 19 + Vite, PostgreSQL/SQLite, Docker Compose.
        *   **Structure**: Details of `api/` (Core, Data, Services, API, Notifications) and `app/frontend/`.
        *   **Key Concepts**: SignalR, OAuth2 Auth, "Skinny Controller" pattern, UTC Date handling.
*   **Verification:** Read the generated file to confirm accuracy.

## 4. 🧪 Testing Strategy
*   **Manual Verification:**
    *   Check `GEMINI.md` for the new rule.
    *   Check `plans/` directory structure.
    *   Review `Architecture.md` content for version accuracy (.NET 10, React 19).

## 5. ✅ Success Criteria
*   `GEMINI.md` contains the English-only plan constraint.
*   `PLAN_Google.md` is deleted.
*   The `plans/` directory only contains plans from Jan 22, 2026, and the `archive` folder.
*   `Architecture.md` is updated and reflects the current project structure.
*   This plan itself is written entirely in English.