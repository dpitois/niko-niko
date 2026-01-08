### Plan d'Action Détaillé

Ce plan est structuré en deux phases principales pour aborder chaque problème de manière isolée.

#### Phase 1 : Correction du mécanisme d'ouverture/fermeture de la barre latérale

L'objectif est de rendre le bouton dans l'en-tête de la barre latérale capable d'ouvrir et de fermer le panneau.

1.  **Fichier à modifier :** `app/frontend/src/components/layout/Sidebar.tsx`
    *   **Logique à implémenter :**
        *   L' `IconButton` présent dans le `DrawerHeader` doit devenir dynamique.
        *   **Si la barre est ouverte (`open` est `true`)**, le clic sur le bouton doit appeler `handleDrawerClose()`. L'icône affichée sera `<ChevronLeftIcon />`.
        *   **Si la barre est fermée (`open` est `false`)**, le clic sur le même bouton doit appeler `handleDrawerOpen()`. L'icône affichée sera `<ChevronRightIcon />`.
    *   **Investigation supplémentaire :** Il faudra vérifier si `handleDrawerOpen` est bien passé en prop. D'après l'analyse précédente, `AppLayout.tsx` le passe, mais il n'est pas récupéré dans les props du composant `Sidebar`. Il faudra l'ajouter.

2.  **Fichier à modifier :** `app/frontend/src/components/layout/AppLayout.tsx`
    *   **Logique à implémenter :**
        *   Au lieu de jouer sur la marge (`marginLeft`) du contenu principal (`Main`), ce qui décale toute la page, il est plus robuste de faire varier la largeur du composant `Drawer` lui-même.
        *   **Quand la barre est ouverte (`open` est `true`)**, sa largeur sera `drawerWidth` (ex: 240px).
        *   **Quand la barre est fermée (`open` est `false`)**, sa largeur sera réduite pour n'afficher que les icônes (ex: `theme.spacing(7)`).
        *   Le contenu principal (`Main`) devra adapter sa marge à gauche en fonction de ces nouvelles largeurs. Cela rend l'animation plus fluide et le code plus cohérent avec les standards de Material UI (MUI).

#### Phase 2 : Masquer la barre latérale pour les utilisateurs non authentifiés

L'objectif est d'empêcher l'affichage de la barre latérale sur les pages qui ne nécessitent pas d'être authentifié (comme la page de connexion).

1.  **Fichier à modifier :** `app/frontend/src/App.tsx` (ou le composant qui gère les routes principales)
    *   **Logique à implémenter :**
        *   Actuellement, `AppLayout` est probablement utilisé comme un wrapper global pour toutes les pages.
        *   Il faut identifier la page de `Login` et lui appliquer une mise en page différente qui n'inclut pas `AppLayout` (et donc pas la `Sidebar`). Les autres pages, protégées, continueront d'utiliser `AppLayout`.

2.  **Fichier à modifier :** `app/frontend/src/components/layout/AppLayout.tsx`
    *   **Logique de sécurité à implémenter (en alternative ou complément) :**
        *   Importer le hook `useAuth` depuis le `AuthContext`.
        *   Récupérer l'état de l'utilisateur (`const { user } = useAuth();`).
        *   Conditionner l'affichage du composant `<Sidebar />` à la présence de l'objet `user`.
            *   Exemple : `{user && <Sidebar ... />}`
        *   Ajuster le style du composant `Main` pour qu'il occupe 100% de la largeur si la `Sidebar` n'est pas affichée.

### Todo List Hiérarchique

Voici le résumé sous forme de liste de tâches :

*   **Tâche 1 : Rendre la barre latérale fonctionnelle**
    *   [ ] **1.1. Modifier `Sidebar.tsx`**
        *   [ ] Récupérer la prop `handleDrawerOpen`.
        *   [ ] Rendre l'`onClick` de l'`IconButton` dynamique pour appeler `handleDrawerOpen` ou `handleDrawerClose` en fonction de l'état `open`.
        *   [ ] Afficher l'icône `ChevronRightIcon` ou `ChevronLeftIcon` en fonction de ce même état.
    *   [ ] **1.2. Modifier `AppLayout.tsx` et le style associé**
        *   [ ] Définir une largeur pour la barre latérale fermée (`width` au lieu de `marginLeft`).
        *   [ ] Mettre à jour les styles (CSS-in-JS) pour que le `Drawer` et le `Main` s'animent en fonction de la nouvelle largeur, et non plus de la marge.

*   **Tâche 2 : Gérer la visibilité de la barre latérale**
    *   [ ] **2.1. Modifier `App.tsx` (ou le routeur)**
        *   [ ] Appliquer `AppLayout` uniquement aux routes protégées nécessitant la barre latérale.
        *   [ ] Assurer que la route `/login` utilise une mise en page simple, sans `AppLayout`.
    *   [ ] **2.2. Renforcer la logique dans `AppLayout.tsx`**
        *   [ ] Utiliser `useAuth()` pour ne rendre le composant `<Sidebar />` que si un utilisateur est connecté.

*   **Tâche 3 : Validation**
    *   [ ] **3.1. Tests fonctionnels**
        *   [ ] Vérifier que le bouton de la barre latérale l'ouvre et la ferme correctement.
        *   [ ] Confirmer que les icônes et le contenu s'ajustent bien en mode réduit et complet.
    *   [ ] **3.2. Tests de visibilité**
        *   [ ] S'assurer que la barre latérale est totalement absente de la page `/login`.
        *   [ ] Vérifier qu'elle apparaît correctement après une connexion réussie.