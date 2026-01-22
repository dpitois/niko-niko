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

public class SprintsControllerTests
{
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
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(15))
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
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(15))
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
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(15))
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
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(15))
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
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(15))
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
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(15))
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
}