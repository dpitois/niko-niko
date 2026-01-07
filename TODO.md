# Consignes

- Tu n'effectues pas de changements de code sans me demander.
- Tu commences par me proposer un plan d'action(s) détaillé à valider avant tout changement de code.

# Objectifs

- implémenter une gestion de rôle et de permissions utilisateur
- les rôles s'appliquent en cascade :
    - **Administrateur d'équipe** a les droits et permissions de **Utilisateur** en plus de ces spécificités
    - **Super administrateur** a les droits et permissions de  **Administrateur d'équipe** en plus de ces spécificités

# Roles & permissions

## Super administrateur

- nom : `super-admin`
- il ne peut y avoir qu'un seul et unique `super-admin` dans toute l'application
    - alternative: une variable d'environnement permet de définir un ou plusieurs utilisateurs comme `super-admin`, il s'agirait alors d'une liste d'email correspondant aux utilisateurs concernés. La valeur est tout de même persistée en base de données, au démarrage, cette variable peut être modifiée, si un utilisateur avait ce role et n'est plus présent, alors il passe `user` à moins qu'il soit admin d'une équipe dans ce cas c'est `team-admin`, un nouvel utilisateur précisé dans cette variable obtient alors le rôle `super-admin`
- ce rôle est automatiquement attribué au premier utilistateur créé
- ce rôle a tous les droits et permissions sur l'application
- **permissions :**
    - voir toutes les équipes
    - voir tous les utilisateurs
    - voir tous les sprints
    - voir toutes les invitations
    - supprimer une équipe
    - supprimer un utilisateur
    - supprimer un sprint
    - annuler/supprimer une invitation

## Administrateur d'équipe

- nom : `team-admin`
- il ne peut y avoir qu'un seul `team-admin` par équipe
- ce rôle est automatiquement attribué à l'utilisateur qui crée une équipe
- ce rôle permet de créer un sprint pour l'équipe dont il est `team-admin`
- **permissions :**
    - voir tous les utilisateurs de l'équipe
    - voir tous les sprints de l'équipe
    - voir toutes les invitations de l'équipe
    - supprimer l'équipe
    - supprimer un utilisateur de l'équipe
    - supprimer un sprint de l'équipe de l'équipe
    - annuler/supprimer une invitation de l'équipe

## Utilisateur

- nom : `user`
- par défaut, tout utilisateur a à minima ce rôle
- ce rôle permet de saisir l'humeur de l'utilisateur
- **permissions :**
    - voir tous les utilisateurs de l'équipe
    - voir tous les sprints de l'équipe
    - saisir l'humeur de l'utilisateur
    - modifier l'humeur de l'utilisateur

# Le plan d'action

---

#### **Étape 1 : Améliorer la couverture des tests d'intégration**

1.  **Création du fichier de test :** Je vais créer un nouveau fichier `AuthorizationTests.cs` dans le projet `NikoNiko.Api.IntegrationTests` pour héberger les tests de permissions.
2.  **Enrichir le contexte de test :** J'améliorerai la classe `NikoNikoApiTestApplication.cs` pour fournir des méthodes utilitaires permettant de créer et d'authentifier facilement des utilisateurs avec des rôles spécifiques (`user`, `team-admin`, `super-admin`).

---

#### **Étape 2 : Écriture des scénarios de test par rôle**

1.  **Tests pour le rôle `user` :**
    *   Vérifier qu'un utilisateur ne peut voir que les équipes dont il est membre.
    *   Vérifier qu'un utilisateur ne peut pas accéder aux endpoints d'administration (création/suppression d'équipes, de sprints, etc.).
    *   Confirmer qu'il reçoit une erreur `403 Forbidden` lorsqu'il tente une action non autorisée.

2.  **Tests pour le rôle `team-admin` :**
    *   Vérifier qu'un `team-admin` peut créer des sprints et des invitations pour l'équipe qu'il administre.
    *   Vérifier qu'il peut supprimer son équipe.
    *   Confirmer qu'il ne peut pas gérer les ressources d'une équipe qu'il n'administre pas.

3.  **Tests pour le rôle `super-admin` :**
    *   Vérifier que le `super-admin` peut lister toutes les équipes, tous les utilisateurs et toutes les ressources de l'application.
    *   Vérifier qu'il peut supprimer n'importe quelle équipe, utilisateur ou autre ressource.
    *   Confirmer que ses accès sont globaux et ne sont pas limités à une seule équipe.

---

#### **Étape 3 : Exécution et validation**

1.  **Lancement des tests :** J'exécuterai la suite de tests complète pour m'assurer que les nouvelles validations sont correctes et qu'aucune régression n'a été introduite.
2.  **Validation :** Je confirmerai que tous les tests passent, garantissant que les règles de gestion des rôles sont bien appliquées par l'API.


## backup 

  Plan d'action complet (mis à jour)

  Je n'effectuerai aucune modification avant votre validation.

  Phase 1 : Analyse et Refactoring de l'Autorisation

   1. Recherche des `[AllowAnonymous]` : Je vais commencer par rechercher toutes les occurrences de l'attribut [AllowAnonymous] dans le code source de l'API pour
      m'assurer qu'elles sont justifiées (ex: callbacks OAuth, endpoints publics de documentation comme Swagger). Je vous soumettrai les résultats pour validation.

   2. Création de Politiques d'Autorisation personnalisées :
       * Je vais créer un Requirement et un Handler pour la politique IsTeamMember. Ce handler vérifiera si l'utilisateur authentifié est membre de l'équipe spécifiée
         dans la route.
       * Je ferai de même pour la politique IsTeamAdmin, qui vérifiera si l'utilisateur est l'administrateur de l'équipe.
       * J'enregistrerai ces politiques dans Program.cs.

  Phase 2 : Correction de la logique de l'API Backend

   1. Refactoring des Contrôleurs : Je remplacerai les vérifications de permissions manuelles dans les contrôleurs (TeamsController, SprintsController, etc.) par les
      nouveaux attributs [Authorize(Policy = "...")].

   2. Correction de `GET /api/users` : Je modifierai cet endpoint pour qu'il retourne une liste d'utilisateurs filtrée en fonction des équipes de l'appelant (sauf pour
      le super-admin).

   3. Correction de `POST /api/teams` : Je restreindrai cet endpoint pour qu'il ne soit plus accessible par le rôle user de base, mais seulement par les team-admin (et
      super-admin).

  Phase 3 : Mise à jour des Tests d'Intégration

   1. Mise à jour des Données de Test : Je vais enrichir le setup de test pour inclure un scénario plus complexe avec plusieurs équipes et utilisateurs aux rôles
      variés.
   2. Mise à jour des Tests : Je vais corriger et ajouter les tests nécessaires dans AuthorizationTests.cs pour valider tous les cas de la matrice de permissions, en
      particulier :
       * Qu'un user ne peut pas créer d'équipe.
       * Qu'un user voit bien une liste d'utilisateurs et de sprints limitée à ses équipes.
       * Que les politiques d'autorisation personnalisées (IsTeamAdmin, IsTeamMember) fonctionnent correctement pour bloquer/autoriser l'accès aux ressources.

  Phase 4 : Documentation

   1. Mise à jour du `README.md` : Une fois les changements validés et testés, j'ajouterai la matrice de permissions au fichier README.md pour documenter clairement les
      règles de l'application.