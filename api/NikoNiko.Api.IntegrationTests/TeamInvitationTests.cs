using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using NikoNiko.Core.DTOs.Team.Invitation;
using NikoNiko.Core.Models;
using NikoNiko.Data;
using NikoNiko.Services;

using Xunit;

namespace NikoNiko.Api.IntegrationTests;

public class TeamInvitationTests
{
    [Fact]
    public async Task AcceptInvitation_WhenTokenIsValid_ShouldAddUserToTeamAndInvalidateInvitation()
    {
        // Arrange
        await using var application = new NikoNikoApiTestApplication();

        // 1. Setup initial data
        var teamAdmin = new User { Id = Guid.NewGuid(), Name = "Admin User", OAuthId = "github|admin", Email = "admin@example.com" };
        var newMember = new User { Id = Guid.NewGuid(), Name = "New Member", OAuthId = "github|newmember", Email = "newmember@example.com" };
        var team = new Team { Id = Guid.NewGuid(), Name = "Test Team", AdminId = teamAdmin.Id };

        // Use a separate scope to seed data and ensure it's saved before the next step
        using (var scope = application.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Users.AddRange(teamAdmin, newMember);
            dbContext.Teams.Add(team);
            await dbContext.SaveChangesAsync();
        }

        // 2. Create an invitation
        string invitationToken;
        Guid invitationId;
        using (var scope = application.Services.CreateScope())
        {
            var invitationService = scope.ServiceProvider.GetRequiredService<ITeamInvitationService>();
            var createDto = new CreateTeamInvitationDto { TeamId = team.Id, ExpirationInDays = 1 };
            var invitationDto = await invitationService.CreateTeamInvitationAsync(team.Id, teamAdmin.Id, createDto);
            invitationToken = invitationDto.Token;
            invitationId = invitationDto.Id;
        }

        // 3. Create a client authenticated as the new member
        var client = application.CreateClient();
        string jwtToken;
        using (var scope = application.Services.CreateScope())
        {
            var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, newMember.Id.ToString()) };
            jwtToken = tokenService.GenerateToken(claims);
        }
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

        // Act
        var response = await client.PostAsync($"/api/teaminvitations/{invitationToken}/accept", null);

        // Assert
        response.EnsureSuccessStatusCode(); // Status 2xx

        // Verify the database state
        using (var scope = application.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Assertion 1: User is now a member of the team
            var isMember = await dbContext.TeamUsers.AnyAsync(tu => tu.TeamId == team.Id && tu.UserId == newMember.Id);
            Assert.True(isMember, "User should have been added to the team.");

            // Assertion 2: Invitation is invalidated
            var invitation = await dbContext.TeamInvitations.IgnoreQueryFilters().FirstOrDefaultAsync(ti => ti.Id == invitationId);
            Assert.NotNull(invitation);
            // The service sets Status to "Accepted". The bug was that IsDeleted was not set.
            Assert.Equal("Accepted", invitation.Status);
            // This is the key assertion for the bug fix
            Assert.True(invitation.IsDeleted, "Invitation should be marked as soft-deleted (IsDeleted = true).");
        }
    }

    [Fact]
    public async Task DeleteInvitation_AsTeamAdmin_ShouldMarkAsDeleted()
    {
        // Arrange
        await using var application = new NikoNikoApiTestApplication();

        // 1. Setup initial data
        var teamAdmin = new User { Id = Guid.NewGuid(), Name = "Admin User", OAuthId = "github|admin_del", Email = "admin_del@example.com" };
        var team = new Team { Id = Guid.NewGuid(), Name = "Test Team Delete", AdminId = teamAdmin.Id };

        using (var scope = application.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Users.Add(teamAdmin);
            dbContext.Teams.Add(team);
            await dbContext.SaveChangesAsync();
        }

        // 2. Create an invitation
        Guid invitationId;
        using (var scope = application.Services.CreateScope())
        {
            var invitationService = scope.ServiceProvider.GetRequiredService<ITeamInvitationService>();
            var createDto = new CreateTeamInvitationDto { TeamId = team.Id, ExpirationInDays = 1 };
            var invitationDto = await invitationService.CreateTeamInvitationAsync(team.Id, teamAdmin.Id, createDto);
            invitationId = invitationDto.Id;
        }

        // 3. Create a client authenticated as the team admin
        var client = application.CreateClient();
        string jwtToken;
        using (var scope = application.Services.CreateScope())
        {
            var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, teamAdmin.Id.ToString()) };
            jwtToken = tokenService.GenerateToken(claims);
        }
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

        // Act: Use the new route
        var response = await client.DeleteAsync($"/api/teams/{team.Id}/invitations/{invitationId}");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);

        // Verify the database state
        using (var scope = application.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var invitation = await dbContext.TeamInvitations.IgnoreQueryFilters().FirstOrDefaultAsync(ti => ti.Id == invitationId);
            Assert.NotNull(invitation);
            Assert.True(invitation.IsDeleted, "Invitation should be marked as soft-deleted (IsDeleted = true).");
        }
    }

    [Fact]
    public async Task DeleteInvitation_WithWrongTeamId_ShouldReturnBadRequest()
    {
        // Arrange
        await using var application = new NikoNikoApiTestApplication();

        // 1. Setup initial data
        var teamAdmin = new User { Id = Guid.NewGuid(), Name = "Admin User", OAuthId = "github|admin_del_fail", Email = "admin_del_fail@example.com" };
        var team = new Team { Id = Guid.NewGuid(), Name = "Test Team 1", AdminId = teamAdmin.Id };
        var otherTeamId = Guid.NewGuid();

        using (var scope = application.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Users.Add(teamAdmin);
            dbContext.Teams.Add(team);
            await dbContext.SaveChangesAsync();
        }

        // 2. Create an invitation for team 1
        Guid invitationId;
        using (var scope = application.Services.CreateScope())
        {
            var invitationService = scope.ServiceProvider.GetRequiredService<ITeamInvitationService>();
            var createDto = new CreateTeamInvitationDto { TeamId = team.Id, ExpirationInDays = 1 };
            var invitationDto = await invitationService.CreateTeamInvitationAsync(team.Id, teamAdmin.Id, createDto);
            invitationId = invitationDto.Id;
        }

        // 3. Create a client authenticated as the team admin (who is also super admin or just admin of team 1)
        var client = application.CreateClient();
        string jwtToken;
        using (var scope = application.Services.CreateScope())
        {
            var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
            var claims = new[] {
                            new Claim(ClaimTypes.NameIdentifier, teamAdmin.Id.ToString()),
                            new Claim("is_super_admin", "true") // Use super admin to bypass policy check on otherTeamId but trigger controller check
                        };
            jwtToken = tokenService.GenerateToken(claims);
        }
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

        // Act: Try to delete invitation of team 1 using otherTeamId in route
        var response = await client.DeleteAsync($"/api/teams/{otherTeamId}/invitations/{invitationId}");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AcceptInvitation_WhenUserIsAlreadyMember_ShouldNotAddDuplicateAndInvalidateInvitation()
    {
        // Arrange
        await using var application = new NikoNikoApiTestApplication();

        // 1. Setup initial data
        var teamAdmin = new User { Id = Guid.NewGuid(), Name = "Admin User", OAuthId = "github|admin2", Email = "admin2@example.com" };
        var existingMember = new User { Id = Guid.NewGuid(), Name = "Existing Member", OAuthId = "github|existingmember", Email = "existingmember@example.com" };
        var team = new Team { Id = Guid.NewGuid(), Name = "Another Test Team", AdminId = teamAdmin.Id };

        // Use a separate scope to seed data and ensure it's saved before the next step
        using (var scope = application.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Users.AddRange(teamAdmin, existingMember);
            dbContext.Teams.Add(team);
            // Manually add the user as an existing member to the team
            dbContext.TeamUsers.Add(new TeamUser { TeamId = team.Id, UserId = existingMember.Id });
            await dbContext.SaveChangesAsync();
        }

        // 2. Create an invitation for the already existing member
        string invitationToken;
        Guid invitationId;
        using (var scope = application.Services.CreateScope())
        {
            var invitationService = scope.ServiceProvider.GetRequiredService<ITeamInvitationService>();
            var createDto = new CreateTeamInvitationDto { TeamId = team.Id, ExpirationInDays = 1 };
            var invitationDto = await invitationService.CreateTeamInvitationAsync(team.Id, teamAdmin.Id, createDto);
            invitationToken = invitationDto.Token;
            invitationId = invitationDto.Id;
        }

        // 3. Create a client authenticated as the existing member
        var client = application.CreateClient();
        string jwtToken;
        using (var scope = application.Services.CreateScope())
        {
            var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, existingMember.Id.ToString()) };
            jwtToken = tokenService.GenerateToken(claims);
        }
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

        // Act
        var response = await client.PostAsync($"/api/teaminvitations/{invitationToken}/accept", null);

        // Assert
        response.EnsureSuccessStatusCode(); // Status 2xx

        // Verify the database state
        using (var scope = application.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Assertion 1: Ensure only one TeamUser entry exists for this member
            var teamUsersCount = await dbContext.TeamUsers
                .CountAsync(tu => tu.TeamId == team.Id && tu.UserId == existingMember.Id);
            Assert.Equal(1, teamUsersCount);

            // Assertion 2: Invitation is invalidated
            var invitation = await dbContext.TeamInvitations.IgnoreQueryFilters().FirstOrDefaultAsync(ti => ti.Id == invitationId);
            Assert.NotNull(invitation);
            Assert.Equal("Accepted", invitation.Status);
            Assert.True(invitation.IsDeleted, "Invitation should be marked as soft-deleted (IsDeleted = true).");
        }
    }
    [Fact]
    public async Task AcceptInvitation_WhenNewUserRegistersViaOAuthWithToken_ShouldCreateUserAndAddThemToTeam()
    {
        // Arrange
        await using var application = new NikoNikoApiTestApplication();

        // 1. Setup initial data
        var teamAdmin = new User { Id = Guid.NewGuid(), Name = "Admin User New", OAuthId = "github|adminnew", Email = "adminnew@example.com" };
        var team = new Team { Id = Guid.NewGuid(), Name = "New User Test Team", AdminId = teamAdmin.Id };

        using (var scope = application.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Users.Add(teamAdmin);
            dbContext.Teams.Add(team);
            await dbContext.SaveChangesAsync();
        }

        // 2. Create an invitation
        string invitationToken;
        Guid invitationId;
        using (var scope = application.Services.CreateScope())
        {
            var invitationService = scope.ServiceProvider.GetRequiredService<ITeamInvitationService>();
            var createDto = new CreateTeamInvitationDto { TeamId = team.Id, ExpirationInDays = 1 };
            var invitationDto = await invitationService.CreateTeamInvitationAsync(team.Id, teamAdmin.Id, createDto);
            invitationToken = invitationDto.Token;
            invitationId = invitationDto.Id;
        }

        // 3. Act: Simulate a new user logging in via GitHub with the invitation token
        // This will go through AuthController.LoginGitHub, TestAuthenticationHandler, AuthController.SigninGitHub, and AuthController.HandleSignIn
        var client = application.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        // Simulate the client hitting /api/auth/login-github with the invitation token
        var loginResponse = await client.GetAsync($"/api/auth/login-github?invitationToken={invitationToken}");

        // Ensure the initial login request was redirected
        Assert.Equal(System.Net.HttpStatusCode.Redirect, loginResponse.StatusCode);

        // Extract the redirect location, which should be to /api/auth/signin-github
        var redirectUri = loginResponse.Headers.Location;
        Assert.NotNull(redirectUri);
        Assert.Contains("/api/auth/signin-github", redirectUri.ToString());

        // Follow the redirect to the sign-in callback
        var signInResponse = await client.GetAsync(redirectUri);
        Assert.Equal(System.Net.HttpStatusCode.Redirect, signInResponse.StatusCode);

        // The final redirect from AuthController.SigninGitHub is to the frontend callback,
        // which contains the JWT token.
        var finalRedirectUri = signInResponse.Headers.Location;
        Assert.NotNull(finalRedirectUri);
        Assert.Contains("/auth/callback", finalRedirectUri.ToString());

        // Extract the JWT token from the final redirect URI
        var jwtToken = finalRedirectUri.ToString().Split("token=")[1];
        Assert.False(string.IsNullOrEmpty(jwtToken), "JWT token should be present in the redirect URL.");

        // 4. Assert: Verify the database state after the flow
        using (var scope = application.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // The TestAuthenticationHandler creates a user with email "newtestuser@example.com"
            var newUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == "newtestuser@example.com");
            Assert.NotNull(newUser);

            // Assertion 1: New user is now a member of the team
            var isMember = await dbContext.TeamUsers.AnyAsync(tu => tu.TeamId == team.Id && tu.UserId == newUser.Id);
            Assert.True(isMember, "New user should have been added to the team.");

            // Assertion 2: Invitation is invalidated
            var invitation = await dbContext.TeamInvitations.IgnoreQueryFilters().FirstOrDefaultAsync(ti => ti.Id == invitationId);
            Assert.NotNull(invitation);
            Assert.Equal("Accepted", invitation.Status);
            Assert.True(invitation.IsDeleted, "Invitation should be marked as soft-deleted (IsDeleted = true).");
        }
    }
}