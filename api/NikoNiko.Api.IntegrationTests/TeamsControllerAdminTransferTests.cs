using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NikoNiko.Core.DTOs.Team;
using NikoNiko.Data;
using Xunit;

namespace NikoNiko.Api.IntegrationTests;

public class TeamsControllerAdminTransferTests
{
    [Fact]
    public async Task UpdateTeamAdmin_AsTeamAdmin_UpdatesAdmin()
    {
        // Arrange
        await using var application = new NikoNikoApiTestApplication();
        var (currentAdmin, client, _) = await application.CreateUserAndClient("Current Admin");
        var (newAdmin, _, _) = await application.CreateUserAndClient("New Admin");
        var team = await application.CreateTeam("Test Team", currentAdmin.Id);

        // Add new admin as a member first
        using (var scope = application.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.TeamUsers.Add(new Core.Models.TeamUser { TeamId = team.Id, UserId = newAdmin.Id });
            await dbContext.SaveChangesAsync();
        }

        var updateDto = new UpdateTeamAdminDto { NewAdminId = newAdmin.Id };

        // Act
        var response = await client.PutAsJsonAsync($"/api/teams/{team.Id}/admin", updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using (var scope = application.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var updatedTeam = await dbContext.Teams.Include(t => t.TeamUsers).FirstOrDefaultAsync(t => t.Id == team.Id);
            
            Assert.NotNull(updatedTeam);
            Assert.Equal(newAdmin.Id, updatedTeam.AdminId);
            
            // Verify old admin is still a member
            Assert.Contains(updatedTeam.TeamUsers, tu => tu.UserId == currentAdmin.Id);
            // Verify new admin is a member
            Assert.Contains(updatedTeam.TeamUsers, tu => tu.UserId == newAdmin.Id);
        }
    }

    [Fact]
    public async Task UpdateTeamAdmin_AsSuperAdmin_UpdatesAdmin()
    {
        // Arrange
        await using var application = new NikoNikoApiTestApplication();
        var (currentAdmin, _, _) = await application.CreateUserAndClient("Current Admin");
        var (newAdmin, _, _) = await application.CreateUserAndClient("New Admin");
        var (_, superClient, _) = await application.CreateUserAndClient("Super Admin", isSuperAdmin: true);
        
        var team = await application.CreateTeam("Test Team", currentAdmin.Id);

        // Add new admin as a member first
        using (var scope = application.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.TeamUsers.Add(new Core.Models.TeamUser { TeamId = team.Id, UserId = newAdmin.Id });
            await dbContext.SaveChangesAsync();
        }

        var updateDto = new UpdateTeamAdminDto { NewAdminId = newAdmin.Id };

        // Act
        var response = await superClient.PutAsJsonAsync($"/api/teams/{team.Id}/admin", updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using (var scope = application.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var updatedTeam = await dbContext.Teams.FindAsync(team.Id);
            Assert.Equal(newAdmin.Id, updatedTeam.AdminId);
        }
    }

    [Fact]
    public async Task UpdateTeamAdmin_AsNonAdmin_ReturnsForbidden()
    {
        // Arrange
        await using var application = new NikoNikoApiTestApplication();
        var (currentAdmin, _, _) = await application.CreateUserAndClient("Current Admin");
        var (otherUser, otherClient, _) = await application.CreateUserAndClient("Other User");
        var team = await application.CreateTeam("Test Team", currentAdmin.Id);

        // Add other user as member
        using (var scope = application.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.TeamUsers.Add(new Core.Models.TeamUser { TeamId = team.Id, UserId = otherUser.Id });
            await dbContext.SaveChangesAsync();
        }

        var updateDto = new UpdateTeamAdminDto { NewAdminId = otherUser.Id };

        // Act
        var response = await otherClient.PutAsJsonAsync($"/api/teams/{team.Id}/admin", updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTeamAdmin_WithNonMember_ReturnsBadRequest()
    {
        // Arrange
        await using var application = new NikoNikoApiTestApplication();
        var (currentAdmin, client, _) = await application.CreateUserAndClient("Current Admin");
        var (nonMember, _, _) = await application.CreateUserAndClient("Non Member");
        var team = await application.CreateTeam("Test Team", currentAdmin.Id);

        var updateDto = new UpdateTeamAdminDto { NewAdminId = nonMember.Id };

        // Act
        var response = await client.PutAsJsonAsync($"/api/teams/{team.Id}/admin", updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
