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

public class AuthControllerTests
{
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

    [Fact]
    public async Task SignIn_NewUser_NoInvitation_CreatesTeam()
    {
        // Arrange
        await using var application = new NikoNikoApiTestApplication();
        var client = application.CreateDefaultClient();

        // Act
        var response = await client.GetAsync("/api/auth/signin-github");

        // Assert
        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        
        using (var scope = application.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            var user = await dbContext.Users
                .Include(u => u.TeamUsers)
                .FirstOrDefaultAsync(u => u.Email == "newtestuser@example.com");
                
            Assert.NotNull(user);
            var team = await dbContext.Teams.FirstOrDefaultAsync(t => t.AdminId == user.Id);
            Assert.NotNull(team);
            Assert.Equal("New Test User's Team", team.Name);
            Assert.Contains(user.TeamUsers, tu => tu.TeamId == team.Id);
        }
    }

    [Fact]
    public async Task SignIn_NewUser_WithInvitation_NoTeamCreated()
    {
        // Arrange
        await using var application = new NikoNikoApiTestApplication();
        var client = application.CreateDefaultClient();
        
        // Pre-create a team and an invitation
        using (var scope = application.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var admin = new User { Id = Guid.NewGuid(), Name = "Admin", Email = "admin@example.com", OAuthId = "github|admin" };
            var team = new Team { Id = Guid.NewGuid(), Name = "Existing Team", AdminId = admin.Id };
            var invitation = new TeamInvitation 
            { 
                Id = Guid.NewGuid(), 
                TeamId = team.Id, 
                Token = "test-token", 
                ExpirationDate = DateTime.UtcNow.AddDays(1),
                CreatorUserId = admin.Id,
                Status = "Pending"
            };
            dbContext.Users.Add(admin);
            dbContext.Teams.Add(team);
            dbContext.TeamInvitations.Add(invitation);
            await dbContext.SaveChangesAsync();
        }

        // Act
        var response = await client.GetAsync("/api/auth/signin-github?invitationToken=test-token");

        // Assert
        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        
        using (var scope = application.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = await dbContext.Users
                .Include(u => u.TeamUsers)
                .ThenInclude(tu => tu.Team)
                .FirstOrDefaultAsync(u => u.Email == "newtestuser@example.com");
                
            Assert.NotNull(user);
            // User should be member of Existing Team
            Assert.Contains(user.TeamUsers, tu => tu.Team != null && tu.Team.Name == "Existing Team");
            
            // User should NOT be admin of any team
            var userAdminTeams = await dbContext.Teams.Where(t => t.AdminId == user.Id).ToListAsync();
            Assert.Empty(userAdminTeams);
        }
    }

    [Fact]
    public async Task SignIn_ExistingUser_NoNewTeam()
    {
        // Arrange
        await using var application = new NikoNikoApiTestApplication();
        var client = application.CreateDefaultClient();
        
        using (var scope = application.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var existingUser = new User 
            { 
                Id = Guid.NewGuid(), 
                Name = "New Test User", 
                Email = "newtestuser@example.com", 
                OAuthId = "github|newtestuser" 
            };
            dbContext.Users.Add(existingUser);
            await dbContext.SaveChangesAsync();
        }
        
        int initialTeamCount;
        using (var scope = application.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            initialTeamCount = await dbContext.Teams.CountAsync();
        }

        // Act
        var response = await client.GetAsync("/api/auth/signin-github");

        // Assert
        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        
        using (var scope = application.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var finalTeamCount = await dbContext.Teams.CountAsync();
            Assert.Equal(initialTeamCount, finalTeamCount);
        }
    }
}
