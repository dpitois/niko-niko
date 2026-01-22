# Implementation Plan - Fix User Deletion (Foreign Key Constraint)

## 1. 🔍 Analysis & Context
*   **Objective:** Allow the deletion of a user who has accepted a team invitation without causing a foreign key constraint violation.
*   **Affected Files:**
    *   `api/NikoNiko.Data/ApplicationDbContext.cs` (Entity Framework Core Configuration)
    *   `api/NikoNiko.Api.IntegrationTests/UsersControllerTests.cs` (New reproduction test)
*   **Key Dependencies:** Entity Framework Core (`DeleteBehavior`).
*   **Risks/Unknowns:** Ensure that switching to `SetNull` does not negatively impact other business logic that might rely on the user presence in invitations (e.g., audit logs).

## 2. 📋 Checklist
- [ ] Step 1: Create a reproduction test case (expected to fail initially).
- [ ] Step 2: Modify `OnDelete` configuration in `ApplicationDbContext`.
- [ ] Step 3: Generate and apply the EF Core migration.
- [ ] Verification: The test must pass.

## 3. 📝 Step-by-Step Implementation Details

### Step 1: Reproduction Test Case
*   **Goal:** Confirm that deleting an invited user currently fails.
*   **Action:**
    *   Modify `api/NikoNiko.Api.IntegrationTests/UsersControllerTests.cs`.
    *   Add method `DeleteUser_WithAcceptedInvitation_ShouldSucceed`.
    *   Scenario:
        1.  Authenticate as Super Admin.
        2.  Create User A (Invited).
        3.  Create a Team and an Invitation.
        4.  Link User A to the Invitation as `AcceptedByUser`.
        5.  Call `DELETE /api/users/{id}` for User A.
        6.  Assert: Should return `204 NoContent`.

### Step 2: Fix Database Configuration
*   **Goal:** Allow deletion by setting the foreign key to null.
*   **Action:**
    *   Modify `api/NikoNiko.Data/ApplicationDbContext.cs` in `OnModelCreating`.
    *   Change:
        ```csharp
        modelBuilder.Entity<TeamInvitation>()
            .HasOne(ti => ti.AcceptedByUser)
            .WithMany()
            .HasForeignKey(ti => ti.AcceptedByUserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict); // Change to DeleteBehavior.SetNull
        ```
    *   To: `DeleteBehavior.SetNull`.

### Step 3: Create Migration
*   **Goal:** Update the database schema.
*   **Action:**
    *   Run `dotnet ef migrations add FixTeamInvitationDeleteBehavior --project ../NikoNiko.Data --startup-project .` inside the `api/NikoNiko.Api` directory within the container.

## 4. 🧪 Testing Strategy
*   **Integration Tests:** Run the new test `DeleteUser_WithAcceptedInvitation_ShouldSucceed`.
*   **Manual Verification:** Attempt to delete the user via API/UI.

## 5. ✅ Success Criteria
*   `DELETE /api/users/{id}` returns `204 NoContent` for a user who accepted an invitation.
*   The `TeamInvitations` record remains but `AcceptedByUserId` is set to `null`.
