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
using NikoNiko.Core.DTOs.Mood;
using NikoNiko.Core.Models;
using NikoNiko.Data;
using Xunit;

namespace NikoNiko.Api.IntegrationTests;

public class UsersControllerTests
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
}
