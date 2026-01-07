# Consignes

- Tu n'effectues pas de changements de code sans me demander.
- Tu commences par me proposer un plan d'action(s) détaillé à valider avant tout changement de code.

# Objectifs

- implémenter une gestion de rôle et de permissions utilisateur
- les rôles s'appliquent en cascade :
    - **Administrateur d'équipe** a les droits et permissions de **Utilisateur** en plus de ces spécificités
    - **Super administrateur** a les droits et permissions de  **Administrateur d'équipe** en plus de ces spécificités

# Roles & permissions

## Super administrateur

- nom : `super-admin`
- il ne peut y avoir qu'un seul et unique `super-admin` dans toute l'application
    - alternative: une variable d'environnement permet de définir un ou plusieurs utilisateurs comme `super-admin`, il s'agirait alors d'une liste d'email correspondant aux utilisateurs concernés. La valeur est tout de même persistée en base de données, au démarrage, cette variable peut être modifiée, si un utilisateur avait ce role et n'est plus présent, alors il passe `user` à moins qu'il soit admin d'une équipe dans ce cas c'est `team-admin`, un nouvel utilisateur précisé dans cette variable obtient alors le rôle `super-admin`
- ce rôle est automatiquement attribué au premier utilistateur créé
- ce rôle a tous les droits et permissions sur l'application
- **permissions :**
    - voir toutes les équipes
    - voir tous les utilisateurs
    - voir tous les sprints
    - voir toutes les invitations
    - supprimer une équipe
    - supprimer un utilisateur
    - supprimer un sprint
    - annuler/supprimer une invitation

## Administrateur d'équipe

- nom : `team-admin`
- il ne peut y avoir qu'un seul `team-admin` par équipe
- ce rôle est automatiquement attribué à l'utilisateur qui crée une équipe
- ce rôle permet de créer un sprint pour l'équipe dont il est `team-admin`
- **permissions :**
    - voir tous les utilisateurs de l'équipe
    - voir tous les sprints de l'équipe
    - voir toutes les invitations de l'équipe
    - supprimer l'équipe
    - supprimer un utilisateur de l'équipe
    - supprimer un sprint de l'équipe de l'équipe
    - annuler/supprimer une invitation de l'équipe

## Utilisateur

- nom : `user`
- par défaut, tout utilisateur a à minima ce rôle
- ce rôle permet de saisir l'humeur de l'utilisateur
- **permissions :**
    - voir tous les utilisateurs de l'équipe
    - voir tous les sprints de l'équipe
    - saisir l'humeur de l'utilisateur
    - modifier l'humeur de l'utilisateur

# Le plan d'action

# Le plan d'action

### Plan d'action : Intégration des Rôles/Permissions et Modification de la Page de Connexion

**Objectif principal** : Intégrer les rôles et permissions des utilisateurs dans le frontend pour contrôler l'accès aux fonctionnalités et modifier l'affichage de la page de connexion.

**Détail du Plan :**

- [x] **1. Analyse de l'Existant**
    - [x] Identifier le composant ou la logique responsable du rendu de l'en-tête global de l'application.
    - [x] Identifier le composant de la page de connexion (`Login`) et son intégration dans le routage.
    - [x] Examiner `app/frontend/src/context/AuthContext.tsx` pour comprendre comment l'état d'authentification est géré et où les informations de l'utilisateur (incluant potentiellement les rôles) peuvent être stockées.
    - [x] Vérifier les appels API liés à l'authentification pour voir si le backend renvoie déjà des informations sur les rôles de l'utilisateur. Si ce n'est pas le cas, cela pourrait nécessiter une modification backend (mais je me concentrerai sur le frontend pour l'instant, en supposant que l'API peut être étendue ou que les rôles sont déjà disponibles via un autre endpoint).

- [x] **2. Mise à jour du Contexte d'Authentification (`AuthContext`) pour les Rôles**
    - [x] Modifier l'interface `AuthContextType` dans `app/frontend/src/context/AuthContext.tsx` pour inclure des propriétés pour les rôles (ex: `isSuperAdmin: boolean`, `isTeamAdmin: boolean`, `isTeamMember: boolean`).
    - [x] Mettre à jour la fonction de connexion (ex: `login` ou `handleAuthCallback`) dans `AuthContext` pour extraire ces informations de l'objet utilisateur retourné par l'API (ou via une nouvelle requête si nécessaire) et les stocker dans l'état de l'authentification.
    - [x] Assurer la persistance de ces rôles (ex: dans le `localStorage` ou les `cookies`) avec les autres informations d'authentification si nécessaire.

- [x] **3. Implémentation du Contrôle d'Accès basé sur les Rôles**
    - [x] **Créer un Hook d'Autorisation (`usePermissions.ts`)**: Développer un hook personnalisé (ex: `app/frontend/src/hooks/usePermissions.ts`) qui utilise le `AuthContext` pour vérifier si l'utilisateur a les rôles nécessaires pour accéder à une fonctionnalité ou une route. Ce hook pourrait offrir des fonctions comme `canManageTeams()`, `isSuperAdmin()`, `isTeamMemberOf(teamId)`.
    - [x] **Mettre à jour les Composants Protégés**:
        - [x] Dans `app/frontend/src/components/ProtectedRoute.tsx`, étendre la logique pour non seulement vérifier l'authentification, mais aussi les rôles requis pour la route si spécifié.
        - [x] Appliquer ce hook ou des vérifications directes de rôles dans les composants frontend qui gèrent les fonctionnalités sensibles (ex: `CreateTeamForm.tsx`, `CreateTeamInvitationForm.tsx`, panneaux d'administration) pour afficher/masquer des éléments UI ou des redirections.

- [x] **4. Modification de l'Affichage de la Page de Connexion**
    - [x] **Identification du Composant Header**: Localiser le fichier du composant `Header` (probablement `app/frontend/src/components/Header.tsx`).
    - [x] **Logique de Rendu Conditionnel**: Dans le composant principal de l'application (ex: `app/frontend/src/App.tsx`), ajouter une logique pour ne pas rendre le `Header` lorsque l'utilisateur est sur la page de connexion. Ceci peut être réalisé en vérifiant la route actuelle via `react-router-dom` (ex: `useLocation().pathname`).
    - [x] S'assurer que la page de connexion (`app/frontend/src/pages/Login.tsx` ou équivalent) n'inclut pas directement le `Header` mais dépend de son parent pour son rendu.

- [x] **5. Tests**
    - [x] Tester la page de connexion pour s'assurer que l'en-tête est correctement masqué.
    - [x] Tester les différentes routes et fonctionnalités avec des utilisateurs ayant des rôles différents (simulés si le backend n'est pas encore prêt) pour vérifier que le contrôle d'accès fonctionne comme prévu.