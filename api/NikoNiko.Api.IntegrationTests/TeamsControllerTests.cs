using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using NikoNiko.Core.DTOs.Mood;
using NikoNiko.Core.DTOs.Sprint;
using NikoNiko.Core.DTOs.Team;
using NikoNiko.Core.DTOs.User;
using NikoNiko.Core.Models;
using NikoNiko.Data;

using Xunit;

namespace NikoNiko.Api.IntegrationTests;

public class TeamsControllerTests
{
    [Fact]
    public async Task CreateTeam_AsRegularUser_ReturnsCreated()
    {
        // Arrange
        await using var application = new NikoNikoApiTestApplication();
        var (user, client, _) = await application.CreateUserAndClient("New Team Admin");
        var createTeamDto = new CreateTeamDto { Name = "My New Team" };

        // Act
        var response = await client.PostAsJsonAsync("/api/teams", createTeamDto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var team = await response.Content.ReadFromJsonAsync<TeamDto>();
        Assert.NotNull(team);
        Assert.Equal("My New Team", team.Name);
        Assert.Equal(user.Id, team.AdminId);
    }

    [Fact]
    public async Task CreateTeam_WhenAtLimit_ReturnsBadRequest()
    {
        // Arrange
        await using var application = new NikoNikoApiTestApplication();
        var (user, client, _) = await application.CreateUserAndClient("Limited User");
        
        // Create 2 teams first (the limit is 2)
        await application.CreateTeam("Team 1", user.Id);
        await application.CreateTeam("Team 2", user.Id);
        
        var createTeamDto = new CreateTeamDto { Name = "Third Team" };

        // Act
        var response = await client.PostAsJsonAsync("/api/teams", createTeamDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var message = await response.Content.ReadAsStringAsync();
        Assert.Contains("maximum number of teams", message);
    }

    [Fact]
    public async Task CreateTeam_AsSuperAdmin_AllowsBypassingLimit()
    {
        // Arrange
        await using var application = new NikoNikoApiTestApplication();
        var (user, client, _) = await application.CreateUserAndClient("Super Admin", isSuperAdmin: true);
        
        // Create 2 teams first
        await application.CreateTeam("Team 1", user.Id);
        await application.CreateTeam("Team 2", user.Id);
        
        var createTeamDto = new CreateTeamDto { Name = "Third Team" };

        // Act
        var response = await client.PostAsJsonAsync("/api/teams", createTeamDto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateTeam_AsSuperAdmin_ReturnsCreated()
    {
        // Arrange
        await using var application = new NikoNikoApiTestApplication();
        var (user, client, _) = await application.CreateUserAndClient("Super Admin", isSuperAdmin: true);
        var createTeamDto = new CreateTeamDto { Name = "My Super Team" };

        // Act
        var response = await client.PostAsJsonAsync("/api/teams", createTeamDto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var team = await response.Content.ReadFromJsonAsync<TeamDto>();
        Assert.NotNull(team);
        Assert.Equal("My Super Team", team.Name);
        Assert.Equal(user.Id, team.AdminId);
    }

    [Fact]
    public async Task GetTeams_AsSuperAdmin_ReturnsAllTeams()
    {
        // Arrange
        await using var application = new NikoNikoApiTestApplication();
        var (teamAdmin1, _, _) = await application.CreateUserAndClient("Team Admin 1");
        await application.CreateTeam("Team 1", teamAdmin1.Id);

        var (teamAdmin2, _, _) = await application.CreateUserAndClient("Team Admin 2");
        await application.CreateTeam("Team 2", teamAdmin2.Id);

        var (_, client, _) = await application.CreateUserAndClient("Super Admin", isSuperAdmin: true);

        // Act
        var response = await client.GetAsync("/api/teams");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var teams = await response.Content.ReadFromJsonAsync<List<TeamWithSprintsDto>>();
        Assert.NotNull(teams);
        Assert.Equal(2, teams.Count);
    }

    [Fact]
    public async Task GetTeams_AsRegularUser_ReturnsOnlyTeamsTheyAreMembersOf()
    {
        // Arrange
        await using var application = new NikoNikoApiTestApplication();
        // Create Team 1 with User 1 as admin
        var (user1, client1, _) = await application.CreateUserAndClient("User 1");
        var team1 = await application.CreateTeam("Team 1", user1.Id);

        // Create Team 2 with User 2 as admin
        var (user2, client2, _) = await application.CreateUserAndClient("User 2");
        var team2 = await application.CreateTeam("Team 2", user2.Id);

        // Create a regular user who will be a member of Team 1 but not Team 2
        var (regularUser, regularUserClient, _) = await application.CreateUserAndClient("Regular User");
        using (var scope = application.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.TeamUsers.Add(new Core.Models.TeamUser { TeamId = team1.Id, UserId = regularUser.Id });
            await dbContext.SaveChangesAsync();
        }

        // Act
        var response = await regularUserClient.GetAsync("/api/teams");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var teams = await response.Content.ReadFromJsonAsync<List<TeamWithSprintsDto>>();
        Assert.NotNull(teams);
        Assert.Single(teams); // Should only be a member of one team
        Assert.Equal(team1.Id, teams[0].Id);
    }

    [Fact]
    public async Task DeleteTeam_AsTeamAdmin_ReturnsNoContent()
    {
        // Arrange
        await using var application = new NikoNikoApiTestApplication();
        var (teamAdmin, client, _) = await application.CreateUserAndClient("Team Admin");
        var team = await application.CreateTeam("Test Team", teamAdmin.Id);

        // Act
        var response = await client.DeleteAsync($"/api/teams/{team.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify team is deleted
        using var scope = application.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var deletedTeam = await dbContext.Teams.FindAsync(team.Id);
        Assert.Null(deletedTeam);
    }

    [Fact]
    public async Task DeleteTeam_AsOtherUser_ReturnsForbidden()
    {
        // Arrange
        await using var application = new NikoNikoApiTestApplication();
        var (teamAdmin, _, _) = await application.CreateUserAndClient("Team Admin");
        var team = await application.CreateTeam("Test Team", teamAdmin.Id);
        var (_, client, _) = await application.CreateUserAndClient("Other User");

        // Act
        var response = await client.DeleteAsync($"/api/teams/{team.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteTeam_AsSuperAdmin_DeletesTeam()
    {
        // Arrange
        await using var application = new NikoNikoApiTestApplication();
        var (teamAdmin, _, _) = await application.CreateUserAndClient("Team Admin");
        var team = await application.CreateTeam("Test Team", teamAdmin.Id);
        var (_, client, _) = await application.CreateUserAndClient("Super Admin", isSuperAdmin: true);

        // Act
        var response = await client.DeleteAsync($"/api/teams/{team.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var scope = application.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var deletedTeam = await dbContext.Teams.FindAsync(team.Id);
        Assert.Null(deletedTeam);
    }

    [Fact]
    public async Task RemoveUserFromTeam_AsTeamAdmin_RemovesUser()
    {
        // Arrange
        await using var application = new NikoNikoApiTestApplication();
        var (teamAdmin, client, _) = await application.CreateUserAndClient("Team Admin");
        var team = await application.CreateTeam("Test Team", teamAdmin.Id);
        var (userToRemove, _, _) = await application.CreateUserAndClient("User To Remove");

        using (var scope = application.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.TeamUsers.Add(new Core.Models.TeamUser { TeamId = team.Id, UserId = userToRemove.Id });
            await dbContext.SaveChangesAsync();
        }

        // Act
        var response = await client.DeleteAsync($"/api/teams/{team.Id}/users/{userToRemove.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using (var scope = application.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var teamUser = await dbContext.TeamUsers.FirstOrDefaultAsync(tu => tu.TeamId == team.Id && tu.UserId == userToRemove.Id);
            Assert.Null(teamUser);
        }
    }
}