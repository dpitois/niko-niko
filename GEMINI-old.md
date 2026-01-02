# État du Projet Niko Niko

## Travail Effectué

*   **Backend (.NET)**
    *   Initialisation du projet et de la structure Docker.
    *   Mise en place de la couche de données avec Entity Framework Core et PostgreSQL.
    *   Création des modèles de données et des contrôleurs d'API CRUD de base.
    *   Mise en place de la documentation OpenAPI/Swagger.
    *   Implémentation de la logique d'authentification OAuth 2.0 (GitHub fonctionnel, Google/Microsoft temporairement désactivés) et de la génération de token JWT.
    *   Correction du problème de taille de clé JWT (passage à HS256).
    *   Correction du problème d'enregistrement des dates UTC dans PostgreSQL pour les sprints.
    *   Mise à jour de l'API MoodEntry pour permettre la mise à jour des entrées existantes et la récupération par sprint/utilisateur/date.
    *   Mise à jour de l'API Team pour inclure les sprints dans les informations d'équipe.

*   **Frontend (React)**
    *   Initialisation du projet avec Vite et TypeScript.
    *   Installation des dépendances (`axios`, `swr`, `react-router-dom`).
    *   Mise en place d'un `Dockerfile` avec NGINX et proxy vers le backend.
    *   Création d'une page de connexion, d'une page de callback et d'un tableau de bord.
    *   Implémentation du routage et de la récupération de données de base (équipes).
    *   **Implémentation des fonctionnalités de gestion des sprints :** Création de sprints et affichage par équipe, avec une page dédiée pour la création.
    *   **Implémentation des fonctionnalités de renseignement d'humeur :** Enregistrement de l'humeur par sprint, avec affichage et mise à jour effective, via une page dédiée.
    *   **Implémentation d'un tableau de bord d'administration basique** et d'un formulaire de création d'équipes.
    *   **Mise en place d'une navigation basique** (avec un header) et d'un style initial (police, taille, couleurs, gras pour les libellés).
    *   **Implémentation de la page "Mes Équipes"** qui liste les équipes de l'utilisateur avec leur sprint en cours.
    *   **Implémentation de la déconnexion utilisateur**.
    *   Résolution des problèmes de compilation TypeScript et des dépendances (`jwt-decode`).

## Problème Actuel

*   **Authentification :** L'authentification GitHub est fonctionnelle. Google et Microsoft sont désactivés en attente de configuration ClientId/ClientSecret.

## Prochaines Étapes

1.  **Intégrer SignalR pour les notifications** : Mettre en place la communication en temps réel entre le backend et le frontend.
2.  **Implémenter la logique de gamification** : Ajouter l'attribution et l'affichage des badges.
3.  **Tests unitaires et d'intégration** : Rédiger des tests pour le backend et le frontend.
4.  **Finaliser l'interface utilisateur** : Peaufinage de l'UI/UX et ajout de validations (au-delà de ce qui a déjà été fait).
5.  **Valider le workflow de déploiement Docker**.

# Gemini Added Memories
- The user prefers to be given the command to run the development server or start the Azure Function API, instead of being asked for permission to execute it.
- When launching the project with `docker compose up`, the user prefers the `-d` option to run services in detached mode.