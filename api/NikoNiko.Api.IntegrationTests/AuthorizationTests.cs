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
using NikoNiko.Core.DTOs.Sprint;
using NikoNiko.Core.DTOs.Team;
using NikoNiko.Core.DTOs.User;
using NikoNiko.Core.DTOs.Mood; // Added
using NikoNiko.Core.Models; // Added
using NikoNiko.Data;
using Xunit;

namespace NikoNiko.Api.IntegrationTests;

public class AuthorizationTests
{
    [Fact]
    public async Task GetUsers_AsAnonymous_ReturnsUnauthorized()
    {
        // Arrange
        await using var application = new NikoNikoApiTestApplication();
        var client = application.CreateClient();

        // Act
        var response = await client.GetAsync("/api/users");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetUsers_AsRegularUser_ReturnsOnlyTeamMembers()
    {
        // Arrange
        await using var application = new NikoNikoApiTestApplication();
        var (teamAdmin, _, _) = await application.CreateUserAndClient("Team Admin");
        var team = await application.CreateTeam("Test Team", teamAdmin.Id);

        var (user1, client, _) = await application.CreateUserAndClient("User 1");
        var (user2, _, _) = await application.CreateUserAndClient("User 2");

        using (var scope = application.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.TeamUsers.Add(new Core.Models.TeamUser { TeamId = team.Id, UserId = user1.Id });
            await dbContext.SaveChangesAsync();
        }

        // Act
        var response = await client.GetAsync("/api/users");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var users = await response.Content.ReadFromJsonAsync<List<UserDto>>();
        Assert.NotNull(users);
        Assert.Contains(users, u => u.Id == user1.Id);
        Assert.DoesNotContain(users, u => u.Id == user2.Id);
    }

    [Fact]
    public async Task GetUsers_AsSuperAdmin_ReturnsOk()
    {
        // Arrange
        await using var application = new NikoNikoApiTestApplication();
        var (_, client, _) = await application.CreateUserAndClient("Super Admin", isSuperAdmin: true);
        await application.CreateUserAndClient("Another User");

        // Act
        var response = await client.GetAsync("/api/users");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var users = await response.Content.ReadFromJsonAsync<List<UserDto>>();
        Assert.NotNull(users);
        Assert.True(users.Count >= 2);
    }

    [Fact]
    public async Task CreateTeam_AsRegularUser_ReturnsForbidden()
    {
        // Arrange
        await using var application = new NikoNikoApiTestApplication();
        var (_, client, _) = await application.CreateUserAndClient("New Team Admin");
        var createTeamDto = new CreateTeamDto { Name = "My New Team" };

        // Act
        var response = await client.PostAsJsonAsync("/api/teams", createTeamDto);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
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
    public async Task CreateSprint_AsTeamAdmin_ReturnsCreated()
    {
        // Arrange
        await using var application = new NikoNikoApiTestApplication();
        var (teamAdmin, client, _) = await application.CreateUserAndClient("Team Admin");
        var team = await application.CreateTeam("Test Team", teamAdmin.Id);

        var createSprintDto = new CreateSprintDto
        {
            Name = "New Sprint",
            TeamId = team.Id,
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(15)
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/sprints", createSprintDto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var sprint = await response.Content.ReadFromJsonAsync<SprintDto>();
        Assert.NotNull(sprint);
        Assert.Equal("New Sprint", sprint.Name);
    }

    [Fact]
    public async Task CreateSprint_AsOtherUser_ReturnsForbidden()
    {
        // Arrange
        await using var application = new NikoNikoApiTestApplication();
        var (teamAdmin, _, _) = await application.CreateUserAndClient("Team Admin");
        var team = await application.CreateTeam("Test Team", teamAdmin.Id);
        var (_, client, _) = await application.CreateUserAndClient("Other User");

        var createSprintDto = new CreateSprintDto
        {
            Name = "New Sprint",
            TeamId = team.Id,
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(15)
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/sprints", createSprintDto);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
    
    [Fact]
    public async Task GetSprint_AsTeamMember_ReturnsOk()
    {
        // Arrange
        await using var application = new NikoNikoApiTestApplication();
        var (teamAdmin, adminClient, _) = await application.CreateUserAndClient("Team Admin");
        var team = await application.CreateTeam("Test Team", teamAdmin.Id);
        var createSprintDto = new CreateSprintDto
        {
            Name = "Test Sprint",
            TeamId = team.Id,
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(15)
        };
        var (teamMember, memberClient, _) = await application.CreateUserAndClient("Team Member");

        // Add team member to the team
        using (var scope = application.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.TeamUsers.Add(new Core.Models.TeamUser { TeamId = team.Id, UserId = teamMember.Id });
            await dbContext.SaveChangesAsync();
        }

        // Create the sprint
        var createSprintResponse = await adminClient.PostAsJsonAsync("/api/sprints", createSprintDto);
        createSprintResponse.EnsureSuccessStatusCode();
        var sprint = await createSprintResponse.Content.ReadFromJsonAsync<SprintDto>();
        Assert.NotNull(sprint);

        // Act
        var getSprintResponse = await memberClient.GetAsync($"/api/sprints/{sprint.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, getSprintResponse.StatusCode);
        var fetchedSprint = await getSprintResponse.Content.ReadFromJsonAsync<SprintDto>();
        Assert.NotNull(fetchedSprint);
        Assert.Equal(sprint.Id, fetchedSprint.Id);
    }

    [Fact]
    public async Task GetSprint_AsNonTeamMember_ReturnsForbidden()
    {
        // Arrange
        await using var application = new NikoNikoApiTestApplication();
        var (teamAdmin, adminClient, _) = await application.CreateUserAndClient("Team Admin");
        var team = await application.CreateTeam("Test Team", teamAdmin.Id);
        var createSprintDto = new CreateSprintDto
        {
            Name = "Test Sprint",
            TeamId = team.Id,
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(15)
        };
        var (_, nonMemberClient, _) = await application.CreateUserAndClient("Non Team Member");

        // Create the sprint
        var createSprintResponse = await adminClient.PostAsJsonAsync("/api/sprints", createSprintDto);
        createSprintResponse.EnsureSuccessStatusCode();
        var sprint = await createSprintResponse.Content.ReadFromJsonAsync<SprintDto>();
        Assert.NotNull(sprint);

        // Act
        var getSprintResponse = await nonMemberClient.GetAsync($"/api/sprints/{sprint.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, getSprintResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteSprint_AsTeamAdmin_ReturnsNoContent()
    {
        // Arrange
        await using var application = new NikoNikoApiTestApplication();
        var (teamAdmin, adminClient, _) = await application.CreateUserAndClient("Team Admin");
        var team = await application.CreateTeam("Test Team", teamAdmin.Id);
        var createSprintDto = new CreateSprintDto
        {
            Name = "Sprint to Delete",
            TeamId = team.Id,
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(15)
        };

        var createSprintResponse = await adminClient.PostAsJsonAsync("/api/sprints", createSprintDto);
        createSprintResponse.EnsureSuccessStatusCode();
        var sprint = await createSprintResponse.Content.ReadFromJsonAsync<SprintDto>();
        Assert.NotNull(sprint);

        // Act
        var deleteResponse = await adminClient.DeleteAsync($"/api/sprints/{sprint.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // Verify sprint is deleted
        using (var scope = application.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var deletedSprint = await dbContext.Sprints.FindAsync(sprint.Id);
            Assert.Null(deletedSprint);
        }
    }

    [Fact]
    public async Task DeleteSprint_AsNonTeamAdmin_ReturnsForbidden()
    {
        // Arrange
        await using var application = new NikoNikoApiTestApplication();
        var (teamAdmin, adminClient, _) = await application.CreateUserAndClient("Team Admin");
        var team = await application.CreateTeam("Test Team", teamAdmin.Id);
        var createSprintDto = new CreateSprintDto
        {
            Name = "Sprint to Delete",
            TeamId = team.Id,
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(15)
        };

        var createSprintResponse = await adminClient.PostAsJsonAsync("/api/sprints", createSprintDto);
        createSprintResponse.EnsureSuccessStatusCode();
        var sprint = await createSprintResponse.Content.ReadFromJsonAsync<SprintDto>();
        Assert.NotNull(sprint);

        var (nonAdminUser, nonAdminClient, _) = await application.CreateUserAndClient("Non Admin User");
        // Ensure nonAdminUser is a member of the team to differentiate from Unauthorized
        using (var scope = application.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.TeamUsers.Add(new Core.Models.TeamUser { TeamId = team.Id, UserId = nonAdminUser.Id });
            await dbContext.SaveChangesAsync();
        }

        // Act
        var deleteResponse = await nonAdminClient.DeleteAsync($"/api/sprints/{sprint.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, deleteResponse.StatusCode);

        // Verify sprint is not deleted
        using (var scope = application.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var existingSprint = await dbContext.Sprints.FindAsync(sprint.Id);
            Assert.NotNull(existingSprint);
        }
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

    [Fact]
    public async Task GetMoodEntries_AsTeamMember_ReturnsOk()
    {
        // Arrange
        await using var application = new NikoNikoApiTestApplication();
        var (teamAdmin, adminClient, _) = await application.CreateUserAndClient("Team Admin");
        var team = await application.CreateTeam("Test Team", teamAdmin.Id);
        var createSprintDto = new CreateSprintDto
        {
            Name = "Test Sprint",
            TeamId = team.Id,
            StartDate = DateTime.UtcNow.Date,
            EndDate = DateTime.UtcNow.AddDays(15)
        };
        var createSprintResponse = await adminClient.PostAsJsonAsync("/api/sprints", createSprintDto);
        createSprintResponse.EnsureSuccessStatusCode();
        var sprint = await createSprintResponse.Content.ReadFromJsonAsync<SprintDto>();
        Assert.NotNull(sprint);

        var (teamMember, memberClient, _) = await application.CreateUserAndClient("Team Member");
        using (var scope = application.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.TeamUsers.Add(new Core.Models.TeamUser { TeamId = team.Id, UserId = teamMember.Id });
            await dbContext.SaveChangesAsync();
        }

        var createMoodEntryDto = new CreateMoodEntryDto
        {
            SprintId = sprint.Id,
            Date = DateTime.UtcNow.Date,
            Mood = MoodType.Happy,
            UserId = teamMember.Id 
        };
        var createMoodResponse = await memberClient.PostAsJsonAsync("/api/moodentries", createMoodEntryDto);
        createMoodResponse.EnsureSuccessStatusCode();

        // Act
        var getMoodEntriesResponse = await memberClient.GetAsync("/api/moodentries");

        // Assert
        Assert.Equal(HttpStatusCode.OK, getMoodEntriesResponse.StatusCode);
        var moodEntries = await getMoodEntriesResponse.Content.ReadFromJsonAsync<List<MoodEntryDto>>();
        Assert.NotNull(moodEntries);
        Assert.Contains(moodEntries, me => me.Id != Guid.Empty && me.Mood == MoodType.Happy && me.UserId == teamMember.Id);
    }

    [Fact]
    public async Task GetMoodEntries_AsNonTeamMember_ReturnsForbidden()
    {
        // Arrange
        await using var application = new NikoNikoApiTestApplication();
        var (teamAdmin, adminClient, _) = await application.CreateUserAndClient("Team Admin");
        var team = await application.CreateTeam("Test Team", teamAdmin.Id);
        var createSprintDto = new CreateSprintDto
        {
            Name = "Test Sprint",
            TeamId = team.Id,
            StartDate = DateTime.UtcNow.Date,
            EndDate = DateTime.UtcNow.AddDays(15)
        };
        var createSprintResponse = await adminClient.PostAsJsonAsync("/api/sprints", createSprintDto);
        createSprintResponse.EnsureSuccessStatusCode();
        var sprint = await createSprintResponse.Content.ReadFromJsonAsync<SprintDto>();
        Assert.NotNull(sprint);

        var (teamMember, memberClient, _) = await application.CreateUserAndClient("Team Member");
        using (var scope = application.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.TeamUsers.Add(new Core.Models.TeamUser { TeamId = team.Id, UserId = teamMember.Id });
            await dbContext.SaveChangesAsync();
        }

        var createMoodEntryDto = new CreateMoodEntryDto
        {
            SprintId = sprint.Id,
            Date = DateTime.UtcNow.Date,
            Mood = MoodType.Happy,
            UserId = teamMember.Id
        };
        var createMoodResponse = await memberClient.PostAsJsonAsync("/api/moodentries", createMoodEntryDto);
        createMoodResponse.EnsureSuccessStatusCode();

        var (_, nonMemberClient, _) = await application.CreateUserAndClient("Non Team Member");

        // Act
        var getMoodEntriesResponse = await nonMemberClient.GetAsync("/api/moodentries");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, getMoodEntriesResponse.StatusCode);
    }
    
    [Fact]
    public async Task SuperAdminJwt_ContainsIsSuperAdminClaim()
    {
        // Arrange
        await using var application = new NikoNikoApiTestApplication();

        // Act
        var (_, _, jwtToken) = await application.CreateUserAndClient("Super Admin", isSuperAdmin: true);
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(jwtToken);

        // Assert
        var isSuperAdminClaim = token.Claims.FirstOrDefault(c => c.Type == "is_super_admin");
        Assert.NotNull(isSuperAdminClaim);
        Assert.Equal("true", isSuperAdminClaim.Value, ignoreCase: true);
    }
}
