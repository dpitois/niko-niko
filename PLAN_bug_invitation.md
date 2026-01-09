### Plan pour la correction du bug où un utilisateur invité n'est pas associé à l'équipe : (COMPLETED)

1.  **Comprendre le flux d'invitation actuel :** (COMPLETED)
    *   Examiner la méthode `AcceptInvitation` dans `NikoNiko.Services/ITeamInvitationService.cs` et son implémentation dans `NikoNiko.Services/TeamInvitationService.cs`. (COMPLETED)
    *   Revoir l'action du contrôleur correspondante dans `NikoNiko.Api/Controllers/TeamInvitationsController.cs`. (COMPLETED)
    *   Analyser les modèles de données `TeamInvitation.cs` et `TeamUser.cs`. (COMPLETED)

2.  **Identifier la cause racine :** (COMPLETED)
    *   Déterminer pourquoi l'entrée `TeamUser` n'est pas correctement créée ou mise à jour après l'acceptation d'une invitation, liant l'utilisateur invité à l'équipe. (COMPLETED)

3.  **Mettre en œuvre le correctif :** (COMPLETED)
    *   Modifier `NikoNiko.Services/TeamInvitationService.cs` pour s'assurer qu'une entrée `TeamUser` est correctement créée ou mise à jour, liant l'`User.Id` (de l'utilisateur invité) et le `Team.Id` (de l'invitation) avec le rôle approprié (par exemple, `TeamMember`). (COMPLETED)

4.  **Ajouter/Mettre à jour les tests :** (COMPLETED)
    *   Créer un nouveau cas de test d'intégration ou améliorer un test existant dans `NikoNiko.Api.IntegrationTests/TeamInvitationTests.cs` pour simuler le flux d'acceptation d'invitation et vérifier que la relation `TeamUser` est correctement établie et persistée. (COMPLETED)

### Extension du plan : Gérer le flux d'invitation pour un nouvel utilisateur (EN COURS)

**Nouveau scénario :** Un utilisateur X (inexistant dans la base de données) reçoit un lien d'invitation. Après s'être connecté/inscrit via OAuth, il doit être automatiquement associé à l'équipe de l'invitation.

**Stratégie retenue : Option B - Utiliser l'URI de redirection avec jeton d'invitation comme paramètre d'état.**

**Plan d'action :**

1.  **Frontend (`app/frontend`) :**
    *   **1.1 Détecter et stocker le jeton d'invitation (dans `App.tsx` et `AcceptInvitationPage.tsx`) :**
        *   Lors de l'accès à la route `/accept-invitation/:token`, extraire le `token` de l'URL.
        *   Stocker ce `token` dans `sessionStorage` sous une clé spécifique (ex: `invitationToken`).
        *   Rediriger l'utilisateur vers la page de `/login`.
    *   **1.2 Initialiser le flux OAuth avec le jeton (dans `LoginPage.tsx`) :**
        *   Lors du chargement de `LoginPage`, vérifier la présence d'un `invitationToken` dans `sessionStorage`.
        *   Si un `invitationToken` est trouvé, l'ajouter comme paramètre de requête à l'URL de redirection OAuth (ex: `/api/auth/login-github?invitationToken=XYZ`) lorsque l'utilisateur clique sur le bouton de connexion GitHub.
    *   **1.3 Nettoyer le jeton (dans `AuthCallbackPage.tsx`) :**
        *   Après la redirection OAuth et la gestion du JWT par le backend (qui aura traité le `invitationToken`), supprimer l'`invitationToken` de `sessionStorage`.

2.  **Backend (`api/NikoNiko.Api`) :**
    *   **2.1 Réception du jeton d'invitation post-OAuth (COMPLETED) :**
        *   Modifier les méthodes `LoginGitHub` pour accepter `invitationToken` en paramètre et l'ajouter aux `AuthenticationProperties.Items`. (COMPLETED)
        *   Modifier les méthodes `SigninGitHub` pour récupérer `invitationToken` des `AuthenticateResult.Properties.Items`. (COMPLETED)
    *   **2.2 Traitement du jeton d'invitation après authentification/création de l'utilisateur (COMPLETED) :**
        *   Dans `AuthController.HandleSignIn`, après la création ou la récupération de l'utilisateur (`user`) et la génération du JWT (`token`), appeler `_teamInvitationService.AcceptTeamInvitationAsync(invitationToken, user.Id)` si `invitationToken` est présent. (COMPLETED)
    *   **2.3 Redirection finale :** Le `redirectUrl` final vers le frontend (`_frontendRedirectUrl/auth/callback?token={jwtToken}`) devrait être construit sans le jeton d'invitation, car l'acceptation aura déjà été traitée par le backend.

3.  **Mise à jour des tests d'intégration (`NikoNiko.Api.IntegrationTests`) :**
    *   **3.1 Ajouter un nouveau cas de test :** Simuler un utilisateur inexistant qui clique sur un lien d'invitation, est redirigé vers OAuth, est créé par `HandleSignIn`, puis est automatiquement associé à l'équipe via l'invitation.