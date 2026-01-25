# Implementation Plan - Sprint Name Template

## 1. 🔍 Analysis & Context
*   **Objective:** Allow Team Admins to define a template (e.g., `Sprint [year] [id]`) to auto-generate sprint names during creation.
*   **Affected Files:**
    *   `api/NikoNiko.Core/Models/Team.cs`
    *   `api/NikoNiko.Core/DTOs/Team/TeamDto.cs`
    *   `api/NikoNiko.Core/DTOs/Team/TeamWithSprintsDto.cs`
    *   `api/NikoNiko.Core/DTOs/Team/UpdateTeamDto.cs`
    *   `api/NikoNiko.Services/TeamService.cs`
    *   `app/frontend/src/models/Team.ts`
    *   `app/frontend/src/models/Team/TeamWithSprintsDto.ts`
    *   `app/frontend/src/components/EditTeamDialog.tsx`
    *   `app/frontend/src/components/AdminCreateSprintForm.tsx`
    *   `app/frontend/src/pages/AdminTeamsPage.tsx`
*   **Key Dependencies:** `Entity Framework Core` (migrations), `React` (frontend logic).
*   **Risks/Unknowns:** None significant. Logic for `[id]` will rely on `sprints.length + 1`.

## 2. 📋 Checklist
- [x] Step 1: Backend Model & DTO Updates
- [x] Step 2: Database Migration
- [x] Step 3: Backend Service Update
- [x] Step 4: Frontend Model Updates
- [x] Step 5: Frontend Edit Team Dialog
- [x] Step 6: Frontend Create Sprint Logic
- [ ] Verification

## 3. 📝 Step-by-Step Implementation Details

### 🚀 Execution Rules
1. **Interactive Flow**: Execute ONLY ONE step at a time.
2. **Atomic Updates**: Before and after each step, you MUST use `replace` to update the checklist in section 2 and the status in section 3.
3. **Status Vocabulary**: Use `[x]` (Done), `[>]` (In Progress), `[-]` (Skipped/N.A.), `[!]` (Failed/Blocked).
4. **User Confirmation**: After updating the file, stop and wait for explicit user confirmation before proceeding to the next step.
5. **No Stealth Actions**: NEVER execute code or modify files without having first updated the plan to reflect that you are about to do so.

### Step 1: Backend Model & DTO Updates
*   **Goal:** Add `SprintNameTemplate` property to the Team model and relevant DTOs.
*   **Status:** [x] Done
*   **Action:**
    *   Modify `api/NikoNiko.Core/Models/Team.cs`:
        ```csharp
        [MaxLength(100)]
        public string? SprintNameTemplate { get; set; }
        ```
    *   Modify `api/NikoNiko.Core/DTOs/Team/TeamDto.cs`:
        ```csharp
        public string? SprintNameTemplate { get; init; }
        ```
    *   Modify `api/NikoNiko.Core/DTOs/Team/UpdateTeamDto.cs`:
        ```csharp
        public string? SprintNameTemplate { get; init; }
        ```
    *   Modify `api/NikoNiko.Core/DTOs/Team/TeamWithSprintsDto.cs` (if it exists and differs from `TeamDto`):
        ```csharp
        public string? SprintNameTemplate { get; init; }
        ```
*   **Verification:** Project builds (`dotnet build api/NikoNiko.Core`).

### Step 2: Database Migration
*   **Goal:** Update the database schema.
*   **Status:** [x] Done (Migration created; DB update skipped by user request)
*   **Action:**
    *   Run command:
        ```bash
        dotnet ef migrations add AddSprintNameTemplateToTeam --project api/NikoNiko.Data/NikoNiko.Data.csproj --startup-project api/NikoNiko.Api/NikoNiko.Api.csproj
        # Database update skipped per user instruction
        # dotnet ef database update --project api/NikoNiko.Data/NikoNiko.Data.csproj --startup-project api/NikoNiko.Api/NikoNiko.Api.csproj
        ```
*   **Verification:** Migration is created.

### Step 3: Backend Service Update
*   **Goal:** Map the new property in `TeamService`.
*   **Status:** [x] Done
*   **Action:**
    *   Modify `api/NikoNiko.Services/TeamService.cs`:
        *   In `GetTeamsAsync`: Add `SprintNameTemplate = t.SprintNameTemplate,` to the projection.
        *   In `GetTeamByIdAsync`: Add `SprintNameTemplate = t.SprintNameTemplate,` to the projection.
        *   In `UpdateTeamAsync`: Map `team.SprintNameTemplate = updateTeamDto.SprintNameTemplate;`.
*   **Verification:** Check if `GetTeams` endpoint returns the new field (can use `curl` or Swagger if running).

### Step 4: Frontend Model Updates
*   **Goal:** Update TypeScript interfaces.
*   **Status:** [x] Done
*   **Action:**
    *   Modify `app/frontend/src/models/Team.ts`:
        ```typescript
        export interface TeamDto {
          // ...
          sprintNameTemplate?: string;
        }
        ```
    *   Modify `app/frontend/src/models/Team/TeamWithSprintsDto.ts`:
        ```typescript
        export interface TeamWithSprintsDto {
          // ...
          sprintNameTemplate?: string;
        }
        ```
*   **Verification:** Frontend builds (`npm run build` in `app/frontend` - check for TS errors).

### Step 5: Frontend Edit Team Dialog
*   **Goal:** Allow admins to edit the template.
*   **Status:** [x] Done
*   **Action:**
    *   Modify `app/frontend/src/pages/AdminTeamsPage.tsx`:
        *   Update `handleUpdateTeam` to accept `sprintNameTemplate` argument and pass it to API.
    *   Modify `app/frontend/src/components/EditTeamDialog.tsx`:
        *   Update props interface `onUpdate` to include `sprintNameTemplate?: string`.
        *   Add state `const [sprintNameTemplate, setSprintNameTemplate] = useState(...)`.
        *   Add `TextField` for "Sprint Name Template" with helper text explaining tags: `[yyyy], [yy], [MM], [team], [id]`.
        *   Pass value in `handleSubmit`.
*   **Verification:** Open "Edit Team" dialog, see new field, save, reload to verify persistence.

### Step 6: Frontend Create Sprint Logic
*   **Goal:** Auto-generate sprint name.
*   **Status:** [x] Done
*   **Action:**
    *   Modify `app/frontend/src/components/AdminCreateSprintForm.tsx`:
        *   In `handleTeamChange`, implemented logic to generate name:
            ```typescript
            if (team.sprintNameTemplate) {
               let newName = team.sprintNameTemplate;
               const nextDate = dayjs(nextStart); // Already calculated in component
               newName = newName.replace(/\[yyyy\]/g, nextDate.format('YYYY'));
               newName = newName.replace(/\[yy\]/g, nextDate.format('YY'));
               newName = newName.replace(/\[MM\]/g, nextDate.format('MM'));
               newName = newName.replace(/\[team\]/g, team.name);
               // Simple ID logic: count + 1
               const nextId = (team.sprints?.length || 0) + 1;
               newName = newName.replace(/\[id\]/g, nextId.toString());
               setName(newName);
            }
            ```
*   **Verification:** Select a team with a template in "Create Sprint" form, verify the Name field is auto-filled correctly.

## 4. 🧪 Testing Strategy
*   **Unit Tests:**
    *   None explicitly required for this refactor, but can verify regex logic if extracted to a helper.
*   **Manual Verification:**
    1.  Go to Admin > Teams.
    2.  Edit a Team.
    3.  Set Template to `Sprint [yyyy]-[MM] #[id]`.
    4.  Save.
    5.  Go to Sprints > Create Sprint.
    6.  Select the Team.
    7.  Verify Name is pre-filled (e.g., `Sprint 2026-01 #5`).
    *   **Fix:** Added missing translation keys (`labelSprintNameTemplate`, `sprintNameTemplateHelper`) to `en.json` and `fr.json`.

## 5. ✅ Success Criteria
*   Admin can save a Sprint Name Template.
*   Creating a sprint auto-fills the name based on the template, date, and sprint count.
