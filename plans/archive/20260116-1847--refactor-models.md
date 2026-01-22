# Implementation Plan - Refactorisation des Modèles Frontend

## 1. 🔍 Analysis & Context
*   **Objective:** Centraliser toutes les interfaces et types (hors Props et ContextType) dans le dossier `src/models` pour améliorer la maintenabilité et la réutilisabilité.
*   **Affected Files:**
    - `src/components/sprints/MoodGridDisplay.tsx`
    - `src/utils/moodTrendUtils.ts`
    - `src/models/User.ts` (pour unification)
    - `src/models/ChartData.ts` (nouveau)
*   **Key Dependencies:** TypeScript, React.
*   **Risks/Unknowns:** Risques mineurs de conflits de nommage ou de dépendances circulaires lors du regroupement.

## 2. 📋 Checklist
- [ ] Étape 1 : Création/Mise à jour des modèles globaux
- [ ] Étape 2 : Refactorisation de `MoodGridDisplay.tsx`
- [ ] Étape 3 : Refactorisation de `moodTrendUtils.ts`
- [ ] Étape 4 : Mise à jour des imports dépendants
- [ ] Verification : Lint et Build

## 3. 📝 Step-by-Step Implementation Details

### Step 1: Création et unification des modèles
*   **Goal:** Préparer les fichiers dans `src/models` pour accueillir les types déplacés.
*   **Action:**
    - Vérifier `src/models/User.ts`. S'assurer qu'il couvre les besoins de `TeamMember`.
    - Créer `src/models/ChartData.ts` pour accueillir `ChartDataPoint`.
*   **Verification:** Vérifier que les fichiers sont bien exportés.

### Step 2: Refactorisation de `MoodGridDisplay.tsx`
*   **Goal:** Supprimer l'interface `TeamMember` locale.
*   **Action:**
    - Modifier `src/components/sprints/MoodGridDisplay.tsx`.
    - Supprimer `interface TeamMember`.
    - Importer `User` depuis `@/models/User`.
    - Remplacer les usages de `TeamMember` par `User`.
*   **Verification:** Vérifier que le composant compile sans erreur de type.

### Step 3: Refactorisation de `moodTrendUtils.ts`
*   **Goal:** Déplacer `ChartDataPoint` vers les modèles.
*   **Action:**
    - Déplacer la définition de `ChartDataPoint` de `src/utils/moodTrendUtils.ts` vers `src/models/ChartData.ts`.
    - Importer `ChartDataPoint` dans `src/utils/moodTrendUtils.ts`.
*   **Verification:** Vérifier que les fonctions utilitaires compilent.

### Step 4: Mise à jour des composants dépendants
*   **Goal:** S'assurer que les composants utilisant ces utilitaires pointent vers les nouveaux modèles.
*   **Action:**
    - Vérifier `src/components/dashboard/MoodTrendChart.tsx` et d'autres fichiers utilisant `ChartDataPoint`.
    - Mettre à jour les imports si nécessaire.
*   **Verification:** Recherche globale de `ChartDataPoint` pour vérifier les imports.

## 4. 🧪 Testing Strategy
*   **Unit Tests:** Si des tests existent pour `moodTrendUtils`, les relancer.
*   **Manual Verification:**
    1. Lancer `npx tsc --noEmit` pour le check de types.
    2. Lancer `npm run lint` pour le style.
    3. Lancer `npm run build` pour la validation finale.

## 5. ✅ Success Criteria
*   Aucune interface/type (hors Props/ContextType) n'est déclaré en dehors de `src/models`.
*   Le build de production réussit sans erreur.
*   Le lint ne signale aucune erreur d'import ou de type.
