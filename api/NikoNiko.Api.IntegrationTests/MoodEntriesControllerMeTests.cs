using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using NikoNiko.Core.DTOs;
using NikoNiko.Core.DTOs.Mood;
using NikoNiko.Core.Models;
using NikoNiko.Data;

using Xunit;

namespace NikoNiko.Api.IntegrationTests;

public class MoodEntriesControllerMeTests
{
    [Fact]
    public async Task GetMyMoodEntries_ShouldReturnPaginatedResults()
    {
        // Arrange
        await using var application = new NikoNikoApiTestApplication();
        var (user, client, _) = await application.CreateUserAndClient("Mood History User");

        // Create a team and sprint to associate moods with
        Guid sprintId;
        using (var scope = application.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var team = new Team { Id = Guid.NewGuid(), Name = "Team A", AdminId = user.Id };
            var sprint = new Sprint
            {
                Id = Guid.NewGuid(),
                TeamId = team.Id,
                Name = "Sprint 1",
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow.AddDays(30)
            };

            dbContext.Teams.Add(team);
            dbContext.Sprints.Add(sprint);

            // Create 25 mood entries
            for (int i = 0; i < 25; i++)
            {
                dbContext.MoodEntries.Add(new MoodEntry
                {
                    UserId = user.Id,
                    SprintId = sprint.Id,
                    Date = DateTime.UtcNow.AddDays(-i),
                    Mood = MoodType.Happy
                });
            }

            await dbContext.SaveChangesAsync();
            sprintId = sprint.Id;
        }

        // Act - Page 1 (Size 10)
        var responsePage1 = await client.GetAsync("/api/moodentries/me?page=1&pageSize=10");
        var resultPage1 = await responsePage1.Content.ReadFromJsonAsync<PagedResult<MoodEntryDto>>();

        // Act - Page 2 (Size 10)
        var responsePage2 = await client.GetAsync("/api/moodentries/me?page=2&pageSize=10");
        var resultPage2 = await responsePage2.Content.ReadFromJsonAsync<PagedResult<MoodEntryDto>>();

        // Act - Page 3 (Size 10) - Should have 5 items
        var responsePage3 = await client.GetAsync("/api/moodentries/me?page=3&pageSize=10");
        var resultPage3 = await responsePage3.Content.ReadFromJsonAsync<PagedResult<MoodEntryDto>>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, responsePage1.StatusCode);
        Assert.NotNull(resultPage1);
        Assert.Equal(25, resultPage1.TotalCount);
        Assert.Equal(10, resultPage1.Items.Count());
        Assert.Equal(1, resultPage1.Page);

        Assert.Equal(HttpStatusCode.OK, responsePage2.StatusCode);
        Assert.NotNull(resultPage2);
        Assert.Equal(25, resultPage2.TotalCount);
        Assert.Equal(10, resultPage2.Items.Count());
        Assert.Equal(2, resultPage2.Page);

        Assert.Equal(HttpStatusCode.OK, responsePage3.StatusCode);
        Assert.NotNull(resultPage3);
        Assert.Equal(25, resultPage3.TotalCount);
        Assert.Equal(5, resultPage3.Items.Count());
        Assert.Equal(3, resultPage3.Page);
    }
}