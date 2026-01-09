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
- **Frontend** : Application **React 19+** avec TypeScript, Material UI, Axios, et SWR.
- **Base de Données** : **PostgreSQL / SQLite (configurable)**.
- **Déploiement** : **Docker** (3 services : backend, frontend, db).

## 3. Fonctionnalités Clés

- **Authentification** : OAuth2 (GitHub, Google). Microsoft est temporairement désactivé.
- **Gestion d'Équipes** : Création d'équipes (via le tableau de bord admin), gestion des membres et des invitations (création, acceptation, suppression).
- **Sprints** : Définition de périodes de travail par les admins et suivi des sprints sur le tableau de bord, y compris la création de sprints et une page dédiée pour la creation de sprint.
- **Suivi d'Humeur** : Enregistrement quotidien (😊/😐/🙁) par sprint, désormais fonctionnel sur le frontend et mis à jour de manière effective, avec une page dédiée pour la saisie de l'humeur.
- **Notifications Temps Réel** : SignalR pour notifier les actions importantes.
- **Gamification** : Attribution de badges pour encourager la participation.
- **Tableau de Bord** : Vue centralisée des équipes, sprints et calendriers, avec une navigation basique, un tableau de bord d'administration et une page "Mes Équipes" pour l'utilisateur.
- **Déconnexion utilisateur** : Fonctionnalité de déconnexion implémentée côté frontend.

## 4. Modèles de Données Principaux

- `User` : Utilisateur avec infos OAuth, équipes et badges.
- `Team` : Équipe avec un admin, des membres et des sprints.
- `Sprint` : Période de temps avec des dates de début/fin.
- `MoodEntry` : Enregistrement d'humeur d'un utilisateur pour une date donnée.
- `Badge` : Récompense de gamification.

## Roles & Permissions

| Action (Endpoint) | Ressource | `user` | `team-admin` | `super-admin` |
| :--- | :--- | :--- | :--- | :--- |
| **Équipes** | | | | |
| `GET /api/teams` | Lister les équipes | Uniquement celles dont il est membre | Uniquement celles dont il est membre/admin | **Toutes** |
| `GET /api/teams/{id}` | Voir une équipe | Uniquement si membre | Uniquement si membre/admin | **Toutes** |
| `POST /api/teams` | Créer une équipe | **Non** | ✓ (devient admin) | ✓ (devient admin) |
| `DELETE /api/teams/{id}`| Supprimer une équipe | Non | **Uniquement son équipe** | **Toutes** |
| **Utilisateurs** | | | | |
| `GET /api/users` | Lister les utilisateurs | **Utilisateurs de ses équipes** | **Utilisateurs de ses équipes** | **Tous** |
| `GET /api/users/{id}` | Voir un utilisateur | **Si dans une équipe commune** | **Si dans une équipe commune** | **Tous** |
| `DELETE /api/users/{id}`| Supprimer un utilisateur | Non | Non | **Tous** |
| `DELETE /api/teams/{teamId}/users/{userId}` | Retirer d'une équipe | Non | **Uniquement de son équipe** | **Toutes** |
| **Sprints** | | | | |
| `GET /api/sprints` | Lister les sprints | **Sprints de ses équipes** | **Sprints de ses équipes** | **Tous** |
| `POST /api/sprints` | Créer un sprint | Non | **Uniquement pour son équipe** | **Tous** |
| `DELETE /api/sprints/{id}` | Supprimer un sprint | Non | **Uniquement de son équipe** | **Tous** |
| **Humeurs (Moods)** | | | | |
| `GET /api/sprints/{sprintId}/moods`| Lister les humeurs | **Humeurs des membres de son équipe pour ce sprint** | **Humeurs des membres de son équipe pour ce sprint** | **Toutes** |
| `POST /api/moods` | Créer une humeur | ✓ **Pour soi-même** | ✓ **Pour soi-même** | ✓ **Pour soi-même** |
| `PUT /api/moods/{id}`| Modifier une humeur | ✓ **Uniquement la sienne**| ✓ **Uniquement la sienne**| ✓ **Uniquement la sienne**|
| **Invitations** | | | | |
| `GET /api/teams/{teamId}/invitations` | Lister les invitations | Non | **Uniquement de son équipe** | **Toutes** |
| `POST /api/teams/{teamId}/invitations`| Créer une invitation | Non | **Uniquement pour son équipe** | **Toutes** |
| `DELETE /api/invitations/{id}` | Supprimer une invitation| Non | **Uniquement de son équipe** | **Toutes** |

## 5. Configuration de l'Authentification

Pour que l'authentification OAuth 2.0 fonctionne, vous devez configurer les fournisseurs externes.

1.  **Créez une application OAuth 2.0** pour chaque fournisseur :
    *   [GitHub Developer Settings](https://github.com/settings/developers)
    *   [Google Cloud Console](https://console.cloud.google.com/apis/credentials)

2.  **Configurez les URI de redirection** : Lors de la création de vos applications, utilisez les callbacks suivants pour l'environnement de développement.
    *   GitHub : `http://localhost:5000/signin-github`
    *   Google : `http://localhost:5000/signin-google`

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

## 6. Plan de Développement

Nous allons construire cette application étape par étape, en commençant par la mise en place de l'environnement de développement, puis en développant le backend et le frontend en parallèle.

- **Étape 1 : Initialisation du Projet**
  - [x] Mettre en place la structure des dossiers (backend, frontend).
  - [x] Configurer `docker-compose.yml` pour les services.
  - [x] Réorganiser la structure du projet en déplaçant le backend dans un répertoire `api` et le frontend dans un répertoire `app`.
- **Étape 2 : Développement Backend (.NET)**
  - [x] Créer les modèles de données et la configuration Entity Framework Core.
  - [x] Mettre en place les migrations de base de données.
  - [x] Développer les contrôleurs API de base (CRUD).
  - [x] Implémenter l'authentification OAuth 2.0 (GitHub et Google fonctionnels, Microsoft temporairement désactivé).
  - [x] Résoudre le problème d'enregistrement des dates UTC dans PostgreSQL.
  - [x] Mettre à jour l'API MoodEntry pour permettre la mise à jour des entrées existantes et la récupération par sprint/utilisateur/date.
  - [x] Mise à jour de l'API Team pour inclure les sprints dans les informations d'équipe.
  - [x] Mettre à jour l'API de création de Mood pour permettre de spécifier une date, avec validation (dans la plage du sprint, pas de date future).
  - [x] **Intégrer SignalR pour les notifications** :
    *   Créer un second projet backend (`SignalR.Service`) dédié à la gestion des connexions SignalR.
    *   Le backend actuel (`backend`) enverra des messages (ex: RabbitMQ ou autre queue légère) suite à des événements.
    *   Le `SignalR.Service` écoutera ces messages et les dispatchera aux clients connectés via SignalR.
  - [x] **Gestion des Membres et Invitations d'Équipe (pour les Admins)**:
    *   [x] Permettre aux admins de lister les membres de leurs équipes.
    *   [x] Implémenter la création d'invitations d'équipe (liens web).
    *   [x] Gérer l'acceptation de ces invitations par les utilisateurs pour rejoindre une équipe.
    *   [x] Implémenter la suppression logique (`soft delete`) des invitations.
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
  - [x] **Interface de Gestion des Membres et Invitations (pour les Admins)**:
    *   [x] Développer l'UI pour lister les membres de l'équipe.
    *   [x] Implémenter le formulaire pour créer des liens d'invitation.
    *   [x] Gérer la logique côté client pour accepter une invitation via un lien.
    *   [x] Ajouter le bouton de suppression d'invitation dans l'UI.
- **Étape 4 : Finalisation et Tests**
  - [ ] Écrire des tests unitaires et d'intégration.
  - [ ] Rédiger la documentation finale.
  - [ ] Valider le workflow de déploiement Docker.

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