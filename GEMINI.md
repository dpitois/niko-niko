# Niko Niko Calendar Project

## Project Overview

The Niko Niko Calendar is a distributed and self-hosted (via Docker) application designed to help Agile teams track their daily morale. Its core objectives are transparency of collective mood, early detection of morale drops, empathy towards team challenges, and motivation through gamification.

**Target Audience**: Agile Teams, Managers, Scrum Masters, and Project Members.

## General Architecture

The application is architected around decoupled services and communicates via RESTful APIs and SignalR for real-time notifications. It is designed to be deployed via Docker Compose, orchestrating the frontend, backend services, and the database.

*   **Frontend**: A React TypeScript application powered by Vite.
*   **Backend Services**: Multiple .NET projects (WebAPI) managing the main API and real-time notifications.
*   **Database**: PostgreSQL or SQLite (configurable) for data persistence.
*   **Containerization**: Docker and Docker Compose for managing the development and production environment.

## Technologies Used

*   **Backend**: API RESTful built with **.NET 10** (WebAPI).
*   **Frontend**: **React 19+** application with TypeScript, Vite, Material UI (MUI v7), Axios, SWR, and React Router DOM.
*   **Database**: **PostgreSQL / SQLite (configurable)**.
*   **Deployment**: **Docker** (orchestrating backend, frontend, and database services).
*   **Real-time Communication**: SignalR.

## Directory Structure

The project is divided into two main directories: `api/` for the .NET backend services and `app/frontend/` for the React frontend application.

### `api/` (Backend .NET)

This directory contains all the .NET projects that make up the application's backend.

*   **`NikoNiko.Api/`**: The main Web API project. It exposes RESTful endpoints, handles OAuth2 authentication, and integrates OpenAPI/Swagger documentation. This is the main entry point for the frontend.
    *   `Controllers/`: Contains API controllers (Auth, Badges, MoodEntries, Sprints, TeamInvitations, Teams, Users).
    *   `Authorization/`: Manages authorization requirements and handlers based on roles (Team Admin, Team Member).
    *   `appsettings.json`: Application configuration files.
    *   `Program.cs`: API application entry point, service and middleware configuration.
*   **`NikoNiko.Core/`**: Shared library project. Contains DTOs (Data Transfer Objects) for inter-layer communication, as well as data models (Entity Framework Core entities) that represent the database structure.
    *   `DTOs/`: Data Transfer Object definitions.
    *   `Models/`: Domain entity definitions.
*   **`NikoNiko.Data/`**: Data access layer. It includes the `ApplicationDbContext` (Entity Framework Core context), entity configurations, and database migrations.
    *   `Migrations/`: History of database schema changes.
*   **`NikoNiko.Data.PostgreSql/` & `NikoNiko.Data.Sqlite/`**: Library projects containing PostgreSQL and SQLite specific extensions for DbContext configuration, allowing the project to switch between databases.
*   **`NikoNiko.Notifications/`**: A dedicated backend service for real-time notifications via SignalR. It listens for internal events and broadcasts messages to connected clients.
    *   `Hubs/`: The `NotificationHub` which manages SignalR connections and message broadcasting.
    *   `Controllers/`: A controller for sending notifications (can be used by other backend services).
*   **`NikoNiko.Services/`**: Business logic layer. Contains interfaces and implementations of services that encapsulate complex business logic (e.g., team invitation management, notification services, token management).
*   **`NikoNiko.Api.IntegrationTests/` & `NikoNiko.Notifications.IntegrationTests/`**: Integration test projects for API and notification services.

### `app/frontend/` (Frontend React)

This directory contains the React web application developed with TypeScript.

*   **`src/`**: The React application source code.
    *   `App.tsx`, `main.tsx`: Root components and application entry point.
    *   `components/`: Reusable React components, including generic UI elements and specific components (e.g., `Header`, `MoodEntryForm`, `CreateTeamForm`, `layout/Sidebar`).
    *   `context/`: React contexts (e.g., `AuthContext`) for global state management.
    *   `hooks/`: Custom React hooks (e.g., `useAuth`) for encapsulating reusable logic.
    *   `models/`: TypeScript interface definitions for data consumed by the frontend, often reflecting backend DTOs.
    *   `pages/`: Page components representing different application views (e.g., Login, Dashboard, Admin/Teams, Admin/Users, Admin/Sprints, MyTeams, PastSprints).
    *   `services/`: Functions and modules for interacting with backend APIs (using Axios and SWR for data management).
*   **`public/`**: Static assets.
*   **`vite.config.ts`**: Vite configuration.
*   **`package.json`**: NPM dependencies and scripts.

## Key Features

*   **Authentication**: OAuth2 (currently GitHub functional).
*   **API Documentation**: Backend includes OpenAPI/Swagger documentation.
*   **Team Management**: Team creation (via admin dashboard), member management (admin role), team invitation system (creation, acceptance, soft deletion).
*   **Sprints**: Admin-defined work periods, sprint tracking on the dashboard, dedicated pages for creation and management.
*   **Mood Tracking**: Daily mood entry (😊/😐/🙁) per sprint, with the option to specify a date (within sprint range, not in the future).
*   **Real-time Notifications**: SignalR integration for real-time notifications on important actions.
*   **Gamification**: Planned badge attribution.
*   **Dashboard**: Centralized view of teams, sprints, and calendars. Includes navigation, administration dashboard, and "My Teams" page.
*   **User Logout**: Fully implemented frontend logout functionality.

## Main Data Models

*   `User`: Stores user information (OAuth details, associated teams, badges, Super Admin status).
*   `Team`: Represents an Agile team, linking an admin, members, and sprints.
*   `Sprint`: Defines a time-boxed work period with start and end dates.
*   `MoodEntry`: Records a user's mood for a specific date within a sprint.
*   `Badge`: Represents gamification rewards.
*   `TeamInvitation`: Manages invitations to join a team.

## Building and Running the Project

The project uses Docker Compose for orchestration, allowing you to run all services (backend, frontend, database) with a single command.

### Prerequisites

*   Docker and Docker Compose installed.
*   .NET SDK (for local backend development/testing outside Docker).
*   Node.js and npm/yarn (for local frontend development/testing outside Docker).

### 1. Configure Authentication

For OAuth 2.0 authentication (currently GitHub), you must configure external providers:

1.  **Create an OAuth 2.0 application** for GitHub:
    *   [GitHub Developer Settings](https://github.com/settings/developers)
2.  **Configure Redirect URIs**: Use the following callback for development. It is important that this URL exactly matches the one configured in your GitHub application.
    *   GitHub: `http://localhost:5000/signin-github`
3.  **Update your `.env` file**: Replace `ClientId` and `ClientSecret` values with your own. Ensure the `JWT_KEY` variable is also defined in `.env`. You can find these environment variables under the `backend` service in `docker-compose.yml`.

    ```yaml
    # Example snippet from docker-compose.yml
    backend:
      environment:
        - Authentication__GitHub__ClientId=YOUR_CLIENT_ID_GITHUB
        - Authentication__GitHub__ClientSecret=YOUR_CLIENT_SECRET_GITHUB
        - Authentication__FrontendRedirectUrl=http://localhost:3000/auth/callback # The frontend callback URL after successful authentication
        - JWT_KEY=YOUR_VERY_SECRET_KEY # Must be a strong, random key
    ```
### 1.1. Database Configuration

The project can be configured to use **PostgreSQL** or **SQLite**.

-   **To use SQLite (default in the `feature/back_sqlite` branch)**:
    1.  In `api/backend/appsettings.json`, ensure that `DatabaseProvider` is set to `"SQLite"`.
    2.  In `docker-compose.yml`, the `db` service (PostgreSQL) should be commented out.

-   **To switch back to PostgreSQL**:
    1.  In `api/backend/appsettings.json`, change `DatabaseProvider` to `"PostgreSQL"` (or any value other than "SQLite").
    2.  In `docker-compose.yml`, uncomment the `db` service.
    3.  **Note**: EF Core migrations are provider-specific. To change the database, you may need to delete the `Migrations` folder and create new ones.

### 1.2. Managing Entity Framework Core Migrations

EF Core migrations must be run inside the `backend` Docker container to ensure access to the mapped SQLite database.

1.  **Ensure the `backend` service is running** (at least `docker compose up -d backend`).
2.  **Access the `backend` container's shell**:
    ```bash
    docker compose exec backend bash
    ```
3.  **Navigate to the API project folder** inside the container:
    ```bash
    cd /app/api/NikoNiko.Api
    ```
4.  **Add a new migration** (replace `YourMigrationName` with a descriptive name):
    ```bash
    dotnet ef migrations add YourMigrationName --project ../NikoNiko.Data --startup-project .
    ```
5.  **Migrations are applied automatically** when the `backend` service starts via `dbContext.Database.Migrate()` in `Program.cs`. You do not need to run `dotnet ef database update` manually.
6.  **Exit the container shell**:
    ```bash
    exit
    ```

### 2. Run with Docker Compose (Recommended)

To build and run all services in detached mode:

```bash
docker compose up -d --build
```

To stop the services:

```bash
docker compose down
```

### 3. Local Development (Optional)

If you wish to run frontend and/or backend locally without Docker Compose, follow these steps:

#### Backend (.NET)

1.  Navigate to the `api/NikoNiko.Api` directory:
    ```bash
    cd api/NikoNiko.Api
    ```
2.  Install .NET dependencies:
    ```bash
    dotnet restore
    ```
3.  **After any change in .NET projects, execute `dotnet format` to apply code style preferences defined in `.editorconfig`**.
4.  Update `appsettings.json` with your database connection string and OAuth settings. Ensure the PostgreSQL database is running (e.g., via `docker compose up db`).
5.  Run the backend API:
    ```bash
    dotnet run
    ```
    The API will typically run on `http://localhost:5000` (or as configured in `launchSettings.json`).

#### Frontend (React)

1.  Navigate to the `app/frontend` directory:
    ```bash
    cd app/frontend
    ```
2.  Install Node.js dependencies:
    ```bash
    npm install
    # or yarn install
    ```
3.  Start the development server:
    ```bash
    npm run dev
    # or yarn dev
    ```
    The frontend application will typically be accessible at `http://localhost:5173` (or as configured by Vite).

4.  Control every changes using ES Lint:
    ```bash
    npm run lint
    ```
    Fix any lint or Typescript error.

5.  Format code using Prettier:
    ```bash
    npm run format
    ```
    Ensure code is properly formatted before committing.

## Development Conventions

*   **Project Structure**: The project is organized into an `api` directory for all .NET backend projects and an `app` directory for the frontend application.
*   **Frontend Styling**: Material UI (MUI v7) is used for all UI components and styling. Direct CSS modules are deprecated.
*   **Authentication**: Managed via `AuthContext` and `useAuth` hook for centralized state, using `react-router-dom` for routing and `axios`/`swr` for data fetching.
*   **API Calls**: Frontend uses `axios` and `swr` for data fetching.

---
## Gemini Added Memories
- The user prefers to be given the command to run the development server or start the Azure Function API, instead of being asked for permission to execute it.
- When launching the project with `docker compose up`, the user prefers the `-d` option to run services in detached mode.
- **Material UI Grid Syntax**: When using the Material UI Grid component, the correct syntax is `<Grid size={{ xs: 12, sm: 6 }}>`. The `item` prop is deprecated and should not be used.