using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;

using NikoNiko.Core.DTOs.Sprint;

using Xunit;

namespace NikoNiko.Api.IntegrationTests;

public class SprintsControllerValidationTests
{
    [Fact]
    public async Task CreateSprint_WithEmptyName_ReturnsBadRequest_WithValidationErrors()
    {
        // Arrange
        await using var application = new NikoNikoApiTestApplication();
        var (teamAdmin, client, _) = await application.CreateUserAndClient("Team Admin");
        var team = await application.CreateTeam("Test Team", teamAdmin.Id);

        var createSprintDto = new CreateSprintDto
        {
            Name = "", // Invalid: Empty
            TeamId = team.Id,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(15))
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/sprints", createSprintDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problemDetails);
        Assert.True(problemDetails.Errors.ContainsKey("Name"));
        Assert.Contains("validation.required", problemDetails.Errors["Name"]);
    }

    [Fact]
    public async Task CreateSprint_WithEndDateBeforeStartDate_ReturnsBadRequest_WithValidationErrors()
    {
        // Arrange
        await using var application = new NikoNikoApiTestApplication();
        var (teamAdmin, client, _) = await application.CreateUserAndClient("Team Admin");
        var team = await application.CreateTeam("Test Team", teamAdmin.Id);

        var createSprintDto = new CreateSprintDto
        {
            Name = "Invalid Dates Sprint",
            TeamId = team.Id,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)) // Invalid: Before Start
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/sprints", createSprintDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problemDetails);
        Assert.True(problemDetails.Errors.ContainsKey("EndDate"));
        Assert.Contains("validation.endDateBeforeStartDate", problemDetails.Errors["EndDate"]);
    }
}

// Helper class to deserialize the standard RFC 7807 problem details
public class ValidationProblemDetails
{
    public required string Type { get; set; }
    public required string Title { get; set; }
    public int Status { get; set; }
    public required string TraceId { get; set; }
    public required Dictionary<string, string[]> Errors { get; set; }
}