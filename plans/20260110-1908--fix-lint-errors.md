# Implementation Plan - Correction Lint Frontend

## 1. 🔍 Analysis & Context
*   **Objective:** Éliminer toutes les erreurs et avertissements ESLint (variables inutilisées et mauvaises pratiques React Hooks) pour assainir le projet.
*   **Affected Files:**
    *   `src/pages/LoginPage.tsx` (React Hook)
    *   `src/pages/CreateSprintPage.tsx` (React Hook)
    *   `src/components/AdminCreateSprintForm.tsx` (Unused var)
    *   `src/components/CreateSprintForm.tsx` (Unused var)
    *   `src/components/CreateTeamForm.tsx` (Unused var)
    *   `src/components/MoodEntryForm.tsx` (Unused var)
    *   `src/components/sprints/SprintMoodGrid.tsx` (Unused var)
    *   `src/pages/AdminSprintsPage.tsx` (Unused var)
    *   `src/pages/AdminTeamsPage.tsx` (Unused var)
    *   `src/pages/AdminUsersPage.tsx` (Unused var)
*   **Key Dependencies:** `react`, `react-router-dom`, ESLint.
*   **Risks/Unknowns:** Attention aux boucles de redirection dans `CreateSprintPage`.

## 2. 📋 Checklist
- [ ] Step 1: Refactor `LoginPage.tsx` (Lazy State Init).
- [ ] Step 2: Refactor `CreateSprintPage.tsx` (URL as Source of Truth).
- [ ] Step 3: Supprimer les variables inutilisées dans les composants et pages.
- [ ] Verification: `npm run lint` & `npm run build`.

## 3. 📝 Step-by-Step Implementation Details

### Step 1: Refactor `LoginPage.tsx`
*   **Goal:** Supprimer le `useEffect` qui appelle `setState` de manière synchrone.
*   **Action:**
    *   Modifier `src/pages/LoginPage.tsx`:
        *   Remplacer l'initialisation de `githubLoginHref` et `googleLoginHref` par une fonction d'initialisation.
        *   Supprimer le `useEffect`.
        ```typescript
        // Remplacer:
        // const [githubLoginHref, setGithubLoginHref] = useState('/api/auth/login-github');
        // useEffect ...

        // Par:
        const [githubLoginHref] = useState(() => {
            const token = localStorage.getItem('invitationToken');
            return token ? `/api/auth/login-github?invitationToken=${token}` : '/api/auth/login-github';
        });
        const [googleLoginHref] = useState(() => {
            const token = localStorage.getItem('invitationToken');
            return token ? `/api/auth/login-google?invitationToken=${token}` : '/api/auth/login-google';
        });
        ```

### Step 2: Refactor `CreateSprintPage.tsx`
*   **Goal:** Simplifier la gestion de l'état de l'équipe sélectionnée et corriger l'erreur de hook.
*   **Action:**
    *   Modifier `src/pages/CreateSprintPage.tsx`:
        *   Supprimer `useState` (`selectedTeam`, `setSelectedTeam`) et son `useEffect` associé.
        *   Calculer `selectedTeam` directement : `const selectedTeam = urlTeamId || '';`
        *   Ajouter un `useEffect` pour rediriger vers la première équipe si aucune n'est sélectionnée dans l'URL :
        ```typescript
        useEffect(() => {
            if (!urlTeamId && teams && teams.length > 0) {
                navigate(`/sprint/create/${teams[0].id}`, { replace: true });
            }
        }, [urlTeamId, teams, navigate]);
        ```
        *   Mettre à jour `handleTeamSelectChange` pour utiliser uniquement `navigate`.
        *   Mettre à jour le JSX pour utiliser la variable calculée `selectedTeam`.

### Step 3: Nettoyage des variables inutilisées
*   **Goal:** Supprimer les variables déclarées mais non utilisées.
*   **Action:**
    *   Modifier les fichiers suivants en remplaçant `catch (err) {` (ou similaire) par `catch {` :
        *   `src/components/AdminCreateSprintForm.tsx`
        *   `src/components/CreateSprintForm.tsx`
        *   `src/components/CreateTeamForm.tsx`
        *   `src/components/MoodEntryForm.tsx` (variable `_err`)
        *   `src/pages/AdminSprintsPage.tsx`
        *   `src/pages/AdminTeamsPage.tsx`
        *   `src/pages/AdminUsersPage.tsx`
    *   Dans `src/components/sprints/SprintMoodGrid.tsx` : Supprimer la variable inutilisée `_error` (vérifier contexte).

## 4. 🧪 Testing Strategy
*   **Linting:** Exécuter `npm run lint` (doit retourner 0).
*   **Build:** Exécuter `npm run build` (doit retourner 0).
*   **Manual:** Vérifier que la page Login a les bons liens (avec token si présent) et que la page Create Sprint sélectionne bien une équipe par défaut.

## 5. ✅ Success Criteria
*   Aucune erreur ou warning affiché par `npm run lint`.
