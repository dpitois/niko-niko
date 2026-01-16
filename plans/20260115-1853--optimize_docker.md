# Plan d'Optimisation de l'Infrastructure Docker

## 1. 🔍 Analyse & Contexte
*   **Objectif :** Moderniser, optimiser et centraliser la configuration Docker du projet. Cela inclut le regroupement des Dockerfiles dans un dossier unique `docker/` avec une nomenclature explicite (`Dockerfile.xxx`), l'optimisation des images (Alpine/Multi-stage), la mise à jour des fichiers `docker-compose` et la documentation associée.
*   **Fichiers Affectés :**
    *   `docker-compose.yml`
    *   `docker-compose.override.yml`
    *   Anciens fichiers : `api/NikoNiko.Api/Dockerfile`, `api/NikoNiko.Notifications/Dockerfile`, `app/frontend/Dockerfile`, `api/Dockerfile.test`.
    *   Nouveaux fichiers : `docker/Dockerfile.backend`, `docker/Dockerfile.notifications`, `docker/Dockerfile.frontend`, `docker/Dockerfile.test-backend`.
    *   Documentation : `README.md`, `Architecture.md`, `GEMINI.md`.
*   **Dépendances Clés :**
    *   Base Images : `.NET 10 Alpine` (Backend), `Node 24 Alpine` & `Nginx Alpine Slim` (Frontend).

## 2. 📋 Checklist
- [ ] Etape 1 : Création de la structure `docker/`.
- [ ] Etape 2 : Création de `docker/Dockerfile.backend`.
- [ ] Etape 3 : Création de `docker/Dockerfile.notifications`.
- [ ] Etape 4 : Création de `docker/Dockerfile.frontend`.
- [ ] Etape 5 : Création de `docker/Dockerfile.test-backend`.
- [ ] Etape 6 : Mise à jour de `docker-compose.yml`.
- [ ] Etape 7 : Mise à jour de `docker-compose.override.yml`.
- [ ] Etape 8 : Nettoyage des anciens Dockerfiles.
- [ ] Etape 9 : Mise à jour de la documentation (`README.md`, `Architecture.md`, `GEMINI.md`).
- [ ] Verification : Build complet, Test Run et vérification des logs.

## 3. 📝 Détails de l'implémentation étape par étape

### Etape 1 : Infrastructure
*   **Action :** Créer le répertoire `docker/` à la racine.

### Etape 2 : Dockerfile Backend
*   **Fichier :** `docker/Dockerfile.backend`
*   **Contenu :**
    *   **Stage Build :** `FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build`.
    *   **Optimisation Cache :** Copie des `.slnx` et `.csproj` (chemins relatifs `api/...`) puis `dotnet restore`.
    *   **Build :** Copie du code source `api/` et `dotnet publish` du projet `NikoNiko.Api`.
    *   **Stage Final :** `FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine`. Installation de `icu-libs` et `curl`. Copie des artefacts.

### Etape 3 : Dockerfile Notifications
*   **Fichier :** `docker/Dockerfile.notifications`
*   **Contenu :**
    *   Similaire au Backend, mais cible le projet `NikoNiko.Notifications.csproj`.

### Etape 4 : Dockerfile Frontend
*   **Fichier :** `docker/Dockerfile.frontend`
*   **Contenu :**
    *   **Stage Build :** `FROM node:24-alpine AS build`.
    *   **Contexte :** Copie de `app/frontend/package*.json` puis `npm ci`. Copie de `app/frontend/` puis `npm run build`.
    *   **Stage Final :** `FROM nginx:alpine-slim`.
    *   **Config :** Copie de `app/frontend/nginx.conf` vers `/etc/nginx/conf.d/default.conf`.
    *   **Artefacts :** Copie de `/app/dist` vers `/usr/share/nginx/html`.

### Etape 5 : Dockerfile Tests Backend
*   **Fichier :** `docker/Dockerfile.test-backend` (Migration de `api/Dockerfile.test`)
*   **Action :** Déplacer/Adapter le contenu existant pour qu'il fonctionne depuis le contexte racine (chemins `api/...`).

### Etape 6 : Mise à jour Docker Compose Principal
*   **Fichier :** `docker-compose.yml`
*   **Modifications :**
    *   Tous les services (`backend`, `notifications`, `frontend`) :
        *   `build.context`: `.` (Racine).
        *   `build.dockerfile`: `docker/Dockerfile.[service]`.

### Etape 7 : Mise à jour Docker Compose Override
*   **Fichier :** `docker-compose.override.yml`
*   **Modifications :**
    *   Service `tests-backend` :
        *   `build.context`: `.`
        *   `build.dockerfile`: `docker/Dockerfile.test-backend`.

### Etape 8 : Nettoyage
*   **Action :** Supprimer :
    *   `api/NikoNiko.Api/Dockerfile`
    *   `api/NikoNiko.Notifications/Dockerfile`
    *   `app/frontend/Dockerfile`
    *   `api/Dockerfile.test`

### Etape 9 : Mise à jour Documentation
*   **Action :**
    *   `README.md` : Mettre à jour la section "Architecture" ou "Build" si elle mentionne l'emplacement des Dockerfiles. Mentionner l'usage de `docker/`.
    *   `Architecture.md` : Vérifier si des diagrammes ou descriptions référencent les anciens emplacements.
    *   `GEMINI.md` : Ajouter une note sur la nouvelle structure Docker dans la section "Development Conventions" ou similaire.

## 4. 🧪 Stratégie de Test
1.  **Build à froid :** `docker compose build --no-cache` pour valider tous les chemins relatifs dans les nouveaux Dockerfiles.
2.  **Lancement Stack :** `docker compose up -d` (incluant l'override par défaut pour le dev).
3.  **Vérification Tests :** Vérifier que le conteneur `tests-backend` s'exécute et produit des résultats.
4.  **Vérification Services :**
    *   Backend : `curl http://localhost:5000/healthz`
    *   Notifications : `curl http://localhost:5001/healthz`
    *   Frontend : Accès `http://localhost:3000`

## 5. ✅ Critères de Succès
*   Infrastructure Docker propre et centralisée ("Flat structure" dans `docker/`).
*   Utilisation des dernières versions LTS/Alpine.
*   Aucune erreur de build liée aux contextes.
*   Documentation à jour reflétant la nouvelle structure.
