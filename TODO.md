# Plan d'Action : Implémentation de la Page "Admin Sprints"

L'objectif est de créer une page fonctionnelle pour que les super-admins puissent voir tous les sprints, en créer de nouveaux pour n'importe quelle équipe, et les supprimer.

## Phase 1: Mise à jour des Services Frontend

1.  **Fichier à modifier :** `app/frontend/src/services/sprintService.ts`
    *   Modifier `getSprints` pour rendre le paramètre `teamId` optionnel.
    *   Ajouter la fonction `deleteSprint`.

## Phase 2: Création des Hooks SWR

1.  **Fichier à créer :** `app/frontend/src/hooks/useSprints.ts`
    *   Créer un hook pour récupérer la liste de tous les sprints.

2.  **Fichier à créer :** `app/frontend/src/hooks/useTeams.ts`
    *   Créer un hook pour récupérer la liste de toutes les équipes.

## Phase 3: Implémentation de l'UI de la page `AdminSprintsPage`

1.  **Fichier à modifier :** `app/frontend/src/pages/AdminSprintsPage.tsx`
    *   **Structure :** Utiliser `Grid` pour diviser la page en une section "Créer" et une section "Sprints Existants".
    *   **Logique :** Utiliser les hooks `useSprints` et `useTeams`. Passer les équipes et une fonction de callback au `CreateSprintForm`.

2.  **Fichier à modifier :** `app/frontend/src/components/CreateSprintForm.tsx`
    *   **Adaptation :**
        *   Accepter une prop `teams`.
        *   Ajouter un `Select` pour choisir l'équipe.
        *   Accepter une prop `onSprintCreated` à appeler après la création.

## Phase 4: Affichage et Suppression des Sprints

1.  **Fichier à modifier :** `app/frontend/src/pages/AdminSprintsPage.tsx`
    *   **Affichage :** Mapper les sprints pour les afficher, incluant un bouton de suppression.
    *   **Logique de suppression :**
        *   Ajouter un `Dialog` de confirmation.
        *   Au clic, ouvrir le `Dialog`.
        *   Si confirmé, appeler `sprintService.deleteSprint`, notifier, et rafraîchir la liste avec `mutate`.
