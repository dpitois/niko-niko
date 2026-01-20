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
using Microsoft.AspNetCore.TestHost;
using Moq;

using NikoNiko.Core.DTOs.Mood;
using NikoNiko.Core.DTOs.Sprint;
using NikoNiko.Core.DTOs.Team;
using NikoNiko.Core.DTOs.User;
using NikoNiko.Core.Models;
using NikoNiko.Core.Interfaces;
using NikoNiko.Data;
using NikoNiko.Services;

using Xunit;

namespace NikoNiko.Api.IntegrationTests;

public class MoodEntriesControllerTests
{
    [Fact]
    public async Task CreateMoodEntry_SendsNotificationWithCorrectTeamId()
    {
        // Arrange
        var mockNotificationService = new Mock<INotificationService>();
        await using var application = new NikoNikoApiTestApplication();
        var client = application.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddScoped(_ => mockNotificationService.Object);
            });
        }).CreateClient();

        var (user, userClient, token) = await application.CreateUserAndClient("Notified User");
        client.DefaultRequestHeaders.Authorization = userClient.DefaultRequestHeaders.Authorization;

        var team = await application.CreateTeam("Test Team", user.Id);
        var createSprintDto = new CreateSprintDto
        {
            Name = "Test Sprint",
            TeamId = team.Id,
            StartDate = DateTime.UtcNow.Date,
            EndDate = DateTime.UtcNow.AddDays(15)
        };
        var sprintResponse = await client.PostAsJsonAsync("/api/sprints", createSprintDto);
        sprintResponse.EnsureSuccessStatusCode();
        var sprint = await sprintResponse.Content.ReadFromJsonAsync<SprintDto>();
        Assert.NotNull(sprint);

        var createMoodEntryDto = new CreateMoodEntryDto
        {
            SprintId = sprint.Id,
            Date = DateTime.UtcNow.Date,
            Mood = MoodType.Happy,
            UserId = user.Id
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/moodentries", createMoodEntryDto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        mockNotificationService.Verify(n => n.SendMoodNotificationAsync(
            It.Is<string>(s => s == user.Email),
            It.IsAny<string>(),
            It.Is<string>(s => s == user.Id.ToString()),
            It.Is<Guid>(g => g == team.Id)
        ), Times.Once);
    }

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

    [Fact]
    public async Task CreateMoodEntry_AheadOfUtc_ReturnsCreated()
    {
        // Arrange
        await using var application = new NikoNikoApiTestApplication();
        var (user, client, _) = await application.CreateUserAndClient("Tokyo User");
        var team = await application.CreateTeam("Global Team", user.Id);

        // Create sprint covering "tomorrow"
        var createSprintDto = new CreateSprintDto
        {
            Name = "Sprint 1",
            TeamId = team.Id,
            StartDate = DateTime.UtcNow.Date.AddDays(-1),
            EndDate = DateTime.UtcNow.Date.AddDays(5)
        };
        var sprintResp = await client.PostAsJsonAsync("/api/sprints", createSprintDto);
        sprintResp.EnsureSuccessStatusCode();
        var sprint = await sprintResp.Content.ReadFromJsonAsync<SprintDto>();
        Assert.NotNull(sprint);

        // Scenario: User is in a timezone ahead of UTC (e.g. UTC+24 for test simplicity)
        // They try to post a mood for "Tomorrow" (relative to UTC), which is "Today" for them.
        // Condition: entryDate <= UtcNow + Offset

        var entryDate = DateTime.UtcNow.Date.AddDays(1); // "Tomorrow" UTC
        var offsetMinutes = 24 * 60; // +24 hours offset

        var createMoodEntryDto = new CreateMoodEntryDto
        {
            SprintId = sprint.Id,
            UserId = user.Id,
            Mood = MoodType.Happy,
            Date = entryDate,
            TimezoneOffset = offsetMinutes
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/moodentries", createMoodEntryDto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateMoodEntry_BehindUtc_ReturnsBadRequest()
    {
        // Arrange
        await using var application = new NikoNikoApiTestApplication();
        var (user, client, _) = await application.CreateUserAndClient("NY User");
        var team = await application.CreateTeam("Global Team", user.Id);

        var createSprintDto = new CreateSprintDto
        {
            Name = "Sprint 1",
            TeamId = team.Id,
            StartDate = DateTime.UtcNow.Date.AddDays(-1),
            EndDate = DateTime.UtcNow.Date.AddDays(5)
        };
        var sprintResp = await client.PostAsJsonAsync("/api/sprints", createSprintDto);
        sprintResp.EnsureSuccessStatusCode();
        var sprint = await sprintResp.Content.ReadFromJsonAsync<SprintDto>();
        Assert.NotNull(sprint);

        // Scenario: User is in a timezone behind UTC (e.g. UTC-5)
        // They try to post a mood for "Tomorrow" (relative to UTC).
        // Condition: entryDate > UtcNow + Offset

        var entryDate = DateTime.UtcNow.Date.AddDays(1); // "Tomorrow" UTC
        var offsetMinutes = -300; // -5 hours offset (NY)

        var createMoodEntryDto = new CreateMoodEntryDto
        {
            SprintId = sprint.Id,
            UserId = user.Id,
            Mood = MoodType.Happy,
            Date = entryDate,
            TimezoneOffset = offsetMinutes
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/moodentries", createMoodEntryDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}