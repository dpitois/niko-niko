using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using NikoNiko.Core.DTOs.User.Export;
using NikoNiko.Core.Models;
using NikoNiko.Core.Interfaces;
using NikoNiko.Data;
using NikoNiko.Services;

namespace NikoNiko.Api.IntegrationTests;

public class UsersControllerExportTests : IClassFixture<NikoNikoApiTestApplication>
{
    private readonly NikoNikoApiTestApplication _factory;

    public UsersControllerExportTests(NikoNikoApiTestApplication factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ExportData_ShouldReturnJsonFile_WithCorrectData()
    {
        // Arrange
        var client = _factory.CreateClient();

        // 1. Create a user
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        var user = new User
        {
            OAuthId = "export-test-oauth",
            Name = "Export Tester",
            Email = "export@test.com",
            Provider = "GitHub",
            CreatedAt = DateTime.UtcNow
        };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        // 2. Create a team and add user
        var team = new Team
        {
            Name = "Export Team",
            AdminId = user.Id
        };
        dbContext.Teams.Add(team);
        await dbContext.SaveChangesAsync();

        dbContext.TeamUsers.Add(new TeamUser { UserId = user.Id, TeamId = team.Id });
        await dbContext.SaveChangesAsync();

        // 3. Create a sprint and mood entry
        var sprint = new Sprint
        {
            Name = "Sprint Export",
            StartDate = DateTime.UtcNow.AddDays(-5),
            EndDate = DateTime.UtcNow.AddDays(5),
            TeamId = team.Id
        };
        dbContext.Sprints.Add(sprint);
        await dbContext.SaveChangesAsync();

        var moodEntry = new MoodEntry
        {
            UserId = user.Id,
            SprintId = sprint.Id,
            Mood = MoodType.Happy,
            Date = DateTime.UtcNow.AddDays(-1)
        };
        dbContext.MoodEntries.Add(moodEntry);
        await dbContext.SaveChangesAsync();

        // Generate Token
        var token = tokenService.CreateToken(user);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync("/api/users/me/export");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var content = await response.Content.ReadAsStringAsync();
        var exportDto = JsonSerializer.Deserialize<UserExportDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(exportDto);
        
        // Identity Check
        Assert.Equal(user.Name, exportDto.Identity.Username);
        
        // Team Check
        Assert.Single(exportDto.Teams);
        var exportedTeam = exportDto.Teams.First();
        Assert.Equal(team.Name, exportedTeam.Name);
        Assert.NotEqual(team.Id.ToString(), exportedTeam.Id); // ID should be hashed
        
        // Mood Check
        Assert.Single(exportDto.History);
        var exportedMood = exportDto.History.First();
        Assert.Equal(MoodType.Happy.ToString(), exportedMood.Mood);
        Assert.Equal(exportedTeam.Id, exportedMood.TeamId); // Team IDs should match between sections
    }
}
