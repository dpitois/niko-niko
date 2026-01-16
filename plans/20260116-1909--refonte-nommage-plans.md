# Implementation Plan - Refonte de la Gestion et du Nommage des Plans

## 1. 🔍 Analysis & Context
*   **Objective:** Standardiser le nommage des plans avec un préfixe dateur `yyyyMMdd`, migrer l'existant et activer le versionnage Git du dossier `plans/`.
*   **Affected Files:** `.gemini/commands/plan.toml`, `.gitignore`, tous les fichiers dans `plans/`.
*   **Key Dependencies:** Commande shell `date` pour l'automatisation, `git` pour le suivi de version.
*   **Risks/Unknowns:** Conflits potentiels lors du renommage si des fichiers ont des noms similaires, nécessité de mettre à jour le template interne de la commande `plan.toml` elle-même pour refléter le changement.

## 2. 📋 Checklist
- [ ] Step 1: Mise à jour de la commande `plan`
- [ ] Step 2: Migration des fichiers existants
- [ ] Step 3: Activation du versionnage Git
- [ ] Verification: Test de création d'un nouveau plan

## 3. 📝 Step-by-Step Implementation Details

### Step 1: Mise à jour de la commande `plan`
*   **Goal:** Automatiser l'inclusion de la date dans le nom du fichier généré par Gemini.
*   **Action:**
    *   Modifier `.gemini/commands/plan.toml`.
    *   Mettre à jour l'instruction de sortie finale dans le `prompt` : remplacer `plans/[feature-name].md` par `plans/[yyyyMMdd]--[feature-name].md`.
    *   Mettre à jour la mention `Write the plan to plans/[feature_name].md` en bas du fichier.
*   **Verification:** Vérifier visuellement le fichier `.toml` après modification.

### Step 2: Migration des fichiers existants
*   **Goal:** Appliquer la nouvelle convention aux fichiers déjà présents dans `plans/` en utilisant leur date de création réelle.
*   **Action:**
    *   Exécuter une boucle shell pour renommer les fichiers en extrayant leur date de naissance (`Birth`) via `stat`.
    *   Commande suggérée : `for f in plans/*.md; do date_prefix=$(stat -c %w "$f" | cut -d' ' -f1 | sed 's/-//g'); if [[ $f != plans/${date_prefix}--* ]]; then mv "$f" "plans/${date_prefix}--$(basename "$f")"; fi; done`
*   **Verification:** `ls plans/` pour confirmer que chaque fichier a un préfixe correspondant à son historique.

### Step 3: Activation du versionnage Git
*   **Goal:** Permettre à Gemini de retrouver les traces des plans précédents de manière persistante.
*   **Action:**
    *   Modifier `.gitignore` : supprimer la ligne `plans/`.
    *   Ajouter les fichiers au suivi : `git add plans/`.
*   **Verification:** `git status` pour confirmer que le dossier n'est plus ignoré.

## 4. 🧪 Testing Strategy
*   **Manual Verification:** 
    1. Lancer une commande de planification pour une feature fictive.
    2. Vérifier que le fichier créé porte bien le préfixe `20260116--`.
    3. Vérifier que `git status` affiche le nouveau fichier comme prêt à être indexé.

## 5. ✅ Success Criteria
*   Le fichier `.gemini/commands/plan.toml` instruit désormais la création de fichiers avec préfixe temporel.
*   Tous les anciens plans sont renommés selon le format `yyyyMMdd--[name].md`.
*   Le dossier `plans/` est suivi par Git (retiré du `.gitignore`).
