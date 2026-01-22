# Implementation Plan - Organisation des Imports & Alias Frontend

## 1. 🔍 Analysis & Context
*   **Objective:** Mettre en place des alias de chemin (`@/*` -> `src/*`) pour simplifier les imports et configurer ESLint pour trier automatiquement ces imports en 3 groupes hiérarchiques (Bibliothèques -> Utilitaires -> Autres/Composants), aligné sur les pratiques du backend.
*   **Affected Files:** `app/frontend/tsconfig.app.json`, `app/frontend/vite.config.ts`, `app/frontend/package.json`, `app/frontend/eslint.config.js`, et fichiers sources `.ts/.tsx`.
*   **Key Dependencies:** `eslint-plugin-simple-import-sort`, `vite-tsconfig-paths`.
*   **Risks/Unknowns:** Risque mineur de conflit de résolution si les chemins ne sont pas parfaitement synchronisés entre TS et Vite.

## 2. 📋 Checklist
- [ ] Step 1: Configuration des Paths dans `tsconfig.app.json`.
- [ ] Step 2: Installation et configuration de `vite-tsconfig-paths`.
- [ ] Step 3: Installation de `eslint-plugin-simple-import-sort`.
- [ ] Step 4: Configuration avancée des règles de tri dans `eslint.config.js`.
- [ ] Step 5: Application du tri (`lint --fix`).
- [ ] Verification: Build et Lint check.

## 3. 📝 Step-by-Step Implementation Details

### Step 1: Configuration des Paths TypeScript
*   **Goal:** Définir l'alias `@` pointant vers `src`.
*   **Action:**
    *   Modifier `app/frontend/tsconfig.app.json`.
    *   Ajouter/Mettre à jour `compilerOptions` :
        ```json
        {
          "compilerOptions": {
            "baseUrl": ".",
            "paths": {
              "@/*": ["./src/*"]
            },
            // ... existants
          }
        }
        ```
*   **Verification:** Vérifier qu'aucune erreur JSON n'est présente.

### Step 2: Configuration Vite (vite-tsconfig-paths)
*   **Goal:** Assurer que Vite résout les imports `@/...` comme TypeScript.
*   **Action:**
    *   Dans `app/frontend`, installer le plugin :
        ```bash
        npm install --save-dev vite-tsconfig-paths
        ```
    *   Modifier `app/frontend/vite.config.ts` :
        ```typescript
        import { defineConfig } from 'vite';
        import react from '@vitejs/plugin-react';
        import tsconfigPaths from 'vite-tsconfig-paths'; // Ajout

        export default defineConfig({
          plugins: [react(), tsconfigPaths()], // Ajout
        });
        ```

### Step 3: Installation ESLint Plugin
*   **Goal:** Installer le plugin de tri.
*   **Action:**
    *   Dans `app/frontend`, exécuter :
        ```bash
        npm install --save-dev eslint-plugin-simple-import-sort
        ```

### Step 4: Configuration ESLint (`eslint.config.js`)
*   **Goal:** Configurer le tri strict : Libs > Utils > Others.
*   **Action:**
    *   Modifier `app/frontend/eslint.config.js`.
    *   Imports : `import simpleImportSort from 'eslint-plugin-simple-import-sort';`
    *   Plugins : `plugins: { 'simple-import-sort': simpleImportSort, ... }`
    *   Rules :
        ```javascript
        rules: {
          // ... existantes
          'simple-import-sort/imports': [
            'error',
            {
              groups: [
                // 1. Packages 'react', etc.
                ['^react', '^@?\w'],
                // 2. Utilitaires internes (Alias @ ou relatifs)
                // Liste: services, hooks, context, models, utils, theme
                ['^(@|\.)+/(services|hooks|context|models|utils|theme)(/.*|$)'],
                // 3. Autres (Composants, Pages, etc.) - Alias ou relatifs
                ['^@/', '^\.'],
                // 4. Styles
                ['^\u0000']
              ]
            }
          ],
          'simple-import-sort/exports': 'error'
        }
        ```

### Step 5: Application du tri
*   **Goal:** Réorganiser le code existant.
*   **Action:**
    *   Exécuter `npm run lint -- --fix`.
*   **Verification:** Ouvrir un fichier (ex: `App.tsx`) et vérifier l'ordre des imports.

## 4. 🧪 Testing Strategy
*   **Unit Tests:** N/A.
*   **Integration Tests:** `npm run build` pour confirmer que le build de production fonctionne avec les alias.
*   **Manual Verification:** Lancer l'app en local et naviguer pour s'assurer qu'aucun module n'est manquant (runtime error).

## 5. ✅ Success Criteria
*   Build succès.
*   Lint succès.
*   Imports organisés selon la hiérarchie définie.
