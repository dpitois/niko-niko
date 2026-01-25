# Implementation Plan - GDPR Account Deletion

## 1. 🔍 Analysis & Context
*   **Objective:** Implement "Right to Erasure" by anonymizing mood data (setting User to null) and ensuring these entries remain visible in the frontend under a generic "Deleted User" label.
*   **Affected Files:**
    *   `api/NikoNiko.Core/Models/MoodEntry.cs`, `TeamInvitation.cs` (Schema)
    *   `api/NikoNiko.Core/Models/UserDeletionLog.cs` (New Entity)
    *   `api/NikoNiko.Core/DTOs/Mood/MoodEntryDto.cs` (DTO update)
    *   `api/NikoNiko.Data/ApplicationDbContext.cs` (Config)
    *   `api/NikoNiko.Services/UserService.cs` (Logic)
    *   `app/frontend/src/models/Mood.ts` (Frontend Model)
    *   `app/frontend/src/components/sprints/SprintMoodGrid.tsx` (UI Logic)
    *   `app/frontend/src/pages/ProfilePage.tsx` (UX)
*   **Key Dependencies:** EF Core, React.
*   **Risks/Unknowns:** Handling `null` UserIds in frontend mapping requires careful logic in `SprintMoodGrid`.

## 2. 📋 Checklist
- [x] Step 1: Create `UserDeletionLog` Entity
- [x] Step 2: Update Data Models & DTO (Nullable FKs)
- [x] Step 3: EF Core Configuration & Migration
- [x] Step 4: Implement Anonymization Logic in `UserService`
- [x] Step 5: Update Frontend Models & Grid Logic
- [x] Step 6: Frontend Feedback Improvements
- [x] Verification

## 3. 📝 Step-by-Step Implementation Details

### 🚀 Execution Rules
1. **Interactive Flow**: Execute ONLY ONE step at a time.
2. **Atomic Updates**: Before and after each step, you MUST use `replace` to update the checklist in section 2 and the status in section 3.
3. **Status Vocabulary**: Use `[x]` (Done), `[>]` (In Progress), `[-]` (Skipped/N.A.), `[!]` (Failed/Blocked).
4. **User Confirmation**: After updating the file, stop and wait for explicit user confirmation before proceeding to the next step.
5. **No Stealth Actions**: NEVER execute code or modify files without having first updated the plan to reflect that you are about to do so.

### Step 1: Create `UserDeletionLog` Entity
*   **Status:** [x] Done
*   **Goal:** Create a persistent audit log for deleted accounts.
*   **Action:**
    *   Create `api/NikoNiko.Core/Models/UserDeletionLog.cs`.
    *   Properties: `Id` (Guid), `HashedIdentity` (string), `DeletedAt` (DateTime).
*   **Verification:** File exists.

### Step 2: Update Data Models, DTOs & Services (Nullable FKs)
*   **Status:** [x] Done
*   **Goal:** Allow `MoodEntry` and `TeamInvitation` to exist without a linked User and reflect this in DTOs and Services.
*   **Action:**
    *   Modify `api/NikoNiko.Core/Models/MoodEntry.cs`: `UserId` -> `Guid?`, `User` -> `User?`. [x]
    *   Modify `api/NikoNiko.Core/Models/TeamInvitation.cs`: `CreatorUserId` -> `Guid?`, `CreatorUser` -> `User?`. [x]
    *   Modify `api/NikoNiko.Core/DTOs/Mood/MoodEntryDto.cs`: `UserId` -> `Guid?`. [x]
    *   Modify `api/NikoNiko.Core/DTOs/Team/Invitation/TeamInvitationDto.cs`: `CreatorUserId` -> `Guid?`. [x]
    *   Modify `api/NikoNiko.Services/TeamInvitationService.cs`: Handle null creators in DTO mapping. [x]
*   **Verification:** Project builds. [x]

### Step 3: EF Core Configuration & Migration
*   **Status:** [x] Done
*   **Goal:** Apply schema changes.
*   **Action:**
    *   Modify `api/NikoNiko.Data/ApplicationDbContext.cs`: Register `UserDeletionLog`, config `MoodEntry` & `TeamInvitation` (OnDelete SetNull/ClientSetNull).
    *   Run `dotnet ef migrations add GDPR_AccountDeletion` in `api/NikoNiko.Api`.
    *   Run `docker compose restart backend`.
*   **Verification:** DB updated.

### Step 4: Implement Anonymization Logic in `UserService`
*   **Status:** [x] Done
*   **Goal:** Orchestrate deletion (Anonymize -> Log -> Delete).
*   **Action:**
    *   Modify `api/NikoNiko.Services/UserService.cs` -> `DeleteUserAsync`:
        *   Anonymize Moods (`UserId = null`).
        *   Anonymize Invitations (`CreatorUserId = null`).
        *   Create `UserDeletionLog`.
        *   Remove User.
        *   Save changes.
*   **Verification:** Manual test of deletion.

### Step 5: Update Frontend Models & Grid Logic
*   **Status:** [x] Done
*   **Goal:** Display anonymized moods as "Deleted User".
*   **Action:**
    *   Modify `app/frontend/src/models/Mood.ts`: `userId` -> `string | null`.
    *   Modify `app/frontend/src/components/sprints/SprintMoodGrid.tsx`:
        *   Detect moods with `userId === null`.
        *   If found, append a "Ghost User" (`id: 'deleted', name: t('common.deletedUser')`) to `teamMembers`.
        *   Map `mood.userId` from `null` to `'deleted'` before passing to `MoodGridDisplay` (or handle inside).
*   **Verification:** Deleted user rows appear in the grid.

### Step 6: Frontend Feedback Improvements
*   **Status:** [x] Done
*   **Goal:** Explicit confirmation of deletion.
*   **Action:**
    *   Modify `app/frontend/src/pages/ProfilePage.tsx`: Improve success feedback (Dialog or specific route) before logout.
*   **Verification:** UX test.

## 4. 🧪 Testing Strategy
*   **Unit Tests:** `DeleteUserAsync` logic.
*   **Manual Verification:**
    *   User A adds moods -> Deletes account.
    *   User B views sprint -> Sees "Deleted User" row with A's moods.
    *   DB -> User A gone, Moods `UserId` is NULL.

## 5. ✅ Success Criteria
*   Account deletion satisfies GDPR (PII removal).
*   Team mood history is preserved.
*   Frontend gracefully displays "Deleted User" for anonymized entries.
