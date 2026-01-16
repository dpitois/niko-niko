# Plan de Mise à jour des Dépendances (Nuget & NPM)

## 1. 🔍 Analyse & Contexte
*   **Objectif :** Mettre à jour les bibliothèques backend (.NET) et frontend (React/NPM) vers leurs dernières versions stables tout en garantissant l'intégrité de l'application.
*   **Fichiers Affectés :**
    *   `api/**/*.csproj` (Backend)
    *   `app/frontend/package.json` & `app/frontend/package-lock.json` (Frontend)
*   **Dépendances Clés :**
    *   Backend : .NET 10 (Entity Framework Core, Authentication, Swashbuckle).
    *   Frontend : React 19, Material UI v7, Vite 7.
*   **Risques/Inconnus :**
    *   **Backend :** Le projet utilise déjà .NET 10. Les mises à jour seront probablement des correctifs (patches) ou des versions mineures très récentes. Risque de conflit de version entre `Npgsql` et `Microsoft.EntityFrameworkCore`.
    *   **Frontend :** React 19 et MUI v7 sont des versions majeures récentes. Attention aux changements de rupture (breaking changes) dans les composants MUI ou les hooks React si des montées de version mineures importantes ont eu lieu.

## 2. 📋 Checklist
- [ ] Etape 1 : Audit des versions obsolètes (Backend & Frontend).
- [ ] Etape 2 : Mise à jour des packages Nuget (Backend).
- [ ] Etape 3 : Vérification intermédiaire Backend (Build & Tests).
- [ ] Etape 4 : Mise à jour des packages NPM (Frontend).
- [ ] Etape 5 : Vérification intermédiaire Frontend (Lint, Build & Démarrage).
- [ ] Etape 6 : Validation finale (Tests d'intégration & Lancement complet).

## 3. 📝 Détails de l'implémentation étape par étape

### Etape 1 : Audit des versions
*   **But :** Identifier précisément quels paquets nécessitent une mise à jour avant de modifier quoi que ce soit.
*   **Action :**
    *   Dans le dossier `api/`, exécuter : `dotnet list package --outdated` pour voir les paquets .NET à mettre à jour.
    *   Dans le dossier `app/frontend/`, exécuter : `npm outdated` pour lister les dépendances JS obsolètes.
*   **Vérification :** Capturer la sortie des commandes pour planifier les mises à jour spécifiques.

### Etape 2 : Mise à jour Backend (Nuget)
*   **But :** Mettre à jour les dépendances .NET vers la dernière version compatible (Patch/Minor d'abord).
*   **Action :**
    *   Pour chaque projet (`NikoNiko.Api`, `NikoNiko.Core`, `NikoNiko.Data`, etc.), mettre à jour les paquets listés.
    *   Commande générique : `dotnet add [Projet.csproj] package [NomDuPaquet] -v [NouvelleVersion]`
    *   *Priorité :* Mettre à jour les paquets `Microsoft.*` et `System.*` en groupe pour éviter les incompatibilités de version (ex: EF Core).
*   **Vérification :** `dotnet restore` ne doit afficher aucune erreur.

### Etape 3 : Vérification Backend
*   **But :** S'assurer que les mises à jour Nuget n'ont rien cassé.
*   **Action :**
    *   Exécuter `dotnet build api/NikoNiko.slnx` (ou solution équivalente) pour vérifier la compilation.
    *   Exécuter `dotnet test api/` pour lancer tous les tests unitaires et d'intégration.
*   **Vérification :** Build réussi (0 erreurs) et tous les tests passent (Green).

### Etape 4 : Mise à jour Frontend (NPM)
*   **But :** Mettre à jour les dépendances React et outils de build.
*   **Action :**
    *   Dans `app/frontend/` :
    *   Exécuter `npm update` pour les mises à jour mineures/patchs respectant le `package.json` actuel.
    *   Si des versions majeures sont identifiées et nécessaires (via `npm outdated`), utiliser `npm install [package]@latest` avec précaution.
    *   *Note Spéciale :* Vérifier spécifiquement les `peerDependencies` de MUI (@mui/material) et React pour éviter les conflits.
*   **Vérification :** `npm install` se termine sans erreurs d'arborescence de dépendances (peer dependency warnings).

### Etape 5 : Vérification Frontend
*   **But :** Valider le code Frontend après mise à jour.
*   **Action :**
    *   `npm run lint` : Vérifier qu'il n'y a pas de nouvelles erreurs de linter.
    *   `npm run build` : Vérifier que le build de production (Vite + TSC) fonctionne.
    *   `npm run test` (si applicable) : Lancer les tests frontend.
*   **Vérification :** Le dossier `dist/` est généré correctement. Le linter est silencieux.

### Etape 6 : Validation Finale
*   **But :** Tester l'application dans son ensemble.
*   **Action :**
    *   Lancer l'environnement complet : `docker compose up -d --build`.
    *   Vérifier les logs : `docker compose logs -f`.
*   **Vérification :** L'application est accessible sur `http://localhost:5173` (Front) et `http://localhost:5000` (Back). Le login fonctionne.

## 4. 🧪 Stratégie de Test
*   **Tests Unitaires/Intégration (.NET) :** Exécution de la suite complète `dotnet test`. Critère : 100% de réussite.
*   **Build Frontend :** Le TypeScript doit compiler sans erreur (`tsc -b`).
*   **Sanity Check Manuel :**
    *   Lancer l'app.
    *   Se connecter (Auth OAuth/Mock).
    *   Naviguer sur le Dashboard.
    *   Ajouter une entrée d'humeur (Mood).

## 5. ✅ Critères de Succès
*   Toutes les dépendances listées par `outdated` ont été traitées (mises à jour ou ignorées volontairement avec justification).
*   Le build (Back & Front) passe.
*   Aucune régression fonctionnelle détectée sur le parcours critique (Login -> Dashboard -> Mood).
