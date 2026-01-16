# Implementation Plan - Transfert d'Administration d'Équipe & Correctif Invitations

## 1. 🔍 Analysis & Context
*   **Objective:**
    1.  Permettre le transfert explicite de la propriété d'une équipe à un autre membre, avec rétrogradation de l'ancien admin.
    2.  **Correctif:** Résoudre le bug empêchant la création d'invitations (403 pour Team Admin, 400 pour Global Admin).
*   **Affected Files:**
    *   `api/NikoNiko.Core/DTOs/Team/UpdateTeamAdminDto.cs` (New)
    *   `api/NikoNiko.Api/Controllers/TeamsController.cs`
    *   `api/NikoNiko.Api/Controllers/TeamInvitationsController.cs`
    *   `api/NikoNiko.Services/ITeamInvitationService.cs`
    *   `api/NikoNiko.Services/TeamInvitationService.cs`
    *   `app/frontend/src/services/teamService.ts`
    *   `app/frontend/src/services/teamInvitationService.ts`
    *   `app/frontend/src/components/EditTeamDialog.tsx`
    *   `app/frontend/src/components/AdminTeamListItem.tsx`
*   **Key Dependencies:** Entity Framework Core, Auth Policies (`IsTeamAdmin`), Routing.
*   **Risks/Unknowns:**
    *   Changement de route pour les invitations : s'assurer que le frontend est bien aligné.
    *   Le transfert d'admin implique une gestion fine des droits pour l'utilisateur connecté (rafraîchissement UI).

## 2. 📋 Checklist
- [ ] **Fix Invitations:** Modifier la route `CreateInvitation` pour inclure `teamId` (fix 403).
- [ ] **Fix Invitations:** Mettre à jour le service pour autoriser le Super Admin (fix 400).
- [ ] **Fix Invitations:** Mettre à jour l'appel frontend correspondant.
- [ ] **Transfert Admin:** Créer `UpdateTeamAdminDto`.
- [ ] **Transfert Admin:** Implémenter `PUT /api/teams/{id}/admin`.
- [ ] **Transfert Admin:** Ajouter méthode frontend `transferTeamAdmin`.
- [ ] **Transfert Admin:** Mettre à jour `EditTeamDialog` et `AdminTeamListItem`.
- [ ] Verification: Tester les invitations (Admin & Super Admin) et le transfert.

## 3. 📝 Step-by-Step Implementation Details

### Step 1: Fix Invitations (Backend)
*   **Goal:** Corriger les erreurs 403 et 400 lors de la création d'invitations.
*   **Action:**
    *   Modify `api/NikoNiko.Api/Controllers/TeamInvitationsController.cs`:
        *   Change `CreateTeamInvitation` attribute to `[HttpPost("/api/teams/{teamId}/invitations")]`.
        *   Update signature to accept `teamId` from route.
        *   Pass `isSuperAdmin` (from `User.HasClaim`) to the service.
    *   Modify `api/NikoNiko.Services/ITeamInvitationService.cs` & `TeamInvitationService.cs`:
        *   Update `CreateTeamInvitationAsync` signature to accept `bool isSuperAdmin`.
        *   Update logic: `if (team.AdminId != creatorUserId && !isSuperAdmin) throw ...`
*   **Verification:** Swagger call to create invitation as Team Admin (should work) and Super Admin (should work).

### Step 2: Fix Invitations (Frontend)
*   **Goal:** Aligner le frontend sur la nouvelle route API.
*   **Action:**
    *   Modify `app/frontend/src/services/teamInvitationService.ts`:
        *   Update `createTeamInvitation` to use url `/api/teams/${teamId}/invitations`.
*   **Verification:** Check form submission in UI.

### Step 3: Transfert Admin - DTO & Endpoint
*   **Goal:** Créer le contrat et la logique pour le transfert.
*   **Action:**
    *   Create `api/NikoNiko.Core/DTOs/Team/UpdateTeamAdminDto.cs`:
        ```csharp
        public class UpdateTeamAdminDto { public Guid NewAdminId { get; set; } }
        ```
    *   Modify `api/NikoNiko.Api/Controllers/TeamsController.cs`:
        *   Add `[HttpPut("{teamId}/admin")] UpdateTeamAdmin`.
        *   **Logic:**
            1.  Check rights (Policy).
            2.  Update `Team.AdminId`.
            3.  Ensure old admin is added to `TeamUsers` if not present.
            4.  Save.
*   **Verification:** API call returns 204.

### Step 4: Transfert Admin - Frontend Integration
*   **Goal:** Interface utilisateur pour le transfert.
*   **Action:**
    *   Modify `app/frontend/src/services/teamService.ts`: Add `transferTeamAdmin`.
    *   Modify `app/frontend/src/components/EditTeamDialog.tsx`:
        *   Add "Transfer Administration" section (danger zone).
        *   Select dropdown with members (filtered).
        *   Confirm button.
    *   Modify `app/frontend/src/components/AdminTeamListItem.tsx`: Pass props.
*   **Verification:** Manual test of transfer flow.

## 4. 🧪 Testing Strategy
*   **Scenario 1 (Fix):** Team Admin creates invitation -> Success.
*   **Scenario 2 (Fix):** Super Admin creates invitation -> Success.
*   **Scenario 3 (Transfer):** Admin transfers team to Member -> Success, Old Admin becomes Member.

## 5. ✅ Success Criteria
*   Les invitations fonctionnent pour tous les admins légitimes.
*   Le transfert d'admin est fonctionnel et sécurisé.