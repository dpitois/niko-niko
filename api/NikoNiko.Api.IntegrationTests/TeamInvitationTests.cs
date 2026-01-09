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
}