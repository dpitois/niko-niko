using System;
using System.Net;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using NikoNiko.Core.Models;
using NikoNiko.Data;

using Xunit;

namespace NikoNiko.Api.IntegrationTests;

public class UsersControllerDeleteMeTests
{
    [Fact]
    public async Task DeleteMe_ShouldDeleteCurrentUser()
    {
        // Arrange
        await using var application = new NikoNikoApiTestApplication();
        var (user, client, _) = await application.CreateUserAndClient("Self Deletor");

        // Act
        var response = await client.DeleteAsync("/api/users/me");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using (var scope = application.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var dbUser = await dbContext.Users.FindAsync(user.Id);
            Assert.Null(dbUser);
        }
    }

    [Fact]
    public async Task DeleteMe_WhenAdminOfTeamWithOtherMembers_ShouldReturnConflict()
    {
        // Arrange
        await using var application = new NikoNikoApiTestApplication();
        var (user, client, _) = await application.CreateUserAndClient("Admin User");
        var (otherUser, _, _) = await application.CreateUserAndClient("Other Member");

        using (var scope = application.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var team = new Team
            {
                Id = Guid.NewGuid(),
                Name = "Shared Team",
                AdminId = user.Id
            };

            dbContext.Teams.Add(team);
            dbContext.TeamUsers.Add(new TeamUser { TeamId = team.Id, UserId = user.Id });
            dbContext.TeamUsers.Add(new TeamUser { TeamId = team.Id, UserId = otherUser.Id });
            await dbContext.SaveChangesAsync();
        }

        // Act
        var response = await client.DeleteAsync("/api/users/me");

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task DeleteMe_WhenAdminOfTeamWithNoOtherMembers_ShouldDeleteUserAndTeam()
    {
        // Arrange
        await using var application = new NikoNikoApiTestApplication();
        var (user, client, _) = await application.CreateUserAndClient("Sole Admin");
        Guid teamId;

        using (var scope = application.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var team = new Team
            {
                Id = Guid.NewGuid(),
                Name = "Sole Team",
                AdminId = user.Id
            };
            dbContext.Teams.Add(team);
            dbContext.TeamUsers.Add(new TeamUser { TeamId = team.Id, UserId = user.Id });
            await dbContext.SaveChangesAsync();
            teamId = team.Id;
        }

        // Act
        var response = await client.DeleteAsync("/api/users/me");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using (var scope = application.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var dbUser = await dbContext.Users.FindAsync(user.Id);
            var dbTeam = await dbContext.Teams.FindAsync(teamId);
            Assert.Null(dbUser);
            Assert.Null(dbTeam);
        }
    }
}