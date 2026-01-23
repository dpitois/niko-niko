# Project: Niko Niko Calendar

## 1. Application Objective

Create a **distributed** and **self-hosted** (via Docker) application to allow Agile teams to track their daily morale.

- **Transparency**: Visualize collective mood.
- **Early Detection**: Identify drops in morale.
- **Empathy**: Understand team challenges.
- **Motivation**: Encourage use through gamification.

**Target Audience**: Agile Teams, Managers, Scrum Masters, and project members.

## 2. Tech Stack

- **Backend**: RESTful API in **.NET 10** (WebAPI).
  - **Architecture**: **Skinny Controllers** / **Fat Services** pattern.
  - **Contracts**: Interfaces defined in `NikoNiko.Core/Interfaces`.
  - **Logic**: Services implemented in `NikoNiko.Services`.
- **Frontend**: **React 19+** application with TypeScript, Material UI, Axios, and SWR.
- **Database**: **PostgreSQL / SQLite (configurable)**.
- **Deployment**: **Docker** (3 services: backend, frontend, db) with centralized configuration in `docker/`.

## 3. Key Features

- **Authentication**: OAuth2 (GitHub, Google, Discord). Microsoft is temporarily disabled.
- **Team Management**: Team creation (via admin dashboard), member management, and invitations (creation, acceptance, deletion).
- **Sprints**: Work period definitions by admins and sprint tracking on the dashboard, including sprint creation and a dedicated sprint creation page.
- **Mood Tracking**: Daily recording (🤩/😊/😐/☹️/😫) per sprint, now functional on the frontend and effectively updated, with a dedicated page for mood entry.
- **Real-time Notifications**: SignalR for notifying important actions.
- **Gamification**: Badge attribution to encourage participation.
- **Dashboard**: Centralized view of teams, sprints, and calendars, with basic navigation, an administration dashboard, and a "My Teams" page for the user.
- **User Logout**: Logout functionality implemented on the frontend.

## 4. Main Data Models

- `User`: User with OAuth info, teams, and badges.
- `Team`: Team with an admin, members, and sprints.
- `Sprint`: Time period with start/end dates.
- `MoodEntry`: User mood record for a given date.
- `Badge`: Gamification reward.

## Roles & Permissions

| Action (Endpoint) | Resource | `user` | `team-admin` | `super-admin` |
| :--- | :--- | :--- | :--- | :--- |
| **Teams** | | | | |
| `GET /api/teams` | List teams | Only those they are a member of | Only those they are a member/admin of | **All** |
| `GET /api/teams/{id}` | View a team | Only if member | Only if member/admin | **All** |
| `POST /api/teams` | Create a team | **No** | ✓ (becomes admin) | ✓ (becomes admin) |
| `DELETE /api/teams/{id}`| Delete a team | No | **Only their team** | **All** |
| **Users** | | | | |
| `GET /api/users` | List users | **Users from their teams** | **Users from their teams** | **All** |
| `GET /api/users/{id}` | View a user | **If in a common team** | **If in a common team** | **All** |
| `DELETE /api/users/{id}`| Delete a user | No | No | **All** |
| `DELETE /api/teams/{teamId}/users/{userId}` | Remove from a team | No | **Only from their team** | **All** |
| **Sprints** | | | | |
| `GET /api/sprints` | List sprints | **Sprints from their teams** | **Sprints from their teams** | **All** |
| `POST /api/sprints` | Create a sprint | No | **Only for their team** | **All** |
| `DELETE /api/sprints/{id}` | Delete a sprint | No | **Only from their team** | **All** |
| **Moods** | | | | |
| `GET /api/sprints/{sprintId}/moods`| List moods | **Moods of team members for this sprint** | **Moods of team members for this sprint** | **All** |
| `POST /api/moods` | Create a mood | ✓ **For themselves** | ✓ **For themselves** | ✓ **For themselves** |
| `PUT /api/moods/{id}`| Modify a mood | ✓ **Only their own**| ✓ **Only their own**| ✓ **Only their own**|
| **Invitations** | | | | |
| `GET /api/teams/{teamId}/invitations` | List invitations | No | **Only from their team** | **All** |
| `POST /api/teams/{teamId}/invitations`| Create an invitation | No | **Only for their team** | **All** |
| `DELETE /api/invitations/{id}` | Delete an invitation| No | **Only from their team** | **All** |

## 5. Authentication Configuration

For OAuth 2.0 authentication to work, you must configure external providers.

1.  **Create an OAuth 2.0 application** for each provider:
    *   [GitHub Developer Settings](https://github.com/settings/developers)
    *   [Google Cloud Console](https://console.cloud.google.com/apis/credentials)
    *   [Discord Developer Portal](https://discord.com/developers/applications)

2.  **Configure Redirect URIs**: When creating your applications, use the following callbacks for the development environment.
    *   GitHub: `http://localhost:5000/signin-github`
    *   Google: `http://localhost:5000/signin-google`
    *   Discord: `http://localhost:5000/signin-discord`

3.  **Update `appsettings.json` and your `.env` file**: Replace `ClientId` and `ClientSecret` values with your own. Also ensure the `JWT_KEY` variable is defined in `.env`.

    ```json
    "Authentication": {
      "GitHub": {
        "ClientId": "YOUR_GITHUB_CLIENT_ID",
        "ClientSecret": "YOUR_GITHUB_CLIENT_SECRET"
      },
      "Google": {
        "ClientId": "YOUR_GOOGLE_CLIENT_ID",
        "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET"
      },
      "Discord": {
        "ClientId": "YOUR_DISCORD_CLIENT_ID",
        "ClientSecret": "YOUR_DISCORD_CLIENT_SECRET"
      }
      // Microsoft is temporarily disabled.
    }
    ```

## 5.1. Database Configuration

The project can be configured to use **PostgreSQL** or **SQLite**.

- **To use SQLite (default in the `feature/back_sqlite` branch)**:
  1.  In `api/backend/appsettings.json`, ensure that `DatabaseProvider` is set to `"SQLite"`.
  2.  In `docker-compose.yml`, the `db` service (PostgreSQL) must be commented out.

- **To switch back to PostgreSQL**:
  1.  In `api/backend/appsettings.json`, change `DatabaseProvider` to `"PostgreSQL"` (or any value other than "SQLite").
  2.  In `docker-compose.yml`, uncomment the `db` service.
  3.  **Note**: EF Core migrations are provider-specific. To change databases, you may need to delete the `Migrations` folder and create new ones.

## 5.2. Managing Entity Framework Core Migrations

EF Core migrations must be executed inside the `backend` container to ensure access to the mapped SQLite database.

1.  **Ensure the `backend` service is running** (at least `docker compose up -d backend`).
2.  **Access the `backend` container shell**:
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
5.  **Migrations are applied automatically** at `backend` service startup via `dbContext.Database.Migrate()` in `Program.cs`. You don't need to run `dotnet ef database update` manually.
6.  **Exit the container shell**:
    ```bash
    exit
    ```



### 2. Run with Docker Compose

The project uses a split Docker Compose configuration to separate common settings, development specifics, and production overrides.

*   `docker-compose.yml`: Base configuration (services, images, env vars).
*   `docker-compose.override.yml`: Development overrides (ports, test services). **Loaded automatically**.
*   `docker-compose.prod.yml`: Production overrides (Traefik labels, networks).

#### Development (Default)

To build and run all services in detached mode for development (loads `base` + `override`):

```bash
docker compose up -d --build
```

#### Production

To run in production mode (loads `base` + `prod`, ignoring dev overrides):

```bash
docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d --build
```

**Note on PostgreSQL data persistence**: PostgreSQL database data is now stored in a local directory (`./postgres_data`) next to the `docker-compose.yml` file. This facilitates backup and direct management of database data for development environments.

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
    The API will typically run on `http://localhost:5000" (or as configured in `launchSettings.json`).

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

4.  Control every change using ES Lint:
    ```bash
    npm run lint
    ```
    Fix any lint or Typescript error.
