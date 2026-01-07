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

#### **Étape 1 : Backend - Mise à Jour du Modèle de Données**

1.  **Modification de l'entité :** J'ajoute la propriété `public bool IsSuperAdmin { get; set; }` à la classe `User` dans `NikoNiko.Core/Models/User.cs`.
2.  **Migration de la base de données :** Je génère une nouvelle migration EF Core pour appliquer cette colonne à la base de données.

---

#### **Étape 2 : Backend - Logique d'Assignation du Rôle `super-admin`**

1.  **Service au Démarrage :** Je crée une logique dans `Program.cs` qui s'exécute au lancement de l'API.
2.  **Lecture de la Configuration :** Ce service lit la variable d'environnement `SUPER_ADMINS` (liste d'emails).
3.  **Synchronisation :** Il met à jour en base de données les utilisateurs qui doivent devenir ou cesser d'être `super-admin` en fonction de cette liste.
4.  **Logique de Repli :** Si la variable d'environnement est vide, la règle du "premier utilisateur devient administrateur" sera appliquée comme fallback.

---

#### **Étape 3 : Backend - Sécurisation et Tests d'Intégration**

1.  **Mise à jour du Jeton JWT :** J'ajoute la `claim` `is_super_admin` au jeton généré par `TokenService.cs` si l'utilisateur a le rôle.
2.  **Politiques d'Autorisation :** Je définis des politiques d'autorisation claires (ex: "IsSuperAdminPolicy") dans `Program.cs`.
3.  **Protection des Endpoints :** J'applique ces politiques sur les actions des contrôleurs pour restreindre l'accès (`TeamsController`, `UsersController`, etc.).
4.  **Écriture des Tests d'Intégration :** Dans le projet `NikoNiko.Api.IntegrationTests`, je vais créer de nouveaux tests pour couvrir les scénarios suivants :
    *   Un utilisateur non authentifié est bien rejeté (401 Unauthorized).
    *   Un utilisateur standard qui tente d'accéder à une ressource d'administrateur (ex: lister tous les utilisateurs) reçoit une erreur (403 Forbidden).
    *   Un `team-admin` peut gérer les ressources de son équipe, mais pas celles des autres.
    *   Un `super-admin` a bien accès à toutes les ressources, y compris la suppression de n'importe quelle équipe.

---

#### **Étape 4 : Frontend - Adaptation de l'Interface Utilisateur**

1.  **Contexte d'Authentification :** Je mets à jour `AuthContext.tsx` pour extraire et stocker le statut `is_super_admin` depuis le jeton JWT.
2.  **Affichage Conditionnel :** J'utilise ce statut pour afficher/masquer les éléments de l'interface (boutons de suppression, liens vers les panneaux d'administration) en fonction des droits de l'utilisateur connecté.

---

#### **Étape 5 : Configuration**

1.  **Mise à jour de `docker-compose.yml` :** J'ajoute la variable d'environnement `SUPER_ADMINS` au service `backend`.
2.  **Mise à jour du Template `.env.template` :** Je documente la nouvelle variable pour expliquer son utilité et son format.