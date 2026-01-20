using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NikoNiko.Core.DTOs.Sprint;
using NikoNiko.Core.Models;
using NikoNiko.Data;
using Xunit;

namespace NikoNiko.Api.IntegrationTests;

public class SprintsControllerUpdateTests
{
    private DateTime GetUtcDate(int addDays = 0)
    {
        return DateTime.SpecifyKind(DateTime.UtcNow.Date.AddDays(addDays), DateTimeKind.Utc);
    }

    [Fact]
    public async Task UpdateSprint_WithNoMoods_ShouldSuccess()
    {
        // Arrange
        await using var application = new NikoNikoApiTestApplication();
        var (teamAdmin, client, _) = await application.CreateUserAndClient("Team Admin");
        var team = await application.CreateTeam("Test Team", teamAdmin.Id);

        var createSprintDto = new CreateSprintDto
        {
            Name = "Sprint 1",
            TeamId = team.Id,
            StartDate = GetUtcDate(),
            EndDate = GetUtcDate(10)
        };
        var createResponse = await client.PostAsJsonAsync("/api/sprints", createSprintDto);
        var sprint = await createResponse.Content.ReadFromJsonAsync<SprintDto>();
        Assert.NotNull(sprint);

        var updateDto = new UpdateSprintDto
        {
            Name = "Sprint 1 Updated",
            StartDate = sprint.StartDate.AddDays(1),
            EndDate = sprint.EndDate.AddDays(-1)
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/sprints/{sprint.Id}", updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        
        using var scope = application.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var updatedSprint = await dbContext.Sprints.FindAsync(sprint.Id);
        Assert.NotNull(updatedSprint);
        
        Assert.Equal("Sprint 1 Updated", updatedSprint.Name);
        Assert.Equal(updateDto.StartDate.ToUniversalTime(), updatedSprint.StartDate);
        Assert.Equal(updateDto.EndDate.ToUniversalTime(), updatedSprint.EndDate);
    }

    [Fact]
    public async Task UpdateSprint_WithMoods_ValidDates_ShouldSuccess()
    {
        // Arrange
        await using var application = new NikoNikoApiTestApplication();
        var (teamAdmin, client, _) = await application.CreateUserAndClient("Team Admin");
        var team = await application.CreateTeam("Test Team", teamAdmin.Id);

        var startDate = GetUtcDate();
        var endDate = GetUtcDate(10);
        
        var createSprintDto = new CreateSprintDto
        {
            Name = "Sprint 1",
            TeamId = team.Id,
            StartDate = startDate,
            EndDate = endDate
        };
        var createResponse = await client.PostAsJsonAsync("/api/sprints", createSprintDto);
        var sprint = await createResponse.Content.ReadFromJsonAsync<SprintDto>();
        Assert.NotNull(sprint);

        // Add a mood entry in the middle
        var moodDate = startDate.AddDays(5);
        using (var scope = application.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.MoodEntries.Add(new MoodEntry
            {
                SprintId = sprint.Id,
                UserId = teamAdmin.Id,
                Date = moodDate,
                Mood = MoodType.Happy
            });
            await dbContext.SaveChangesAsync();
        }

        // Try to shrink range but include the mood date
        var updateDto = new UpdateSprintDto
        {
            Name = "Sprint 1 Updated",
            StartDate = moodDate,
            EndDate = moodDate.AddDays(1) // Must be > StartDate
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/sprints/{sprint.Id}", updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task UpdateSprint_WithMoods_InvalidStartDate_ShouldFail()
    {
        // Arrange
        await using var application = new NikoNikoApiTestApplication();
        var (teamAdmin, client, _) = await application.CreateUserAndClient("Team Admin");
        var team = await application.CreateTeam("Test Team", teamAdmin.Id);

        var startDate = GetUtcDate();
        var endDate = GetUtcDate(10);
        
        var createSprintDto = new CreateSprintDto
        {
            Name = "Sprint 1",
            TeamId = team.Id,
            StartDate = startDate,
            EndDate = endDate
        };
        var createResponse = await client.PostAsJsonAsync("/api/sprints", createSprintDto);
        var sprint = await createResponse.Content.ReadFromJsonAsync<SprintDto>();
        Assert.NotNull(sprint);

        // Add a mood entry
        var moodDate = startDate.AddDays(2);
        using (var scope = application.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.MoodEntries.Add(new MoodEntry
            {
                SprintId = sprint.Id,
                UserId = teamAdmin.Id,
                Date = moodDate,
                Mood = MoodType.Happy
            });
            await dbContext.SaveChangesAsync();
        }

        // Try to set start date AFTER the mood date
        var updateDto = new UpdateSprintDto
        {
            Name = "Sprint 1 Updated",
            StartDate = moodDate.AddDays(1), // Invalid
            EndDate = endDate
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/sprints/{sprint.Id}", updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateSprint_WithMoods_InvalidEndDate_ShouldFail()
    {
        // Arrange
        await using var application = new NikoNikoApiTestApplication();
        var (teamAdmin, client, _) = await application.CreateUserAndClient("Team Admin");
        var team = await application.CreateTeam("Test Team", teamAdmin.Id);

        var startDate = GetUtcDate();
        var endDate = GetUtcDate(10);
        
        var createSprintDto = new CreateSprintDto
        {
            Name = "Sprint 1",
            TeamId = team.Id,
            StartDate = startDate,
            EndDate = endDate
        };
        var createResponse = await client.PostAsJsonAsync("/api/sprints", createSprintDto);
        var sprint = await createResponse.Content.ReadFromJsonAsync<SprintDto>();
        Assert.NotNull(sprint);

        // Add a mood entry
        var moodDate = startDate.AddDays(8);
        using (var scope = application.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.MoodEntries.Add(new MoodEntry
            {
                SprintId = sprint.Id,
                UserId = teamAdmin.Id,
                Date = moodDate,
                Mood = MoodType.Happy
            });
            await dbContext.SaveChangesAsync();
        }

        // Try to set end date BEFORE the mood date
        var updateDto = new UpdateSprintDto
        {
            Name = "Sprint 1 Updated",
            StartDate = startDate,
            EndDate = moodDate.AddDays(-1) // Invalid
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/sprints/{sprint.Id}", updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
