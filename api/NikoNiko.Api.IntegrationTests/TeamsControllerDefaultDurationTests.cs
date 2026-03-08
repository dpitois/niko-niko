using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using NikoNiko.Core.DTOs.Team;
using NikoNiko.Data;

using Xunit;

namespace NikoNiko.Api.IntegrationTests;

public class TeamsControllerDefaultDurationTests
{
    [Fact]
    public async Task UpdateTeam_WithDefaultSprintDuration_PersistsDuration()
    {
        // Arrange
        await using var application = new NikoNikoApiTestApplication();
        var (teamAdmin, client, _) = await application.CreateUserAndClient("Team Admin");
        var team = await application.CreateTeam("Test Team", teamAdmin.Id);

        var updateTeamDto = new UpdateTeamDto
        {
            Name = "Updated Team Name",
            DefaultSprintDuration = 14
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/teams/{team.Id}", updateTeamDto);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var scope = application.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var updatedTeam = await dbContext.Teams.FindAsync(team.Id);
        Assert.NotNull(updatedTeam);
        Assert.Equal("Updated Team Name", updatedTeam.Name);
        Assert.Equal(14, updatedTeam.DefaultSprintDuration);
    }

    [Fact]
    public async Task UpdateTeam_WithNullDefaultSprintDuration_PersistsNull()
    {
        // Arrange
        await using var application = new NikoNikoApiTestApplication();
        var (teamAdmin, client, _) = await application.CreateUserAndClient("Team Admin");
        var team = await application.CreateTeam("Test Team", teamAdmin.Id);

        // First set it to something
        using (var scope = application.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var t = await dbContext.Teams.FindAsync(team.Id);
            Assert.NotNull(t);
            t.DefaultSprintDuration = 14;
            await dbContext.SaveChangesAsync();
        }

        var updateTeamDto = new UpdateTeamDto
        {
            Name = "Updated Team Name",
            DefaultSprintDuration = null
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/teams/{team.Id}", updateTeamDto);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var assertScope = application.Services.CreateScope();
        var assertDbContext = assertScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var updatedTeam = await assertDbContext.Teams.FindAsync(team.Id);
        Assert.NotNull(updatedTeam);
        Assert.Null(updatedTeam.DefaultSprintDuration);
    }

    [Fact]
    public async Task CreateTeam_WithDefaultSprintDuration_PersistsDuration()
    {
        // Arrange
        await using var application = new NikoNikoApiTestApplication();
        var (user, client, _) = await application.CreateUserAndClient("Super Admin", isSuperAdmin: true);

        var createTeamDto = new CreateTeamDto
        {
            Name = "New Team",
            DefaultSprintDuration = 10
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/teams", createTeamDto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var teamDto = await response.Content.ReadFromJsonAsync<TeamDto>();
        Assert.NotNull(teamDto);
        Assert.Equal(10, teamDto.DefaultSprintDuration);

        using var assertScope = application.Services.CreateScope();
        var assertDbContext = assertScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var createdTeam = await assertDbContext.Teams.FindAsync(teamDto.Id);
        Assert.NotNull(createdTeam);
        Assert.Equal(10, createdTeam.DefaultSprintDuration);
    }

    [Fact]
    public async Task UpdateTeam_WithInvalidDuration_ReturnsBadRequest()
    {
        // Arrange
        await using var application = new NikoNikoApiTestApplication();
        var (teamAdmin, client, _) = await application.CreateUserAndClient("Team Admin");
        var team = await application.CreateTeam("Test Team", teamAdmin.Id);

        var updateTeamDto = new UpdateTeamDto
        {
            Name = "Updated Team Name",
            DefaultSprintDuration = -5
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/teams/{team.Id}", updateTeamDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTeam_WithDurationTooLong_ReturnsBadRequest()
    {
        // Arrange
        await using var application = new NikoNikoApiTestApplication();
        var (teamAdmin, client, _) = await application.CreateUserAndClient("Team Admin");
        var team = await application.CreateTeam("Test Team", teamAdmin.Id);

        var updateTeamDto = new UpdateTeamDto
        {
            Name = "Updated Team Name",
            DefaultSprintDuration = 63 // Too long
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/teams/{team.Id}", updateTeamDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}