using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NikoNiko.Core.DTOs.Sprint;
using NikoNiko.Core.Models;
using NikoNiko.Data;
using NikoNiko.Services;
using Xunit;

namespace NikoNiko.Api.IntegrationTests;

public class RolesAndPermissionsTests
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
    public async Task GetUsers_AsRegularUser_ReturnsForbidden()
    {
        // Arrange
        await using var application = new NikoNikoApiTestApplication();

        // 1. Setup user
        var regularUser = new User { Id = Guid.NewGuid(), Name = "Regular User", OAuthId = "github|regular", Email = "regular@example.com", IsSuperAdmin = false };
        using (var scope = application.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Users.Add(regularUser);
            await dbContext.SaveChangesAsync();
        }

        // 2. Create authenticated client
        var client = application.CreateClient();
        using (var scope = application.Services.CreateScope())
        {
            var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, regularUser.Id.ToString()) };
            var jwtToken = tokenService.GenerateToken(claims);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
        }

        // Act
        var response = await client.GetAsync("/api/users");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetUsers_AsSuperAdmin_ReturnsOk()
    {
        // Arrange
        await using var application = new NikoNikoApiTestApplication();

        // 1. Setup user
        var superAdminUser = new User { Id = Guid.NewGuid(), Name = "Super Admin", OAuthId = "github|superadmin", Email = "superadmin@example.com", IsSuperAdmin = true };
        using (var scope = application.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Users.Add(superAdminUser);
            await dbContext.SaveChangesAsync();
        }

        // 2. Create authenticated client
        var client = application.CreateClient();
        using (var scope = application.Services.CreateScope())
        {
            var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, superAdminUser.Id.ToString()),
                new Claim("is_super_admin", "true")
            };
            var jwtToken = tokenService.GenerateToken(claims);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
        }

        // Act
        var response = await client.GetAsync("/api/users");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task DeleteTeam_AsTeamAdmin_ReturnsNoContent()
    {
        // Arrange
        await using var application = new NikoNikoApiTestApplication();
        var client = application.CreateClient();

        // 1. Setup user and team
        var teamAdmin = new User { Id = Guid.NewGuid(), Name = "Team Admin", OAuthId = "github|teamadmin", Email = "teamadmin@example.com" };
        var team = new Team { Id = Guid.NewGuid(), Name = "Test Team", AdminId = teamAdmin.Id };
        using (var scope = application.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Users.Add(teamAdmin);
            dbContext.Teams.Add(team);
            await dbContext.SaveChangesAsync();
        }

        // 2. Create authenticated client for team admin
        using (var scope = application.Services.CreateScope())
        {
            var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, teamAdmin.Id.ToString()) };
            var jwtToken = tokenService.GenerateToken(claims);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
        }

        // Act
        var response = await client.DeleteAsync($"/api/teams/{team.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify team is deleted
        using (var scope = application.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var deletedTeam = await dbContext.Teams.FindAsync(team.Id);
            Assert.Null(deletedTeam);
        }
    }

    [Fact]
    public async Task DeleteTeam_AsOtherUser_ReturnsForbidden()
    {
        // Arrange
        await using var application = new NikoNikoApiTestApplication();
        var client = application.CreateClient();

        // 1. Setup user and team
        var teamAdmin = new User { Id = Guid.NewGuid(), Name = "Team Admin", OAuthId = "github|teamadmin", Email = "teamadmin@example.com" };
        var otherUser = new User { Id = Guid.NewGuid(), Name = "Other User", OAuthId = "github|other", Email = "other@example.com" };
        var team = new Team { Id = Guid.NewGuid(), Name = "Test Team", AdminId = teamAdmin.Id };
        using (var scope = application.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Users.AddRange(teamAdmin, otherUser);
            dbContext.Teams.Add(team);
            await dbContext.SaveChangesAsync();
        }

        // 2. Create authenticated client for the other user
        using (var scope = application.Services.CreateScope())
        {
            var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, otherUser.Id.ToString()) };
            var jwtToken = tokenService.GenerateToken(claims);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
        }

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
        var client = application.CreateClient();

        // 1. Setup users and team
        var teamAdmin = new User { Id = Guid.NewGuid(), Name = "Team Admin", OAuthId = "github|teamadmin", Email = "teamadmin@example.com" };
        var superAdmin = new User { Id = Guid.NewGuid(), Name = "Super Admin", OAuthId = "github|superadmin", Email = "superadmin@example.com", IsSuperAdmin = true };
        var team = new Team { Id = Guid.NewGuid(), Name = "Test Team", AdminId = teamAdmin.Id };
        using (var scope = application.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Users.AddRange(teamAdmin, superAdmin);
            dbContext.Teams.Add(team);
            await dbContext.SaveChangesAsync();
        }

        // 2. Create authenticated client for super admin
        using (var scope = application.Services.CreateScope())
        {
            var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, superAdmin.Id.ToString()),
                new Claim("is_super_admin", "true")
            };
            var jwtToken = tokenService.GenerateToken(claims);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
        }

        // Act
        var response = await client.DeleteAsync($"/api/teams/{team.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify team is deleted
        using (var scope = application.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var deletedTeam = await dbContext.Teams.FindAsync(team.Id);
            Assert.Null(deletedTeam);
        }
    }

    [Fact]
    public async Task CreateSprint_AsTeamAdmin_ReturnsCreated()
    {
        // Arrange
        await using var application = new NikoNikoApiTestApplication();
        var client = application.CreateClient();

        var teamAdmin = new User { Id = Guid.NewGuid(), Name = "Team Admin", OAuthId = "github|teamadmin", Email = "teamadmin@example.com" };
        var team = new Team { Id = Guid.NewGuid(), Name = "Test Team", AdminId = teamAdmin.Id };
        using (var scope = application.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Users.Add(teamAdmin);
            dbContext.Teams.Add(team);
            await dbContext.SaveChangesAsync();
        }
        
        using (var scope = application.Services.CreateScope())
        {
            var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, teamAdmin.Id.ToString()) };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenService.GenerateToken(claims));
        }

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
        var client = application.CreateClient();

        var teamAdmin = new User { Id = Guid.NewGuid(), Name = "Team Admin", OAuthId = "github|teamadmin", Email = "teamadmin@example.com" };
        var otherUser = new User { Id = Guid.NewGuid(), Name = "Other User", OAuthId = "github|other", Email = "other@example.com" };
        var team = new Team { Id = Guid.NewGuid(), Name = "Test Team", AdminId = teamAdmin.Id };
        using (var scope = application.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Users.AddRange(teamAdmin, otherUser);
            dbContext.Teams.Add(team);
            await dbContext.SaveChangesAsync();
        }

        using (var scope = application.Services.CreateScope())
        {
            var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, otherUser.Id.ToString()) };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenService.GenerateToken(claims));
        }

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
}

