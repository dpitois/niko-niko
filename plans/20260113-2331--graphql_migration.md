# Plan de Migration - Introduction de GraphQL (Backend & Frontend)

## 1. 🔍 Analyse & Contexte
*   **Objectif :** Mettre en place une API GraphQL sur le backend (.NET) avec une parité CRUD complète, et intégrer sa consommation dans le frontend (React) pour optimiser le Dashboard.
*   **Fichiers Affectés :**
    *   **Backend:** `api/NikoNiko.Api/Program.cs`, `api/NikoNiko.Api/GraphQL/`, `api/NikoNiko.Api.IntegrationTests/`.
    *   **Frontend:** `app/frontend/package.json`, `app/frontend/src/lib/apolloClient.ts`, `app/frontend/src/pages/DashboardPage.tsx`, `app/frontend/src/components/sprints/SprintMoodGrid.tsx`.

## 2. 📋 Checklist
- [x] **Étape 1 : Infrastructure & Configuration (Backend).**
- [x] **Étape 2 : Configuration du Proxy Nginx.**
- [x] **Étape 3 : Définition des Types et Queries.**
    - [x] Exposition du champ `members` sur `TeamType`.
- [x] **Étape 4 : Implémentation des Mutations.**
- [x] **Étape 5 : Sécurisation & Tests (Backend).**
- [x] **Étape 6 : Intégration Frontend.**
    - [x] Installation Apollo Client (@apollo/client v3).
    - [x] Configuration du `ApolloProvider` avec Auth JWT.
    - [x] Création Query `GET_MY_TEAMS_DASHBOARD`.
    - [x] Création Mutation `ADD_MOOD_ENTRY`.
    - [x] Migration de `DashboardPage` (REST -> GraphQL).
    - [x] Migration de `SprintMoodGrid` (REST -> GraphQL Mutation).
    - [x] Validation du build `npm run build`.

## 3. 📝 Détails d'Implémentation

### Intégration Frontend (Réalisée)
L'intégration Frontend a permis de démontrer la puissance de GraphQL :
1.  **Réduction des requêtes :** Au lieu de faire 1 requête `/api/teams` + N requêtes `/api/teams/{id}/sprints`, le Dashboard charge désormais **tout** (Equipes, Membres, Sprints, Humeurs) en **une seule requête GraphQL**.
2.  **Optimistic UI :** La grille d'humeur utilise toujours une mise à jour optimiste pour une réactivité immédiate, connectée désormais à une Mutation GraphQL.

### Backend
Le backend expose désormais un champ `members` sur le type `Team`, résolu via la navigation `TeamUsers`.

## 4. 🧪 Stratégie de Test & Requirements
*   **Requirement :** Toute Query ou Mutation GraphQL définie dans l'application frontend **doit** impérativement faire l'objet d'un test d'intégration correspondant dans `api/NikoNiko.Api.IntegrationTests/GraphQLTests.cs`.
*   **Tests d'Intégration Réalisés :**
    - `GetMe_ShouldReturnCurrentUser`
    - `GetMyTeams_ShouldReturnOnlyUserTeams`
    - `GetTeam_ShouldReturnTeam_WhenUserIsMember`
    - `AddMoodEntry_ShouldCreateEntry_WhenValid`
    - `GetMyTeamsDashboard_ShouldReturnFullData` (Couvre la Query complexe du Dashboard)
    - `CreateTeam`, `CreateSprint`.
*   **Couverture :** Validation de l'authentification, de la structure des réponses (UUID, types énumérés), et de la logique métier.

## 5. 🚀 Prochaines Étapes
*   **Généralisation de GraphQL :** Procéder à la migration des autres écrans (AdminTeams, AdminUsers, PastSprints) pour homogénéiser l'architecture et bénéficier des performances de GraphQL partout.
*   **Affinage de la visibilité des membres :** Il a été noté que le retour des membres d'équipe n'est pas encore parfait :
    - Certains utilisateurs attendus ne sont pas visibles (investigation sur \`TeamType.cs\` nécessaire).
    - L'URL de l'avatar (\`avatarUrl\`) est actuellement manquante dans le retour GraphQL utilisé par le Dashboard.
*   **Normalisation des données :** Continuer à veiller à la normalisation des types (Enums String vs Int) et des formats d'ID (GUID avec/sans tirets) entre le backend HotChocolate et le frontend Apollo pour éviter les régressions sur la logique de cycle d'humeur ou les droits d'édition.

---
La migration est fonctionnelle de bout en bout sur la fonctionnalité critique du Dashboard. Les autres pages peuvent être migrées progressivement.
