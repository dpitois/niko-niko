# Implementation Plan - Ajout du Login Discord

## 1. 🔍 Analyse & Contexte
*   **Objectif:** Ajouter l'authentification via Discord en suivant le modèle existant (GitHub/Google).
*   **Fichiers Affectés:**
    *   `api/NikoNiko.Api/NikoNiko.Api.csproj` (Packages)
    *   `api/NikoNiko.Api/appsettings.json` (Configuration)
    *   `api/NikoNiko.Api/Program.cs` (Enregistrement du service)
    *   `api/NikoNiko.Api/Controllers/AuthController.cs` (Endpoints)
    *   `app/frontend/src/pages/LoginPage.tsx` (Interface Utilisateur)
    *   `app/frontend/src/i18n/locales/en.json` & `fr.json` (Traductions)
*   **Dépendances Clés:**
    *   Package NuGet: `AspNet.Security.OAuth.Discord`
    *   Compte Développeur Discord (Client ID/Secret - à configurer par l'utilisateur)
*   **Risques/Inconnues:**
    *   L'icône Discord n'est pas nativement dans `@mui/icons-material`. Il faudra créer un composant `SvgIcon` personnalisé.

## 2. 📋 Checklist
- [x] Etape 1: Ajouter le package NuGet Discord au backend
- [x] Etape 2: Configurer `appsettings.json` avec les placeholders Discord
- [x] Etape 2bis: Mettre à jour `.env.template` et `docker-compose.yml`
- [x] Etape 3: Enregistrer le service d'authentification Discord dans `Program.cs`
- [x] Etape 4: Implémenter les endpoints `LoginDiscord` et `SigninDiscord` dans `AuthController`
- [x] Etape 5: Ajouter les traductions pour le bouton Discord
- [x] Etape 6: Ajouter le bouton et l'icône Discord sur la page de Login

...

### Etape 1: Ajouter le package NuGet Discord au backend
*   **Goal:** Installer la librairie nécessaire pour l'OAuth Discord.
*   **Status:** [x]
*   **Action:**
    *   Exécuter la commande shell pour ajouter le package : `dotnet add api/NikoNiko.Api/NikoNiko.Api.csproj package AspNet.Security.OAuth.Discord`
*   **Verification:** Vérifier que le fichier `.csproj` contient bien la référence.

### Etape 2: Configurer `appsettings.json` avec les placeholders Discord
*   **Goal:** Préparer la structure de configuration pour les secrets Discord.
*   **Status:** [x]
*   **Action:**
    *   Modifier `api/NikoNiko.Api/appsettings.json`.
    *   Ajouter la section `Discord` sous `Authentication`.
    ```json
    "Authentication": {
      "GitHub": { ... },
      "Google": { ... },
      "Discord": {
        "ClientId": "YOUR_DISCORD_CLIENT_ID",
        "ClientSecret": "YOUR_DISCORD_CLIENT_SECRET"
      },
      ...
    }
    ```
*   **Verification:** Lire le fichier pour confirmer la structure.

### Etape 2bis: Mettre à jour `.env.template` et `docker-compose.yml`
*   **Goal:** Assurer la propagation des secrets Discord via Docker.
*   **Status:** [x]
*   **Action:**
    *   Modifier `.env.template` : ajouter `DISCORD_CLIENT_ID` et `DISCORD_CLIENT_SECRET`.
    *   Modifier `docker-compose.yml` : ajouter le mapping dans la section `environment` du service `backend`.
*   **Verification:** Vérifier la présence des variables dans les deux fichiers.

### Etape 3: Enregistrer le service d'authentification Discord dans `Program.cs`
*   **Goal:** Activer le provider Discord dans le pipeline d'authentification ASP.NET Core.
*   **Status:** [>]
*   **Action:**
    *   Modifier `api/NikoNiko.Api/Program.cs`.
    *   Ajouter la logique conditionnelle similaire à Google pour ajouter `.AddDiscord()`.
    ```csharp
    // ... après Google
    var discordClientId = config["Authentication:Discord:ClientId"];
    var discordClientSecret = config["Authentication:Discord:ClientSecret"];

    if (!string.IsNullOrEmpty(discordClientId) && !string.IsNullOrEmpty(discordClientSecret))
    {
        builder.Services.AddAuthentication().AddDiscord(options =>
        {
            options.SignInScheme = "ExternalCookie";
            options.ClientId = discordClientId;
            options.ClientSecret = discordClientSecret;
            options.CallbackPath = "/signin-discord";
            // Map claims if necessary, usually Discord returns clear claims
        });
    }
    ```
*   **Verification:** La compilation (`dotnet build`) doit réussir.

### Etape 4: Implémenter les endpoints `LoginDiscord` et `SigninDiscord` dans `AuthController`
*   **Goal:** Créer les points d'entrée et de retour pour le flux OAuth.
*   **Status:** [x]
*   **Action:**
    *   Modifier `api/NikoNiko.Api/Controllers/AuthController.cs`.
    *   Ajouter `LoginDiscord` (déclenche le Challenge).
    *   Ajouter `SigninDiscord` (gère le callback).
    *   Mettre à jour `HandleSignIn` pour gérer le mapping des claims Discord (notamment l'avatar qui demande une construction d'URL spécifique ou l'usage de claims particuliers).
        *   *Note:* Discord fournit l'ID de l'avatar, l'URL complète doit souvent être construite : `https://cdn.discordapp.com/avatars/{user_id}/{avatar_hash}.png`. Le provider peut le faire ou on le fait manuellement.
*   **Verification:** Vérifier que le code compile.

### Etape 5: Ajouter les traductions pour le bouton Discord
*   **Goal:** Assurer l'i18n du nouveau bouton.
*   **Status:** [x]
*   **Action:**
    *   Modifier `app/frontend/src/i18n/locales/en.json` : Ajouter `"signInDiscord": "Sign in with Discord"`.
    *   Modifier `app/frontend/src/i18n/locales/fr.json` : Ajouter `"signInDiscord": "Se connecter avec Discord"`.
*   **Verification:** Lire les fichiers JSON.

### Etape 6: Ajouter le bouton et l'icône Discord sur la page de Login
*   **Goal:** Rendre l'option visible pour l'utilisateur.
*   **Status:** [x]
*   **Action:**
    *   Modifier `app/frontend/src/pages/LoginPage.tsx`.
    *   Créer/Intégrer une icône Discord (SVG path dans un composant `SvgIcon` local ou inline).
    *   Ajouter le bouton `Button` avec la couleur `#5865F2` (Discord Blurple).
    *   Utiliser la traduction `login.signInDiscord`.
*   **Verification:** Lancer le frontend et vérifier visuellement (si possible) ou vérifier le code.

## 4. 🧪 Testing Strategy
*   **Unit Tests:** Non applicable (intégration principalement).
*   **Integration Tests:** Il faudrait mocker l'auth externe, mais on peut vérifier que l'endpoint `/api/auth/login-discord` redirige bien (302) vers discord.com.
*   **Manual Verification:**
    1.  Configurer un `ClientId`/`ClientSecret` valide dans `.env` ou `appsettings.json` (via User Secrets idéalement).
    2.  Lancer l'app (`docker compose up`).
    3.  Aller sur la page de login.
    4.  Cliquer sur "Se connecter avec Discord".
    5.  Vérifier la redirection, l'auth Discord, et le retour sur l'app avec un utilisateur créé/connecté.

## 5. ✅ Success Criteria
*   Le bouton "Se connecter avec Discord" apparaît sur la page de login avec le bon style.
*   Le clic redirige vers Discord OAuth.
*   Le retour de Discord connecte l'utilisateur et crée son compte si inexistant.
*   L'avatar et le nom de l'utilisateur sont correctement récupérés de Discord.
