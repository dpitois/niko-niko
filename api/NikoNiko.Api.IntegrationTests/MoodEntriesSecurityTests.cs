using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using NikoNiko.Core.DTOs.Mood;
using NikoNiko.Core.DTOs.Sprint;
using NikoNiko.Core.DTOs.Team;
using NikoNiko.Core.DTOs.User;
using NikoNiko.Core.Models;
using NikoNiko.Data;

using Xunit;

namespace NikoNiko.Api.IntegrationTests;

public class MoodEntriesSecurityTests
{
    [Fact]
    public async Task SuperAdmin_PostingToNonMemberTeam_ShouldReturnForbidden()
    {
        // This test verifies that we FIXED the security gap.

        // Arrange
        await using var application = new NikoNikoApiTestApplication();

        // 1. Create a Team and a Sprint managed by a regular user
        var (teamAdmin, adminClient, _) = await application.CreateUserAndClient("Team Admin");
        var team = await application.CreateTeam("Test Team", teamAdmin.Id);
        var createSprintDto = new CreateSprintDto
        {
            Name = "Test Sprint",
            TeamId = team.Id,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(15))
        };
        var createSprintResponse = await adminClient.PostAsJsonAsync("/api/sprints", createSprintDto);
        createSprintResponse.EnsureSuccessStatusCode();
        var sprint = await createSprintResponse.Content.ReadFromJsonAsync<SprintDto>();
        Assert.NotNull(sprint);

        // 2. Create a Super Admin who is NOT a member of the team
        var (superAdmin, superAdminClient, _) = await application.CreateUserAndClient("Super Admin", isSuperAdmin: true);

        // 3. Try to post a mood entry as Super Admin for themselves in that sprint
        var createMoodEntryDto = new CreateMoodEntryDto
        {
            SprintId = sprint.Id,
            Date = DateTime.UtcNow.Date,
            Mood = MoodType.Happy,
            UserId = superAdmin.Id
        };

        // Act
        var response = await superAdminClient.PostAsJsonAsync("/api/moodentries", createMoodEntryDto);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task SuperAdmin_ReadingNonMemberTeamMoods_ShouldReturnOk()
    {
        // This test ensures that Super Admin REMAINS able to READ all moods.

        // Arrange
        await using var application = new NikoNikoApiTestApplication();

        // 1. Create a Team and a Sprint managed by a regular user
        var (teamAdmin, adminClient, _) = await application.CreateUserAndClient("Team Admin");
        var team = await application.CreateTeam("Test Team", teamAdmin.Id);

        var createSprintDto = new CreateSprintDto
        {
            Name = "Test Sprint",
            TeamId = team.Id,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(15))
        };
        var createSprintResponse = await adminClient.PostAsJsonAsync("/api/sprints", createSprintDto);
        createSprintResponse.EnsureSuccessStatusCode();
        var sprint = await createSprintResponse.Content.ReadFromJsonAsync<SprintDto>();
        Assert.NotNull(sprint);

        // 2. Add a member and post a mood
        var (member, memberClient, _) = await application.CreateUserAndClient("Member");
        using (var scope = application.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.TeamUsers.Add(new TeamUser { TeamId = team.Id, UserId = member.Id });
            await dbContext.SaveChangesAsync();
        }

        var createMoodEntryDto = new CreateMoodEntryDto
        {
            SprintId = sprint.Id,
            Date = DateTime.UtcNow.Date,
            Mood = MoodType.Happy,
            UserId = member.Id
        };
        var moodResp = await memberClient.PostAsJsonAsync("/api/moodentries", createMoodEntryDto);
        moodResp.EnsureSuccessStatusCode();

        // 3. Create a Super Admin who is NOT a member of the team
        var (_, superAdminClient, _) = await application.CreateUserAndClient("Super Admin", isSuperAdmin: true);

        // Act
        var response = await superAdminClient.GetAsync($"/api/moodentries/bysprint/{sprint.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var moods = await response.Content.ReadFromJsonAsync<List<MoodEntryDto>>();
        Assert.NotNull(moods);
        Assert.NotEmpty(moods);
    }
}