# Implementation Plan - Architecture Documentation

## 1. 🔍 Analysis & Context
*   **Objective:** Create a comprehensive English documentation of the system's architecture in `Architecture.md` at the root and ensure all API documentation (Swagger/XML) is professional and in English.
*   **Affected Files:**
    *   `Architecture.md` (New)
    *   `api/NikoNiko.Api/NikoNiko.Api.csproj`
    *   `api/NikoNiko.Core/NikoNiko.Core.csproj`
    *   `api/NikoNiko.Api/Controllers/*.cs`
    *   `api/NikoNiko.Core/Models/*.cs`
    *   `api/NikoNiko.Core/DTOs/**/*.cs`
*   **Key Dependencies:** Mermaid.js (for diagrams), Swashbuckle.AspNetCore (for Swagger).
*   **Risks/Unknowns:** Ensuring all Mermaid diagrams accurately reflect the EF Core configuration and SignalR communication flow.

## 2. 📋 Checklist
- [ ] Step 1: Enable XML Documentation generation in .NET projects.
- [ ] Step 2: Translate and enrich XML documentation for Models and DTOs.
- [ ] Step 3: Translate and enrich XML documentation for Controllers.
- [ ] Step 4: Create the `Architecture.md` file with Mermaid diagrams.
- [ ] Verification: Validate Swagger UI and Markdown rendering.

## 3. 📝 Step-by-Step Implementation Details

### Step 1: Enable XML Documentation
*   **Goal:** Ensure the compiler generates XML files for Swagger to consume.
*   **Action:**
    *   Modify `api/NikoNiko.Api/NikoNiko.Api.csproj` and `api/NikoNiko.Core/NikoNiko.Core.csproj` to add `<GenerateDocumentationFile>true</GenerateDocumentationFile>`.
    *   Suppress warning 1591 (missing XML comments) if it becomes too noisy during transition.
*   **Verification:** Check if `.xml` files are generated in `bin/` after build.

### Step 2: Document Models and DTOs
*   **Goal:** Provide clear English descriptions for data structures.
*   **Action:**
    *   Add `<summary>` tags to all classes and properties in `api/NikoNiko.Core/Models` and `api/NikoNiko.Core/DTOs`.
    *   Translate any existing French comments found in these files.
*   **Verification:** Inspect DTOs in Swagger UI "Schemas" section.

### Step 3: Document Controllers
*   **Goal:** Document API endpoints, parameters, and response types.
*   **Action:**
    *   Add XML comments to all methods in `api/NikoNiko.Api/Controllers`.
    *   Include `<response code="...">` tags for common HTTP status codes.
*   **Verification:** Inspect endpoint descriptions in Swagger UI.

### Step 4: Create Architecture.md
*   **Goal:** High-level architectural overview in English.
*   **Action:**
    *   Create `Architecture.md` at the project root.
    *   Add **Service Architecture** section with a Mermaid `graph TD` showing Frontend, API, Notifications (SignalR), and DB.
    *   Add **Database Schema** section with a Mermaid `erDiagram` showing relationships (Users, Teams, Sprints, MoodEntries).
    *   Add **Authentication Workflow** section with a Mermaid `sequenceDiagram` showing OAuth2/JWT flow.
    *   Add **Real-time Notifications** section explaining the SignalR hub.
*   **Verification:** View the file in a Markdown previewer with Mermaid support.

## 4. 🧪 Testing Strategy
*   **Unit Tests:** N/A (Documentation focus).
*   **Integration Tests:** N/A.
*   **Manual Verification:**
    *   Run the API and navigate to `/swagger` to verify all descriptions are in English and accurate.
    *   Verify `Architecture.md` rendering on GitHub/VS Code.

## 5. ✅ Success Criteria
*   `Architecture.md` exists at the root and contains valid Mermaid diagrams in English.
*   Swagger UI displays English descriptions for all endpoints and schemas.
*   No French comments remain in the documented files.
*   The project builds without errors related to documentation generation.
