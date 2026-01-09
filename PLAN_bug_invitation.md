### Plan pour la correction du bug où un utilisateur invité n'est pas associé à l'équipe :

1.  **Comprendre le flux d'invitation actuel :**
    *   Examiner la méthode `AcceptInvitation` dans `NikoNiko.Services/ITeamInvitationService.cs` et son implémentation dans `NikoNiko.Services/TeamInvitationService.cs`.
    *   Revoir l'action du contrôleur correspondante dans `NikoNiko.Api/Controllers/TeamInvitationsController.cs`.
    *   Analyser les modèles de données `TeamInvitation.cs` et `TeamUser.cs`.

2.  **Identifier la cause racine :**
    *   Déterminer pourquoi l'entrée `TeamUser` n'est pas correctement créée ou mise à jour après l'acceptation d'une invitation, liant l'utilisateur invité à l'équipe.

3.  **Mettre en œuvre le correctif :**
    *   Modifier `NikoNiko.Services/TeamInvitationService.cs` pour s'assurer qu'une entrée `TeamUser` est correctement créée ou mise à jour, liant l'`User.Id` (de l'utilisateur invité) et le `Team.Id` (de l'invitation) avec le rôle approprié (par exemple, `TeamMember`).

4.  **Ajouter/Mettre à jour les tests :**
    *   Créer un nouveau cas de test d'intégration ou améliorer un test existant dans `NikoNiko.Api.IntegrationTests/TeamInvitationTests.cs` pour simuler le flux d'acceptation d'invitation et vérifier que la relation `TeamUser` est correctement établie et persistée.
