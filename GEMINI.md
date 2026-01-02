# Niko Niko Calendar Project

## Project Overview

The Niko Niko Calendar is a distributed and self-hosted (via Docker) application designed to help Agile teams track their daily morale. Its core objectives are transparency of collective mood, early detection of morale drops, empathy towards team challenges, and motivation through gamification.

**Target Audience**: Agile Teams, Managers, Scrum Masters, and Project Members.

## Technologies Used

*   **Backend**: API RESTful built with **.NET 10** (WebAPI).
*   **Frontend**: **React 19+** application with TypeScript, Material UI, Axios, and SWR.
*   **Database**: **PostgreSQL / SQLite (configurable)**.
*   **Deployment**: **Docker** (orchestrating backend, frontend, and database services).

## Key Features

*   **Authentication**: OAuth2 (currently GitHub functional; Google and Microsoft temporarily disabled).
*   **Team Management**: Creation of teams (via admin dashboard) and member management (admin role).
*   **Sprints**: Admins define work periods, and sprints are tracked on the dashboard, including dedicated pages for creation.
*   **Mood Tracking**: Daily mood entry (😊/😐/🙁) per sprint. The date of the mood can be specified, defaulting to the current day if not provided. The date must be within the sprint's date range and not in the future.
*   **Real-time Notifications**: Planned integration of SignalR for important actions.
*   **Gamification**: Planned badge attribution to encourage participation.
*   **Dashboard**: Centralized view of teams, sprints, and calendars, featuring basic navigation, an administration dashboard, and a "My Teams" page for the user.
*   **User Logout**: Frontend logout functionality is fully implemented.

## Main Data Models

*   `User`: Stores user information including OAuth details, associated teams, and badges.
*   `Team`: Represents an agile team, linking an admin, members, and sprints.
*   `Sprint`: Defines a time-boxed work period with start and end dates.
*   `MoodEntry`: Records a user's mood for a specific date within a sprint.
*   `Badge`: Represents gamification rewards.

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
2.  **Configure Redirect URIs**: Use the following callback for development:
    *   GitHub: `http://localhost:3000/signin-github`
3.  **Update `docker-compose.yml`**: Replace `ClientId` and `ClientSecret` values with your own. You can find these environment variables under the `backend` service in `docker-compose.yml`.

    ```yaml
    # Example snippet from docker-compose.yml
    backend:
      environment:
        - Authentication__GitHub__ClientId=YOUR_CLIENT_ID_GITHUB
        - Authentication__GitHub__ClientSecret=YOUR_CLIENT_SECRET_GITHUB
    ```

### 1.1. Database Configuration

The project can be configured to use **PostgreSQL** or **SQLite**.

- **To use SQLite (default in the `feature/back_sqlite` branch)**:
  1.  In `api/backend/appsettings.json`, ensure that `DatabaseProvider` is set to `"SQLite"`.
  2.  In `docker-compose.yml`, the `db` service (PostgreSQL) should be commented out.

- **To switch back to PostgreSQL**:
  1.  In `api/backend/appsettings.json`, change `DatabaseProvider` to `"PostgreSQL"` (or any value other than "SQLite").
  2.  In `docker-compose.yml`, uncomment the `db` service.
  3.  **Note**: EF Core migrations are provider-specific. To change the database, you may need to delete the `Migrations` folder and create new ones.

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

4. Control every changes using ES Lint:
   ```bash
   npm run lint
   ```
   Fix any lint or Typescript error.

## Development Conventions

*   **Project Structure**: The project is organized into an `api` directory for all .NET backend projects and an `app` directory for the frontend application.
*   **Frontend Styling**: Material UI (MUI v7) is used for all UI components and styling. Direct CSS modules are deprecated.
*   **Authentication**: Managed via `AuthContext` and `useAuth` hook for centralized state.
*   **API Calls**: Frontend uses `axios` and `swr` for data fetching.

## Next Steps (from README.md - Plan de Développement)

- **Étape 1 : Initialisation du Projet**
  - [x] Mettre en place la structure des dossiers (backend, frontend).
  - [x] Configurer `docker-compose.yml` pour les services.
- **Étape 2 : Développement Backend (.NET)**
  - [x] Créer les modèles de données et la configuration Entity Framework Core.
  - [x] Mettre en place les migrations de base de données.
  - [x] Développer les contrôleurs API de base (CRUD).
  - [x] Implémenter l'authentification OAuth 2.0 (GitHub fonctionnel, Google/Microsoft temporairement désactivés).
  - [x] Résoudre le problème d'enregistrement des dates UTC dans PostgreSQL.
  - [x] Mettre à jour l'API MoodEntry pour permettre la mise à jour des entrées existantes et la récupération par sprint/utilisateur/date.
  - [x] Mise à jour de l'API Team pour inclure les sprints dans les informations d'équipe.
  - [x] Mettre à jour l'API de création de Mood pour permettre de spécifier une date, avec validation (dans la plage du sprint, pas de date future).
  - [x] **Intégrer SignalR pour les notifications** :
    *   Créer un second projet backend (`SignalR.Service`) dédié à la gestion des connexions SignalR.
    *   Le backend actuel (`backend`) enverra des messages (ex: RabbitMQ ou autre queue légère) suite à des événements.
    *   Le `SignalR.Service` écoutera ces messages et les dispatchera aux clients connectés via SignalR.
  - [ ] **Gestion des Membres et Invitations d'Équipe (pour les Admins)**:
    *   Permettre aux admins de lister les membres de leurs équipes.
    *   Implémenter la création d'invitations d'équipe (liens web).
    *   Gérer l'acceptation de ces invitations par les utilisateurs pour rejoindre une équipe.
  - [ ] Implémenter la logique de gamification (attribution de badges).
- **Étape 3 : Développement Frontend (React)**
  - [x] Initialiser l'application React avec Vite et TypeScript.
  - [x] Mettre en place l'authentification OAuth (côté client).
  - [x] Créer les pages et composants principaux (Login, Dashboard, Callback), incluant désormais la gestion des sprints et des humeurs.
  - [x] Implémenter un tableau de bord d'administration et la création d'équipes.
  - [x] Ajouter une navigation basique et des styles initiaux.
  - [x] Intégrer SWR pour la récupération des données.
  - [x] Rendre la sauvegarde de l'humeur effective avec affichage et mise à jour.
  - [x] Créer une page dédiée "Mes Équipes" listant les équipes de l'utilisateur avec leurs sprints actifs et une redirection vers la saisie d'humeur.
  - [x] Implémenter une page dédiée pour la création de sprints.
  - [x] Implémenter la déconnexion utilisateur.
  - [x] **Connecter le client SignalR** au `SignalR.Service` pour recevoir les notifications en temps réel.
  - [x] Intégrer un sélecteur de date pour la saisie d'humeur, avec validation et liaison à l'API.
  - [ ] **Interface de Gestion des Membres et Invitations (pour les Admins)**:
    *   Développer l'UI pour lister les membres de l'équipe.
    *   Implémenter le formulaire pour créer des liens d'invitation.
    *   Gérer la logique côté client pour accepter une invitation via un lien.
- **Étape 4 : Finalisation et Tests**
  - [ ] Écrire des tests unitaires et d'intégration.
  - [ ] Rédiger la documentation finale.
  - [ ] Valider le workflow de déploiement Docker.