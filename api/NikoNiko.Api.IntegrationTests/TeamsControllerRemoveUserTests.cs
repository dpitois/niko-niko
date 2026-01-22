using System;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using NikoNiko.Core.DTOs.Mood;
using NikoNiko.Core.DTOs.Sprint;
using NikoNiko.Core.Models;
using NikoNiko.Data;

using Xunit;

namespace NikoNiko.Api.IntegrationTests;

public class TeamsControllerRemoveUserTests
{
    [Fact]
    public async Task RemoveUser_ShouldDeleteMoods_And_RemoveLink()
    {
        // Arrange
        await using var application = new NikoNikoApiTestApplication();
        var (admin, adminClient, _) = await application.CreateUserAndClient("Admin");
        var team = await application.CreateTeam("Team A", admin.Id);

        // Add Member
        var (member, _, _) = await application.CreateUserAndClient("Member");
        using (var scope = application.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.TeamUsers.Add(new TeamUser { TeamId = team.Id, UserId = member.Id });

            // Add Member to another team to prevent orphan logic
            var teamB = new Team { Name = "Team B", AdminId = admin.Id }; // Admin owns it
            db.Teams.Add(teamB);
            db.TeamUsers.Add(new TeamUser { Team = teamB, UserId = member.Id });

            await db.SaveChangesAsync();
        }

        // Create Sprint & Mood in Team A
        var createSprintDto = new CreateSprintDto
        {
            Name = "Sprint 1",
            TeamId = team.Id,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7))
        };
        var sprintResp = await adminClient.PostAsJsonAsync("/api/sprints", createSprintDto);
        var sprint = await sprintResp.Content.ReadFromJsonAsync<SprintDto>();

        // We need to inject the mood manually or use client. 
        // Using context is faster for setup.
        using (var scope = application.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.MoodEntries.Add(new MoodEntry
            {
                SprintId = sprint.Id,
                UserId = member.Id,
                Mood = MoodType.Happy,
                Date = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        // Act
        var response = await adminClient.DeleteAsync($"/api/teams/{team.Id}/users/{member.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using (var scope = application.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Check link removed
            var link = await db.TeamUsers.FirstOrDefaultAsync(tu => tu.TeamId == team.Id && tu.UserId == member.Id);
            Assert.Null(link);

            // Check moods removed
            var moods = await db.MoodEntries.Where(m => m.UserId == member.Id && m.SprintId == sprint.Id).ToListAsync();
            Assert.Empty(moods);

            // Check NOT orphan (still in Team B)
            var count = await db.TeamUsers.CountAsync(tu => tu.UserId == member.Id);
            Assert.Equal(1, count);
        }
    }

    [Fact]
    public async Task RemoveUser_Orphan_ShouldCreateDefaultTeam()
    {
        // Arrange
        await using var application = new NikoNikoApiTestApplication();
        var (admin, adminClient, _) = await application.CreateUserAndClient("Admin");
        var team = await application.CreateTeam("Team A", admin.Id);

        // Add Member (only in Team A)
        var (member, _, _) = await application.CreateUserAndClient("Member");
        using (var scope = application.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.TeamUsers.Add(new TeamUser { TeamId = team.Id, UserId = member.Id });
            await db.SaveChangesAsync();
        }

        // Act
        var response = await adminClient.DeleteAsync($"/api/teams/{team.Id}/users/{member.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using (var scope = application.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Check link removed from Team A
            var link = await db.TeamUsers.FirstOrDefaultAsync(tu => tu.TeamId == team.Id && tu.UserId == member.Id);
            Assert.Null(link);

            // Check Orphan logic triggered -> New Team Created
            var newTeamLink = await db.TeamUsers
                .Include(tu => tu.Team)
                .FirstOrDefaultAsync(tu => tu.UserId == member.Id);

            Assert.NotNull(newTeamLink);
            Assert.NotNull(newTeamLink.Team); // Ensure Team loaded
            Assert.NotEqual(team.Id, newTeamLink.TeamId);
            Assert.Equal("Member's Team", newTeamLink.Team.Name);
            Assert.Equal(member.Id, newTeamLink.Team.AdminId);
        }
    }
}