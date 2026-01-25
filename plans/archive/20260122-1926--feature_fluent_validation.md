# Implementation Plan - Refactor Validation to FluentValidation

## 1. 🔍 Analysis & Context
*   **Objective:** Replace manual `if/else` validation with FluentValidation in the Backend AND update Frontend forms to display localized error messages based on returned error codes.
*   **Affected Files:**
    *   Backend: `NikoNiko.Api.csproj`, `Program.cs`, `SprintService.cs`, `TeamService.cs`, DTOs.
    *   Frontend: `AdminCreateSprintForm.tsx`, `CreateTeamForm.tsx`, `src/i18n/locales/*.json`.
*   **Key Dependencies:** `FluentValidation.AspNetCore` (Backend).
*   **Strategy:** Backend validators will return **Translation Keys** (e.g., `validation.required`) instead of English text. Frontend will translate these keys.

## 2. 📋 Checklist
- [x] Step 1: Install FluentValidation (Backend)
- [x] Step 2: Configure FluentValidation in Program.cs (Backend)
- [x] Step 3: Implement Sprint Validators with Error Codes (Backend)
- [x] Step 4: Implement Team Validators with Error Codes (Backend)
- [x] Step 5: Refactor SprintService (Backend)
- [x] Step 6: Refactor TeamService and DTOs (Backend)
- [x] Step 7: Create Validation Integration Tests (Backend)
- [x] Step 8: Update Frontend Translations (i18n)
- [x] Step 9: Update AdminCreateSprintForm to translate errors (Frontend)
- [x] Step 10: Update CreateTeamForm to translate errors (Frontend)
- [ ] Step 9: Update CreateTeamForm to translate errors (Frontend)
- [x] Verification

## 3. 📝 Step-by-Step Implementation Details

### 🚀 Execution Rules
1. **Interactive Flow**: Execute ONLY ONE step at a time.
2. **Atomic Updates**: Before and after each step, you MUST use `replace` to update the checklist in section 2 and the status in section 3.
3. **Status Vocabulary**: Use `[x]` (Done), `[>]` (In Progress), `[-]` (Skipped/N.A.), `[!]` (Failed/Blocked).
4. **User Confirmation**: After updating the file, stop and wait for explicit user confirmation before proceeding to the next step.

### Step 1: Install FluentValidation (Backend)
*   **Goal:** Add the necessary NuGet package.
*   **Action:**
    *   Run: `dotnet add api/NikoNiko.Api/NikoNiko.Api.csproj package FluentValidation.AspNetCore`
*   **Verification:** Check .csproj file.

### Step 2: Configure FluentValidation in Program.cs (Backend)
*   **Goal:** Register FluentValidation.
*   **Action:**
    *   Modify `api/NikoNiko.Api/Program.cs`:
        *   Add `using FluentValidation;` & `using FluentValidation.AspNetCore;`
        *   Add `builder.Services.AddFluentValidationAutoValidation();`
        *   Add `builder.Services.AddValidatorsFromAssemblyContaining<Program>();`
*   **Verification:** Project builds.

### Step 3: Implement Sprint Validators with Error Codes (Backend)
*   **Goal:** Create validators returning i18n keys.
*   **Action:**
    *   Create `api/NikoNiko.Api/Validators/Sprint/CreateSprintDtoValidator.cs`.
        ```csharp
        using FluentValidation;
        using NikoNiko.Core.DTOs.Sprint;

        namespace NikoNiko.Api.Validators.Sprint;

        public class CreateSprintDtoValidator : AbstractValidator<CreateSprintDto>
        {
            public CreateSprintDtoValidator()
            {
                RuleFor(x => x.Name).NotEmpty().WithMessage("validation.required")
                                    .MaximumLength(100).WithMessage("validation.maxLength_100");
                RuleFor(x => x.StartDate).NotEmpty().WithMessage("validation.required");
                RuleFor(x => x.EndDate).NotEmpty().WithMessage("validation.required")
                                       .GreaterThan(x => x.StartDate).WithMessage("validation.endDateBeforeStartDate");
            }
        }
        ```
    *   Create `api/NikoNiko.Api/Validators/Sprint/UpdateSprintDtoValidator.cs`.
        *   Use same rules/messages as above.
*   **Verification:** Project builds.

### Step 4: Implement Team Validators with Error Codes (Backend)
*   **Goal:** Create validators returning i18n keys.
*   **Action:**
    *   Create `api/NikoNiko.Api/Validators/Team/CreateTeamDtoValidator.cs`.
        ```csharp
        using FluentValidation;
        using NikoNiko.Core.DTOs.Team;

        namespace NikoNiko.Api.Validators.Team;

        public class CreateTeamDtoValidator : AbstractValidator<CreateTeamDto>
        {
            public CreateTeamDtoValidator()
            {
                RuleFor(x => x.Name).NotEmpty().WithMessage("validation.required")
                                    .MaximumLength(100).WithMessage("validation.maxLength_100");
            }
        }
        ```
    *   Create `api/NikoNiko.Api/Validators/Team/UpdateTeamDtoValidator.cs`.
        *   Use same rules/messages as above.
*   **Verification:** Project builds.

### Step 5: Refactor SprintService (Backend)
*   **Goal:** Remove manual checks in `SprintService.cs`.
*   **Action:**
    *   Remove `if` blocks checking dates in `CreateSprintAsync` and `UpdateSprintAsync`.
*   **Verification:** `dotnet test api/NikoNiko.Api.IntegrationTests/SprintsControllerTests.cs` (Ensure tests still pass or fail with 400 as expected).

### Step 6: Refactor TeamService and DTOs (Backend)
*   **Goal:** Clean up DTOs and TeamService.
*   **Action:**
    *   Remove DataAnnotations (`[Required]`, etc.) from `CreateTeamDto.cs`, `UpdateTeamDto.cs`, `CreateSprintDto.cs`.
*   **Verification:** `dotnet test api/NikoNiko.Api.IntegrationTests/TeamsControllerTests.cs`.

### Step 7: Update Frontend Translations (i18n)
*   **Goal:** Add validation keys to translation files.
*   **Action:**
    *   Modify `app/frontend/src/i18n/locales/en.json` and `fr.json`:
    *   Add section:
        ```json
        "validation": {
          "required": "This field is required",
          "maxLength_100": "Must be 100 characters or less",
          "endDateBeforeStartDate": "End date must be after start date"
        }
        ```
        (And French equivalents: "Ce champ est requis", "La date de fin doit être après la date de début").
*   **Verification:** Check JSON syntax.

### Step 8: Update AdminCreateSprintForm to translate errors (Frontend)
*   **Goal:** Handle server-side validation errors and translate them.
*   **Action:**
    *   Modify `app/frontend/src/components/AdminCreateSprintForm.tsx`:
        *   Add state: `const [serverErrors, setServerErrors] = useState<Record<string, string[]>>({});`
        *   In `handleSubmit`, catch block: set `serverErrors` from `error.response.data.errors`.
        *   In JSX: `helperText={serverErrors['Name'] ? t(serverErrors['Name'][0]) : ''}`
*   **Verification:** Visual check code structure.

### Step 9: Update CreateTeamForm to translate errors (Frontend)
*   **Goal:** Handle server-side validation errors and translate them.
*   **Action:**
    *   Modify `app/frontend/src/components/CreateTeamForm.tsx`:
        *   Similar implementation using `t()` on the error code.
*   **Verification:** Visual check code structure.

## 4. 🧪 Testing Strategy
*   **Integration Tests (Backend):** Verify API returns 400 with the keys (e.g. "validation.required").
*   **Manual Verification (Frontend):**
    *   Trigger validation error.
    *   Verify the displayed text matches the current language (English or French) and not the raw key.

## 5. ✅ Success Criteria
*   Backend returns consistent Error Keys.
*   Frontend properly translates these keys using `i18next`.