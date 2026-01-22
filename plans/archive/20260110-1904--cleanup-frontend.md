# Implementation Plan - Nettoyage Frontend

## 1. 🔍 Analysis & Context
*   **Objective:** Supprimer les composants et pages React inutilisés pour assainir la base de code.
*   **Affected Files:**
    *   `app/frontend/src/components/Header.tsx`
    *   `app/frontend/src/components/TeamView.tsx`
    *   `app/frontend/src/pages/AdminDashboardPage.tsx`
*   **Key Dependencies:** Aucune (ce sont des feuilles mortes du graphe de dépendance).
*   **Risks/Unknowns:** Faible risque que ces fichiers contiennent du code "en attente" d'intégration. Git permettra de les retrouver si nécessaire.

## 2. 📋 Checklist
- [ ] Step 1: Supprimer les fichiers identifiés.
- [ ] Step 2: Vérifier l'intégrité de l'application (Build & Lint).

## 3. 📝 Step-by-Step Implementation Details

### Step 1: Suppression des fichiers morts
*   **Goal:** Éliminer physiquement les fichiers du projet.
*   **Action:**
    *   Supprimer `app/frontend/src/components/Header.tsx`
    *   Supprimer `app/frontend/src/components/TeamView.tsx`
    *   Supprimer `app/frontend/src/pages/AdminDashboardPage.tsx`
*   **Verification:** Vérifier que les fichiers n'existent plus.

### Step 2: Validation technique
*   **Goal:** S'assurer que la suppression n'a brisé aucune dépendance cachée.
*   **Action:**
    *   Exécuter la commande de build dans le dossier `app/frontend` : `npm run build`
    *   Exécuter le linter : `npm run lint`
*   **Verification:** Les deux commandes doivent se terminer avec un code de sortie 0 (succès).

## 4. 🧪 Testing Strategy
*   **Unit Tests:** Non applicable (suppression de code).
*   **Integration Tests:** Le build (`tsc`) fait office de test d'intégration statique.
*   **Manual Verification:** Lancer l'application (`npm run dev`) et vérifier que les pages principales (Dashboard, Admin) s'affichent correctement sans erreur console.

## 5. ✅ Success Criteria
*   Les fichiers cibles sont supprimés.
*   `npm run build` passe avec succès.
