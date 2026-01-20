# Implementation Plan - Refactor Architecture Services (Global)

## 1. 🔍 Analysis & Context
*   **Objective:** Refactor the backend architecture to separate Service Interfaces and Implementations. Interfaces will be moved to `@api/NikoNiko.Core/Interfaces`, and business logic will be moved from Controllers to Services to achieve "Skinny Controllers".
*   **Affected Files:**
    *   `api/NikoNiko.Core/Interfaces/*.cs` (New location for interfaces)
    *   `api/NikoNiko.Services/*.cs` (Implementations)
    *   `api/NikoNiko.Api/Controllers/*.cs` (All controllers)
    *   `api/NikoNiko.Api/Program.cs` (DI Registration)
*   **Key Dependencies:** `NikoNiko.Data`, `NikoNiko.Core`.
*   **Risks/Unknowns:** Large scale refactoring. Risk of breaking tests or logic if not careful with context (User Claims). Context access in services (e.g. current user ID) might need `IHttpContextAccessor` or passing arguments. The current plan assumes passing arguments (UserId) is better for testability than injecting HttpContext in services.

## 2. 📋 Checklist
- [ ] Step 1: Create `Interfaces` folder in `NikoNiko.Core` and move existing interfaces.
- [ ] Step 2: Create new Service Interfaces (`ISprintService`, `IMoodService`) in `NikoNiko.Core`.
- [ ] Step 3: Implement `SprintService` and `MoodService` in `NikoNiko.Services` (Empty shells first).
- [ ] Step 4: Register new services in `Program.cs`.
- [ ] Step 5: Refactor `TeamService` (Move logic from `TeamsController`).
- [ ] Step 6: Refactor `SprintService` (Move logic from `SprintsController`).
- [ ] Step 7: Refactor `MoodService` (Move logic from `MoodEntriesController`).
- [ ] Step 8: Refactor `UserService` (Move logic from `UsersController`).
- [ ] Verification

## 3. 📝 Step-by-Step Implementation Details

### 🚀 Execution Rules
1. **Interactive Flow**: Execute ONLY ONE step at a time.
2. **Atomic Updates**: Before and after each step, you MUST use `replace` to update the checklist in section 2 and the status in section 3.
3. **Status Vocabulary**: Use `[x]` (Done), `[>]` (In Progress), `[-]` (Skipped/N.A.), `[!]` (Failed/Blocked).
4. **User Confirmation**: After updating the file, stop and wait for explicit user confirmation before proceeding to the next step.
5. **No Stealth Actions**: NEVER execute code or modify files without having first updated the plan to reflect that you are about to do so.

### Step 1: Move Interfaces to Core
*   **Goal:** Centralize abstractions in Core project.
*   **Action:**
    *   Create directory `api/NikoNiko.Core/Interfaces`.
    *   Move `ITeamService.cs`, `IUserService.cs`, `ITokenService.cs`, `ITeamInvitationService.cs`, `INotificationService.cs` from `api/NikoNiko.Services/` to `api/NikoNiko.Core/Interfaces/`.
    *   Update namespaces in these files to `NikoNiko.Core.Interfaces`.
    *   Update references in `NikoNiko.Services` (implementations) and `NikoNiko.Api` (controllers/program).
*   **Verification:** Build solution.

### Step 2: Define New Interfaces
*   **Goal:** Define contracts for logic currently in controllers.
*   **Action:**
    *   Create `api/NikoNiko.Core/Interfaces/ISprintService.cs`.
    *   Create `api/NikoNiko.Core/Interfaces/IMoodService.cs`.
    *   *Note:* Signatures will mimic controller actions but take `userId` or `isSuperAdmin` as explicit parameters where needed.
*   **Verification:** File creation.

### Step 3: Implement New Services (Shells)
*   **Goal:** Prepare classes for logic transfer.
*   **Action:**
    *   Create `api/NikoNiko.Services/SprintService.cs` implementing `ISprintService`.
    *   Create `api/NikoNiko.Services/MoodService.cs` implementing `IMoodService`.
    *   Inject `ApplicationDbContext` into them.
*   **Verification:** Build solution.

### Step 4: Register Services
*   **Goal:** Ensure DI container knows about new services.
*   **Action:**
    *   Modify `api/NikoNiko.Api/Program.cs`: Add `builder.Services.AddScoped<ISprintService, SprintService>();` and `builder.Services.AddScoped<IMoodService, MoodService>();`.
    *   Ensure namespaces are correct.
*   **Verification:** Build solution.

### Step 5: Refactor TeamService
*   **Goal:** Move logic from `TeamsController` to `TeamService`.
*   **Action:**
    *   Update `ITeamService` with methods: `GetTeamsAsync`, `GetTeamAsync`, `CreateTeamAsync`, `UpdateTeamAsync`, `DeleteTeamAsync`, `TransferAdminAsync`, `RemoveUserFromTeamAsync`.
    *   Implement logic in `TeamService` (copying from Controller).
    *   Inject `ITeamService` into `TeamsController` and replace logic with calls.
*   **Verification:** Integration Tests for Teams.

### Step 6: Refactor SprintService
*   **Goal:** Move logic from `SprintsController` to `SprintService`.
*   **Action:**
    *   Update `ISprintService` with methods: `GetSprintsAsync`, `GetSprintAsync`, `CreateSprintAsync`, `UpdateSprintAsync`, `DeleteSprintAsync`.
    *   Implement logic in `SprintService` (validation, overlap checks, etc.).
    *   Update `SprintsController` to use `ISprintService`.
*   **Verification:** Integration Tests for Sprints.

### Step 7: Refactor MoodService
*   **Goal:** Move logic from `MoodEntriesController` to `MoodService`.
*   **Action:**
    *   Update `IMoodService` with methods: `GetMoodEntriesAsync`, `GetMyMoodEntriesAsync`, `GetMoodEntryAsync`, `CreateMoodEntryAsync`, `GetMoodEntriesBySprintAsync`.
    *   Implement logic in `MoodService` (timezone logic, future date checks, notification calls).
    *   Update `MoodEntriesController` to use `IMoodService`.
*   **Verification:** Integration Tests for Moods.

### Step 8: Refactor UserService
*   **Goal:** Move logic from `UsersController` to `UserService`.
*   **Action:**
    *   Update `IUserService` with methods: `GetUsersAsync`, `GetUserAsync`, `DeleteUserAsync` (handling team admin checks).
    *   Implement logic in `UserService`.
    *   Update `UsersController` to use `IUserService`.
*   **Verification:** Integration Tests for Users.

## 4. 🧪 Testing Strategy
*   Integration Tests: Run all existing integration tests to ensure no regression.
    *   `dotnet test api/NikoNiko.Api.IntegrationTests/`
    *   `dotnet test api/NikoNiko.Notifications.IntegrationTests/`

## 5. ✅ Success Criteria
*   All interfaces reside in `NikoNiko.Core/Interfaces`.
*   All Controllers are "Skinny" (mostly just calling services and mapping results).
*   No logic regression (verified by tests).
