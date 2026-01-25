# Implementation Plan - Refactor Documentation

## 1. 🔍 Analysis & Context
*   **Objective:** Reorganize `GEMINI.md` and `AGENTS.md` to clearly separate Project Context/Business Logic from Technical Instructions/Procedures, eliminating redundancy and optimizing token usage.
*   **Affected Files:** `GEMINI.md`, `AGENTS.md`
*   **Key Dependencies:** None
*   **Risks/Unknowns:** Risk of deleting context that might be subtle but important. Mitigation: Careful review of deleted sections to ensure they exist in the target file.

## 2. 📋 Checklist
- [x] Step 1: Read current file contents to confirm state
- [x] Step 2: Update `AGENTS.md` with missing technical details from `GEMINI.md`
- [x] Step 3: Refactor `GEMINI.md` to focus on Context & Memories
- [x] Verification

## 3. 📝 Step-by-Step Implementation Details

### 🚀 Execution Rules
1. **Interactive Flow**: Execute ONLY ONE step at a time.
2. **Atomic Updates**: Before and after each step, you MUST use `replace` to update the checklist in section 2 and the status in section 3.
3. **Status Vocabulary**: Use `[x]` (Done), `[>]` (In Progress), `[-]` (Skipped/N.A.), `[!]` (Failed/Blocked).
4. **User Confirmation**: After updating the file, stop and wait for explicit user confirmation before proceeding to the next step.
5. **No Stealth Actions**: NEVER execute code or modify files without having first updated the plan to reflect that you are about to do so.

### Step 1: Read current file contents
*   **Goal:** Capture the exact current state of documentation to ensure no data loss during transfer.
*   **Action:**
    *   Execute `read_file` for `GEMINI.md` and `AGENTS.md`.
*   **Verification:** Confirm tool output contains full text.

### Step 2: Update `AGENTS.md`
*   **Goal:** Make `AGENTS.md` the single source of truth for technical "How-To".
*   **Action:**
    *   Update `AGENTS.md` to include any technical constraints currently only found in `GEMINI.md` (e.g., specific environment variable nuances, database switching details if missing).
    *   Ensure strict separation: Commands, Style Guides, Architecture Rules.
    *   Structure:
        -   Build & Run Commands (Cheatsheet)
        -   Tech Stack & Versions
        -   Code Style & Naming Conventions
        -   Architecture Patterns (Skinny Controller, Services, etc.)
        -   Folder Structure Logic
*   **Verification:** Check `AGENTS.md` contains all build commands and coding standards.

### Step 3: Refactor `GEMINI.md`
*   **Goal:** Clean up `GEMINI.md` to be a high-level context brain.
*   **Action:**
    *   **Keep:** "Gemini Added Memories", "Project Overview" (High level), "Key Features", "Main Data Models" (Domain view), "Domain Logic".
    *   **Remove:** "Building and Running", "Development Conventions", "Technologies Used" (Detailed list), "Directory Structure" (Detailed tree).
    *   **Add:** A prominent link/note: "> For technical instructions, build commands, and code style, see `AGENTS.md`."
*   **Verification:** Check `GEMINI.md` is significantly shorter and focuses on *what* the project is, not *how* to build it.

## 4. 🧪 Testing Strategy
*   **Manual Verification:** Read both files after changes to ensure:
    *   No information was lost (just moved).
    *   No obvious duplication of "build commands" or "style guides".
    *   `GEMINI.md` clearly directs to `AGENTS.md` for technical tasks.

## 5. ✅ Success Criteria
*   `GEMINI.md` does not contain CLI commands (npm/dotnet/docker).
*   `AGENTS.md` contains all necessary commands to run and test the project.
*   User memories in `GEMINI.md` are preserved.
