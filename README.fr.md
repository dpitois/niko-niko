# Projet : Niko Niko Calendar

## 1. Objectif de l'Application

Créer une application **distribuée** et **auto-hébergée** (via Docker) pour permettre aux équipes Agile de suivre leur moral quotidien.

- **Transparence** : Visualiser l’humeur collective.
- **Détection précoce** : Identifier les baisses de moral.
- **Empathie** : Comprendre les défis de l'équipe.
- **Motivation** : Encourager l'utilisation via la gamification.

**Public Cible** : Équipes Agile, Managers, Scrum Masters, et membres de projets.

## 2. Stack Technique

- **Backend** : API RESTful en **.NET 10** (WebAPI).
  - **Architecture** : Pattern **Skinny Controllers** / **Fat Services**.
  - **Contrats** : Interfaces définies dans `NikoNiko.Core/Interfaces`.
  - **Logique** : Services implémentés dans `NikoNiko.Services`.
- **Frontend** : Application **React 19+** avec TypeScript, Material UI, Axios, et SWR.
- **Base de Données** : **PostgreSQL / SQLite (configurable)**.
- **Déploiement** : **Docker** (3 services : backend, frontend, db) avec une configuration centralisée dans `docker/`.

## 3. Fonctionnalités Clés

- **Authentification** : OAuth2 (GitHub, Google, Discord). Microsoft est temporairement désactivé.
- **Gestion d'Équipes** : Création d'équipes (jusqu'à 2 pour les utilisateurs réguliers), gestion des membres (invitations) et paramètres administratifs comme la **Durée de Sprint par Défaut** et les **Modèles de Nom de Sprint**.
- **Sprints** : Définition de périodes de travail avec validation stricte :
  - **Pas de chevauchement** : Les sprints d'une même équipe ne peuvent pas avoir de dates communes.
  - **Limite de durée** : Un sprint ne peut pas dépasser 2 mois (62 jours).
- **Suivi d'Humeur** : Enregistrement quotidien (🤩/😊/😐/☹️/😫) restreint à la période du sprint en cours. Les dates futures sont bloquées.
- **Notifications Temps Réel** : Intégration de SignalR pour des notifications en temps réel sur les actions importantes (création d'équipe, renommage, mises à jour des membres, entrées d'humeur).
- **Gamification** : Attribution de badges pour encourager la participation.
- **Tableau de Bord** : Vue centralisée des équipes, sprints et calendriers, avec une navigation simplifiée pour les utilisateurs et des vues administratives pour les chefs d'équipe.

## 4. Modèles de Données Principaux

- `User` : Informations de profil et identifiants OAuth associés.
- `Team` : Unité collaborative avec un administrateur, des membres, des sprints et des **paramètres par défaut**.
- `Sprint` : Période de travail définie liée à une équipe avec des dates de début/fin.
- `MoodEntry` : Enregistrements du moral des utilisateurs liés à une date et un sprint spécifiques.
- `Badge` : Récompenses de gamification.

## Rôles & Permissions

| Action (Endpoint) | Ressource | `user` | `team-admin` | `super-admin` |
| :--- | :--- | :--- | :--- | :--- |
| **Équipes** | | | | |
| `GET /api/teams` | Liste les équipes | Ses équipes | Ses équipes | **Toutes** |
| `GET /api/teams/{teamId}` | Voir une équipe | Si membre | Si membre/admin | **Toutes** |
| `POST /api/teams` | Créer une équipe | ✓ (Max 2) | ✓ (Max 2) | **Illimité** |
| `PUT /api/teams/{teamId}` | Modifier une équipe | Non | **Seulement son équipe** | **Toutes** |
| `DELETE /api/teams/{teamId}`| Supprimer une équipe | Non | **Seulement son équipe** | **Toutes** |
| **Utilisateurs** | | | | |
| `GET /api/users` | Liste les utilisateurs | Membres d'équipe | Membres d'équipe | **Tous** |
| `DELETE /api/users/me` | Supprimer son compte| ✓ **Soi-même** | ✓ **Soi-même** | **Tous** |
| `DELETE /api/users/{id}` | Supprimer utilisat. | Non | Non | **Tous** |
| `DELETE /api/teams/{teamId}/users/{userId}` | Retirer d'une équipe | Non | **Seulement de son équipe** | **Toutes** |
| **Sprints** | | | | |
| `GET /api/sprints` | Liste les sprints | Leurs équipes | Leurs équipes | **Tous** |
| `POST /api/sprints` | Créer un sprint | Non | **Seulement pour son équipe** | **Tous** |
| `PUT /api/sprints/{sprintId}` | Modifier un sprint | Non | **Seulement pour son équipe** | **Tous** |
| `DELETE /api/sprints/{sprintId}` | Supprimer un sprint | Non | **Seulement de son équipe** | **Tous** |
| **Humeurs (Moods)** | | | | |
| `GET /api/moods/bysprint/{sprintId}`| Lister les humeurs | Membres d'équipe | Membres d'équipe | **Toutes** |
| `POST /api/moods` | Enregistrer humeur | ✓ **Dans le sprint** | ✓ **Dans le sprint** | ✓ **Dans le sprint** |
| `PUT /api/moods/{id}`| Modifier une humeur | ✓ **Uniquement la sienne**| ✓ **Uniquement la sienne**| ✓ **Uniquement la sienne**|
| **Invitations** | | | | |
| `GET /api/teams/{teamId}/invitations` | Lister les invitations | Non | **Seulement pour son équipe** | **Toutes** |
| `POST /api/teams/{teamId}/invitations`| Créer une invit | Non | **Seulement pour son équipe** | **Toutes** |
| `DELETE /api/teams/{teamId}/invitations/{invitationId}` | Supprimer invit | Non | **Seulement de son équipe** | **Toutes** |

## 5. Configuration de l'Authentification

Pour que l'authentification OAuth 2.0 fonctionne, vous devez configurer les fournisseurs externes.

1.  **Créez une application OAuth 2.0** pour chaque fournisseur :
    *   [GitHub Developer Settings](https://github.com/settings/developers)
    *   [Google Cloud Console](https://console.cloud.google.com/apis/credentials)
    *   [Discord Developer Portal](https://discord.com/developers/applications)

2.  **Configurez les URI de redirection** : Lors de la création de vos applications, utilisez les callbacks suivants pour l'environnement de développement.
    *   GitHub : `http://localhost:5000/signin-github`
    *   Google : `http://localhost:5000/signin-google`
    *   Discord : `http://localhost:5000/signin-discord`

3.  **Mettez à jour `appsettings.json` et votre fichier `.env`** : Remplacez les valeurs de `ClientId` et `ClientSecret` avec les vôtres. Assurez-vous également que la variable `JWT_KEY` est définie dans `.env`.

    ```json
    "Authentication": {
      "GitHub": {
        "ClientId": "VOTRE_CLIENT_ID_GITHUB",
        "ClientSecret": "VOTRE_CLIENT_SECRET_GITHUB"
      },
      "Google": {
        "ClientId": "VOTRE_CLIENT_ID_GOOGLE",
        "ClientSecret": "VOTRE_CLIENT_SECRET_GOOGLE"
      },
      "Discord": {
        "ClientId": "VOTRE_CLIENT_ID_DISCORD",
        "ClientSecret": "VOTRE_CLIENT_SECRET_DISCORD"
      }
      // Microsoft est temporairement désactivé.
    }
    ```

## 5.1. Configuration de la Base de Données

Le projet peut être configuré pour utiliser **PostgreSQL** ou **SQLite**.

- **Pour utiliser SQLite (par défaut dans la branche `feature/back_sqlite`)** :
  1.  Dans `api/backend/appsettings.json`, assurez-vous que `DatabaseProvider` est défini sur `"SQLite"`.
  2.  Dans `docker-compose.yml`, le service `db` (PostgreSQL) doit être commenté.

- **Pour revenir à PostgreSQL** :
  1.  Dans `api/backend/appsettings.json`, changez `DatabaseProvider` pour `"PostgreSQL"` (ou toute autre valeur que "SQLite").
  2.  Dans `docker-compose.yml`, décommentez le service `db`.
  3.  **Note** : Les migrations EF Core sont spécifiques au fournisseur. Pour changer de base de données, vous devrez peut-être supprimer le dossier `Migrations` et en créer de nouvelles.

## 5.2. Gestion des Migrations Entity Framework Core

Les migrations EF Core doivent être exécutées à l'intérieur du conteneur `backend` pour assurer l'accès à la base de données SQLite mappée.

1.  **Assurez-vous que le service `backend` est lancé** (au moins `docker compose up -d backend`).
2.  **Accédez au shell du conteneur `backend`** :
    ```bash
    docker compose exec backend bash
    ```
3.  **Naviguez vers le dossier du projet API** à l'intérieur du conteneur :
    ```bash
    cd /app/api/NikoNiko.Api
    ```
4.  **Ajoutez une nouvelle migration** (remplacez `NomDeVotreMigration` par un nom descriptif) :
    ```bash
    dotnet ef migrations add NomDeVotreMigration --project ../NikoNiko.Data --startup-project .
    ```
5.  **Les migrations sont appliquées automatiquement** au démarrage du service `backend` via `dbContext.Database.Migrate()` dans `Program.cs`. Vous n'avez pas besoin d'exécuter `dotnet ef database update` manuellement.
6.  **Quittez le shell du conteneur** :
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

**Note sur la persistance des données PostgreSQL**: Les données de la base de données PostgreSQL sont désormais stockées dans un répertoire local (`./postgres_data`) à côté du fichier `docker-compose.yml`. Cela facilite la sauvegarde et la gestion directe des données de la base de données pour les environnements de développement.

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