using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using NikoNiko.Core.Models;
using NikoNiko.Data;

using Xunit;

namespace NikoNiko.Api.IntegrationTests;

public class UsersControllerDeleteTests
{
    [Fact]
    public async Task DeleteUser_AsAdminOfTeamWithNoOtherMembers_ShouldDeleteUserAndTeam()
    {
        // Arrange
        await using var application = new NikoNikoApiTestApplication();
        var (superAdmin, client, _) = await application.CreateUserAndClient("Super Admin", isSuperAdmin: true);
        var (userToDelete, _, _) = await application.CreateUserAndClient("User To Delete");

        // Create a team where userToDelete is the admin and ONLY member
        Guid teamId;
        using (var scope = application.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var team = new Team
            {
                Id = Guid.NewGuid(),
                Name = "User's Team",
                AdminId = userToDelete.Id
            };
            var teamUser = new TeamUser { TeamId = team.Id, UserId = userToDelete.Id };

            dbContext.Teams.Add(team);
            dbContext.TeamUsers.Add(teamUser);
            await dbContext.SaveChangesAsync();
            teamId = team.Id;
        }

        // Act
        var response = await client.DeleteAsync($"/api/users/{userToDelete.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using (var scope = application.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = await dbContext.Users.FindAsync(userToDelete.Id);
            var team = await dbContext.Teams.FindAsync(teamId);

            Assert.Null(user); // User should be deleted
            Assert.Null(team); // Team should be deleted (cascade logic in controller)
        }
    }

    [Fact]
    public async Task DeleteUser_AsAdminOfTeamWithOtherMembers_ShouldReturnConflict()
    {
        // Arrange
        await using var application = new NikoNikoApiTestApplication();
        var (superAdmin, client, _) = await application.CreateUserAndClient("Super Admin", isSuperAdmin: true);
        var (userToDelete, _, _) = await application.CreateUserAndClient("User To Delete");
        var (otherUser, _, _) = await application.CreateUserAndClient("Other Member");

        // Create a team where userToDelete is admin but has other members
        Guid teamId;
        using (var scope = application.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var team = new Team
            {
                Id = Guid.NewGuid(),
                Name = "Shared Team",
                AdminId = userToDelete.Id
            };

            dbContext.Teams.Add(team);
            dbContext.TeamUsers.Add(new TeamUser { TeamId = team.Id, UserId = userToDelete.Id });
            dbContext.TeamUsers.Add(new TeamUser { TeamId = team.Id, UserId = otherUser.Id });
            await dbContext.SaveChangesAsync();
            teamId = team.Id;
        }

        // Act
        var response = await client.DeleteAsync($"/api/users/{userToDelete.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        using (var scope = application.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = await dbContext.Users.FindAsync(userToDelete.Id);
            var team = await dbContext.Teams.FindAsync(teamId);

            Assert.NotNull(user); // User should NOT be deleted
            Assert.NotNull(team); // Team should NOT be deleted
        }
    }
}