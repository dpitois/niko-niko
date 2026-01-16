# Stratégie de Refonte : Commande de Planification Interactive

## 1. 🔍 Analyse & Contexte
*   **Objectif :** Transformer le flux d'exécution des plans en un processus itératif, interactif et auto-documenté.
*   **Fichiers affectés :** `.gemini/commands/plan.toml`.
*   **Dépendances clés :** Moteur de prompt Gemini, outils `replace` et `write_file`.
*   **Risques/Incertitudes :** Persistance de l'état entre les tours de conversation (l'agent doit se souvenir de mettre à jour le fichier), gestion des refus de l'utilisateur.

## 2. 📋 Checklist de Stratégie
- [ ] Étude de la syntaxe de prompt pour l'itération pas à pas.
- [ ] Définition du vocabulaire de statut des tâches.
- [ ] Conception du mécanisme de mise à jour automatique du fichier `.md`.
- [ ] Intégration des points d'arrêt (User Confirmation).
- [ ] Vérification de la compatibilité avec la convention de nommage `yyyyMMdd--`.

## 3. 📝 Détails Stratégiques de Mise en Œuvre

### Étape 1 : Révision de l'instruction système dans `plan.toml`
*   **Objectif :** Redéfinir le comportement de l'agent lors de l'exécution.
*   **Action :** 
    *   Modifier le template pour inclure une section d'instructions "Runtime" explicite.
    *   Ajouter une directive forçant l'usage de `replace` sur le fichier de plan après chaque étape complétée.
*   **Vérification :** Le prompt doit mentionner explicitement l'obligation de s'arrêter.

### Étape 2 : Standardisation des statuts de progression
*   **Objectif :** Créer un système visuel clair pour le suivi.
*   **Action :** 
    *   `[x]` : Terminé (Done).
    *   `[>]` : En cours (In Progress).
    *   `[-]` : Non applicable / Annulé (Removed/N.A.).
    *   `[?]` : En attente de décision (Pending).
*   **Vérification :** Inclusion de cette légende dans le template de plan généré.

### Étape 3 : Mécanisme de boucle de confirmation
*   **Objectif :** Garantir que l'agent ne "fonce" pas sans accord.
*   **Action :** 
    *   L'instruction doit stipuler : "Après chaque `Step N`, vous devez poser la question : 'L'étape N est-elle validée ? Souhaitez-vous passer à l'étape N+1 ?'".
*   **Vérification :** Test de simulation de pause.

## 4. 🧪 Stratégie de Vérification
*   **Test Unitaire :** Vérifier que le fichier `.md` est bien modifié par l'agent après une validation simulée.
*   **Test d'Intégration :** Lancer un plan fictif complexe (3 étapes) et vérifier que l'agent s'arrête bien 3 fois.

## 5. ✅ Critères de Succès
*   L'agent met à jour les checkboxes du plan en temps réel.
*   L'agent ne procède jamais à l'étape suivante sans un "oui" ou une validation explicite.
*   Le plan final reflète l'état réel de l'implémentation (avec les étapes sautées ou annulées).
