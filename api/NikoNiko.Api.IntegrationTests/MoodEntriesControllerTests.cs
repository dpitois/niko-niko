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

public class MoodEntriesControllerTests
{
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
}
