# Implementation Plan - Full Stack E2E Tests (Playwright + .NET + SQLite)

## 1. 🔍 Context & Architecture
*   **Objective:** Implement robust End-to-End (E2E) tests.
*   **Architecture:**
    *   **Test Runner:** Playwright (in `app/frontend`).
    *   **Frontend:** Vite Dev Server (served by Playwright on port 5173).
    *   **Backend:** Local .NET instance compiled with `E2E` constant (port 5000).
    *   **Notifications:** Local .NET instance (port 5001).
    *   **Database:** Dedicated SQLite file (`nikoniko.e2e.db`).
*   **Connectivity Strategy:** 
    *   Update `vite.config.ts` to add a `proxy` block for `/api` and `/notificationHub` during development/test.
*   **Security:**
    *   The `TestingController` (DB reset) is wrapped in `#if E2E`, ensuring it is physically absent from production builds.

## 2. 📋 Checklist
- [x] **Step 1: E2E Environment Configuration** (Backend & DB Setup)
- [x] **Step 2: Frontend Proxy Configuration** (Enabling communication)
- [x] **Step 3: Orchestration** (Playwright running .NET + Vite)
- [x] **Step 4: Database Reset Mechanism** (The "Clean State" guarantee)
- [>] **Step 5: Pilot Test Implementation - Authentication** (Detailed focus)
- [ ] **Step 6: Remaining Coverage Implementation** (Iterative)

## 3. 📝 Step-by-Step Details

### Step 1: E2E Environment Configuration
*   **Goal:** Prepare the backend and notification service for E2E.
*   **Actions:**
    *   Create `api/NikoNiko.Api/appsettings.E2E.json` with `Data Source=nikoniko.e2e.db`.
    *   Create `api/NikoNiko.Notifications/appsettings.E2E.json` with specific ports.
    *   Configure `Authentication__FrontendRedirectUrl` to `http://localhost:5173`.

### Step 2: Frontend Proxy Configuration
*   **Goal:** Ensure `npm run dev` (used by Playwright) can talk to the backend.
*   **Actions:**
    *   Modify `app/frontend/vite.config.ts` to add a `server.proxy` block:
        *   `/api` -> `http://localhost:5000`
        *   `/notificationHub` -> `http://localhost:5001` (with WebSocket support).

### Step 3: Orchestration
*   **Goal:** Playwright must start all services with the correct build flags.
*   **Actions:**
    *   Update `app/frontend/playwright.config.ts`.
    *   Configure `webServer` blocks:
        *   **Backend:** `dotnet run --project ../../api/NikoNiko.Api/NikoNiko.Api.csproj /p:DefineConstants="E2E;TRACE;DEBUG" --launch-profile E2E` (Port 5000).
        *   **Notifications:** `dotnet run --project ../../api/NikoNiko.Notifications/NikoNiko.Notifications.csproj` (Port 5001).
        *   **Frontend:** `npm run dev` (Port 5173).

### Step 4: Database Reset Mechanism
*   **Goal:** Allow tests to start with a blank slate.
*   **Actions:**
    *   Create `api/NikoNiko.Api/Controllers/TestingController.cs`.
    *   Wrap the entire class in `#if E2E ... #endif`.
    *   Endpoint: `POST /api/testing/reset`.
    *   Logic: Wipe DB and seed the `SuperAdmin` user.

### Step 5: Pilot Test - Authentication (Detailed)
*   **Goal:** Verify the login flow end-to-end.
*   **Scenarios:**
    1.  **Successful Login:**
        *   **Setup:** Call `/api/testing/reset`.
        *   **Action:** Fill credentials for seeded Admin.
        *   **Assert:** Redirect to Dashboard.
    2.  **Invalid Login:** Assert error message display.

### Step 6: Remaining Coverage Roadmap
*   **6.1 Team Management:** Creation, Edition, Validation.
*   **6.2 Sprint Management:** Active/Overlapping sprints.
*   **6.3 Mood Tracking:** Daily entries and business rules.
*   **6.4 Invitations:** Real flow from invitation to acceptance.
*   **6.5 SignalR:** Verify real-time UI updates when a notification is received.

## 4. 🧪 Verification Strategy
*   Run `npx playwright test`.
*   Check that `nikoniko.e2e.db` is created and used.
*   Confirm zero impact on `nikoniko.db` (dev).