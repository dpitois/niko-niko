# Implementation Plan - Personal Data Export & Frontend Integration

## 1. 🔍 Analysis & Context
*   **Objective:** Implement a GDPR-compliant data export (JSON) accessible via a user profile button. Logic will be encapsulated in a Service for better testing.
*   **Affected Files:**
    *   Backend: `api/NikoNiko.Services/IUserService.cs`, `api/NikoNiko.Services/UserService.cs`, `api/NikoNiko.Api/Controllers/UsersController.cs`, `api/NikoNiko.Api/Program.cs`, `api/NikoNiko.Core/DTOs/User/Export/`
    *   Frontend: `app/frontend/src/services/userService.ts`, `app/frontend/src/pages/ProfilePage.tsx`
*   **Key Dependencies:** `ApplicationDbContext`, `SHA256` for hashing.

## 2. 📋 Checklist
- [x] Step 1: Create Export DTOs.
- [x] Step 2: Implement `UserService` with Export Logic.
- [x] Step 3: Register Service & Update `UsersController`.
- [x] Step 4: Add Integration Test.
- [x] Step 5: Frontend Service & Component Update.
- [x] Verification: Full E2E test.

## 3. 📝 Step-by-Step Implementation Details

### 🚀 Execution Rules
1. **Interactive Flow**: Execute ONLY ONE step at a time.
2. **Atomic Updates**: Before and after each step, you MUST use `replace` to update the checklist in section 2 and the status in section 3.
3. **Status Vocabulary**: Use `[x]` (Done), `[>]` (In Progress), `[-]` (Skipped/N.A.), `[!]` (Failed/Blocked).
4. **User Confirmation**: After updating the file, stop and wait for explicit user confirmation before proceeding to the next step.
5. **No Stealth Actions**: NEVER execute code or modify files without having first updated the plan to reflect that you are about to do so.

### Step 1: Create Export DTOs
*   **Goal:** Define the JSON structure.
*   **Status:** [x] Done
*   **Action:**
    *   Create directory `api/NikoNiko.Core/DTOs/User/Export`.
    *   Create `api/NikoNiko.Core/DTOs/User/Export/UserExportDto.cs`:
        ```csharp
        namespace NikoNiko.Core.DTOs.User.Export;

        public class UserExportDto
        {
            public IdentityExportDto Identity { get; set; } = new();
            public List<TeamExportDto> Teams { get; set; } = new();
            public List<MoodExportDto> History { get; set; } = new();
        }

        public class IdentityExportDto
        {
            public string OpenId { get; set; } = string.Empty;
            public string Username { get; set; } = string.Empty;
            public string? Provider { get; set; }
            public DateTime JoinedAt { get; set; }
        }

        public class TeamExportDto
        {
            public string Id { get; set; } = string.Empty; // Normalized ID
            public string Name { get; set; } = string.Empty;
            public int MemberCount { get; set; }
        }

        public class MoodExportDto
        {
            public DateTime Date { get; set; }
            public string Mood { get; set; } = string.Empty;
            public string TeamId { get; set; } = string.Empty; // Normalized ID reference
        }
        ```

### Step 2: Implement UserService
*   **Goal:** Encapsulate export logic.
*   **Status:** [x] Done
*   **Action:**
    *   Create `api/NikoNiko.Services/IUserService.cs`
    *   Create `api/NikoNiko.Services/UserService.cs`

### Step 3: Register Service & Update Controller
*   **Goal:** Expose the feature via API.
*   **Status:** [x] Done
*   **Action:**
    *   In `api/NikoNiko.Api/Program.cs`:
        *   Add `builder.Services.AddScoped<IUserService, UserService>();` (after other services).
    *   In `api/NikoNiko.Api/Controllers/UsersController.cs`:
        *   Inject `IUserService`.
        *   Add endpoint:
        ```csharp
        [HttpGet("me/export")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ExportData([FromServices] IUserService userService)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out var userId))
            {
                return Unauthorized();
            }

            var exportDto = await userService.GetExportDataAsync(userId);
            if (exportDto == null)
            {
                return NotFound();
            }

            var jsonBytes = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(exportDto, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            var fileName = $"nikoniko-export-{DateTime.UtcNow:yyyyMMdd}.json";
            
            return File(jsonBytes, "application/json", fileName);
        }
        ```

### Step 4: Add Integration Test
*   **Goal:** Verify JSON output automatically.
*   **Status:** [x] Done
*   **Action:**
    *   Create `api/NikoNiko.Api.IntegrationTests/UsersControllerExportTests.cs`
    *   Include a test that creates a user, team, mood, requests export, and asserts the response.

### Step 5: Frontend Implementation
*   **Goal:** User Interface for export.
*   **Status:** [x] Done
*   **Action:**
    *   In `app/frontend/src/services/userService.ts`:
        ```typescript
        export const exportData = async (): Promise<Blob> => {
          const response = await axiosInstance.get('/users/me/export', {
            responseType: 'blob',
          });
          return response.data;
        };
        ```
    *   In `app/frontend/src/pages/ProfilePage.tsx`:
        *   Import `exportData` from service.
        *   Add `handleExport`:
        ```typescript
        const handleExport = async () => {
          try {
            const blob = await exportData();
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = `nikoniko-export-${new Date().toISOString().split('T')[0]}.json`;
            document.body.appendChild(a);
            a.click();
            window.URL.revokeObjectURL(url);
            document.body.removeChild(a);
          } catch (error) {
            console.error('Export failed', error);
            enqueueSnackbar(t('common.error'), { variant: 'error' });
          }
        };
        ```
        *   Connect to button `onClick={handleExport}` and remove `disabled`.

## 4. 🧪 Testing Strategy
*   **Integration:** `UsersControllerExportTests` will cover the data integrity.
*   **Manual E2E:** Click button in Frontend, verify file download and content.

## 5. ✅ Success Criteria
*   Backend tests pass.
*   Profile page button downloads a valid JSON file.