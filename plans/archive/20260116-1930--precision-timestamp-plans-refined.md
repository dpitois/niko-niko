# Stratégie de Mise à Jour : Horodatage Précis (Format Tiret)

## 1. 🔍 Analyse & Contexte
*   **Objectif :** Faire évoluer la convention de nommage des fichiers de plan vers `yyyyMMdd-HHmm--[feature-name].md`. L'utilisation du tiret permet de séparer distinctement le bloc date du bloc heure.
*   **Fichiers affectés :** `.gemini/commands/plan.toml` et `.gitignore`.
*   **Dépendances clés :** Système de fichiers Git, configuration des commandes Gemini.
*   **Risques/Incertitudes :** Nécessité d'ajuster précisément le `.gitignore` pour autoriser ce nouveau motif de caractères.

## 2. 📋 Checklist
- [x] Step 1: Modification de plan.toml
- [x] Step 2: Mise à jour du .gitignore
- [x] Step 3: Validation du processus interactif (incluant le renommage système)

## 3. 📝 Détails Stratégiques de Mise en Œuvre

### Étape 1 : Modification de `plan.toml`
*   **Goal :** Imposer le nouveau format de nommage.
*   **Status :** Done
*   **Action :** 
    *   Remplacer les occurrences de `[yyyyMMdd]--` par `[yyyyMMdd]-[HHmm]--`.
    *   Préciser dans les instructions que l'heure et les minutes doivent impérativement être sur deux chiffres (padding).
*   **Vérification :** Le prompt de la commande `plan` doit explicitement demander ce format.

### Étape 2 : Mise à jour du `.gitignore`
*   **Goal :** Autoriser le nouveau format de fichier dans Git.
*   **Status :** Done
*   **Action :** 
    *   Remplacer `!plans/[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]--*.md` par `!plans/[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]-[0-9][0-9][0-9][0-9]--*.md`.
*   **Vérification :** `git add` sur un nouveau fichier de test pour confirmer qu'il n'est pas ignoré.

### Étape 3 : Validation du processus interactif
*   **Goal :** S'assurer que les nouvelles règles d'exécution (pas à pas) fonctionnent avec ce nommage.
*   **Status :** Done
*   **Action :** 
    *   Générer un plan de test.
    *   **Note :** Incorpore le renommage massif des anciens plans avec les dates système réelles.
*   **Vérification :** Confirmation visuelle du nom de fichier généré.

## 4. 🧪 Stratégie de Vérification
*   **Vérification Git :** Utiliser `git status` pour confirmer que les nouveaux fichiers sont vus comme "untracked" et non ignorés.
*   **Vérification Tri :** Créer deux fichiers à quelques minutes d'intervalle et vérifier leur ordre dans `ls -1 plans/`.

## 5. ✅ Critères de Succès
*   Le fichier créé suit le format `20260116-1930--nom.md`.
*   Le tiret est présent entre le 8ème et le 9ème chiffre.
*   Les deux tirets `--` sont présents avant le nom de la fonctionnalité.
*   Git autorise le commit de ces fichiers sans l'option `-f`.
