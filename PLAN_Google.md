### Plan pour l'implémentation et la documentation de la connexion via Google :

1.  **Backend - Configuration OAuth Google (NikoNiko.Api) :**
    *   Ajouter la configuration du gestionnaire d'authentification `AddGoogle` dans `Program.cs`, en suivant les modèles existants pour d'autres fournisseurs comme GitHub.
    *   Définir les clés de configuration pour `Authentication:Google:ClientId` et `Authentication:Google:ClientSecret` dans `appsettings.json` et `docker-compose.yml`.
    *   Modifier `AuthController.cs` pour gérer Google en tant que fournisseur de connexion externe dans les méthodes `ExternalLogin` et `ExternalLoginCallback`.

2.  **Frontend - Intégration du bouton de connexion Google (app/frontend) :**
    *   Ajouter un bouton/lien "Se connecter avec Google" sur la page de connexion (par exemple, `LoginPage.tsx`).
    *   Mettre à jour le contexte/hook d'authentification (par exemple, `AuthContext.tsx`, `useAuth.ts`) pour gérer le flux de connexion Google.
    *   S'assurer que le frontend gère correctement l'URL de rappel après l'authentification Google.

3.  **Documentation :**
    *   Ajouter une section au `README.md` (ou un `docs/Authentication.md` dédié) détaillant les étapes pour configurer Google OAuth dans la Google Developer Console, obtenir les identifiants et configurer l'application.
