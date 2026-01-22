# Implementation Plan - User Mood Trend Curve

## 1. 🔍 Analysis & Context
*   **Objective:** Ajouter une seconde courbe au widget `TeamMoodTrendWidget` pour afficher la tendance d'humeur personnelle de l'utilisateur connecté, superposée à la moyenne de l'équipe.
*   **Affected Files:** 
    *   `app/frontend/src/components/dashboard/TeamMoodTrendWidget.tsx`
    *   `app/frontend/src/i18n/locales/fr.json`
    *   `app/frontend/src/i18n/locales/en.json`
*   **Key Dependencies:** `useAuth` (pour l'ID utilisateur), `useMoods` (déjà utilisé), Material UI Theme.
*   **Risks/Unknowns:** Superposition visuelle des points (dots) si les valeurs sont identiques. Nécessité de différencier clairement les deux courbes par la couleur.

## 2. 📋 Checklist
- [ ] Step 1: Récupérer l'ID utilisateur via le hook `useAuth`.
- [ ] Step 2: Calculer les données de tendance spécifiques à l'utilisateur dans le `useMemo`.
- [ ] Step 3: Générer les nouveaux chemins SVG (line et area) pour l'utilisateur.
- [ ] Step 4: Afficher la courbe utilisateur et ses points dans le rendu SVG/HTML.
- [ ] Step 5: Mettre à jour les traductions et les tooltips.
- [ ] Verification

## 3. 📝 Step-by-Step Implementation Details

### Step 1: Accès à l'utilisateur connecté
*   **Goal:** Obtenir l'ID de l'utilisateur pour filtrer ses humeurs.
*   **Action:**
    *   Importer `useAuth` dans `TeamMoodTrendWidget.tsx`.
    *   Initialiser `const { user } = useAuth();`.
*   **Verification:** Vérifier via un `console.log` que `user.sub` est accessible.

### Step 2: Calcul des données utilisateur
*   **Goal:** Préparer une série de données `userAverage` dans `chartData`.
*   **Action:**
    *   Modifier l'interface `ChartDataPoint` pour ajouter `userAverage: number | null`.
    *   Mettre à jour la logique de `useMemo` pour accumuler séparément les scores de l'utilisateur (`userScore` et `userCount`).
*   **Verification:** S'assurer que `userAverage` contient bien la valeur de l'humeur de l'utilisateur pour chaque jour saisi.

### Step 3: Génération des chemins SVG
*   **Goal:** Créer les constantes pour le tracé de la nouvelle courbe.
*   **Action:**
    *   Calculer `userPoints` et `validUserPoints` en utilisant `d.userAverage`.
    *   Générer `userLinePath` et `userAreaPath` via `getCurvePath`.
*   **Verification:** Les variables doivent être correctement calculées en fonction des nouvelles données.

### Step 4: Rendu visuel
*   **Goal:** Afficher la courbe et les points avec une couleur distincte (`secondary.main`).
*   **Action:**
    *   Ajouter un `<linearGradient id="gradientUserMood">` dans les `<defs>`.
    *   Ajouter le `<path>` de l'aire et de la ligne utilisateur après ceux de l'équipe.
    *   Ajouter une boucle `validUserPoints.map` pour afficher les points utilisateur (ex: cercles plus petits ou avec une bordure différente).
*   **Verification:** Vérifier que la courbe "équipe" est en `primary` et la courbe "utilisateur" en `secondary`.

### Step 5: Traductions et Tooltips
*   **Goal:** Clarifier quelle donnée appartient à qui.
*   **Action:**
    *   Ajouter `"yourMood": "Votre humeur"` dans `fr.json` et `"yourMood": "Your mood"` dans `en.json`.
    *   Mettre à jour le `Popper` pour afficher soit les deux valeurs, soit identifier la source du point survolé.
*   **Verification:** Vérifier le texte des tooltips au survol des points.

## 4. 🧪 Testing Strategy
*   **Unit Tests:** N/A (Composant UI visuel).
*   **Manual Verification:**
    1.  Se connecter avec un utilisateur ayant saisi des humeurs sur le sprint.
    2.  Vérifier que la courbe utilisateur suit ses propres notes (1=Sad, 2=Neutral, 3=Happy).
    3.  Vérifier que la courbe d'équipe (moyenne) est toujours correcte.
    4.  Changer de langue et vérifier les libellés.

## 5. ✅ Success Criteria
*   Le widget affiche deux courbes distinctes.
*   La courbe utilisateur est de couleur secondaire.
*   Le tooltip affiche clairement "Votre humeur" ou "Humeur moyenne".
*   Si aucune donnée n'est saisie par l'utilisateur, seule la courbe d'équipe (ou le message "No data") s'affiche.
