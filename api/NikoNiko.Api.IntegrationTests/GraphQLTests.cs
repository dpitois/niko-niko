using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using NikoNiko.Core.Models;

using Xunit;

namespace NikoNiko.Api.IntegrationTests;

public class GraphQLTests : IClassFixture<NikoNikoApiTestApplication>
{
    private readonly NikoNikoApiTestApplication _factory;

    public GraphQLTests(NikoNikoApiTestApplication factory)
    {
        _factory = factory;
    }

    private async Task<JsonElement> ExecuteGraphQLQueryAsync(HttpClient client, string query, object? variables = null)
    {
        var requestBody = new
        {
            query = query,
            variables = variables
        };

        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/graphql", content);

        var responseString = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"HTTP Error {response.StatusCode}: {responseString}");
        }

        var json = JsonSerializer.Deserialize<JsonElement>(responseString);

        if (json.TryGetProperty("errors", out var errors))
        {
            throw new Exception($"GraphQL Error: {errors}");
        }

        return json.GetProperty("data");
    }

    [Fact]
    public async Task GetMe_ShouldReturnCurrentUser()
    {
        // Arrange
        var (user, client, _) = await _factory.CreateUserAndClient("GraphQL User");

        // Act
        var query = @"
            query {
                me {
                    id
                    name
                    email
                }
            }
        ";

        var data = await ExecuteGraphQLQueryAsync(client, query);

        // Assert
        Assert.Equal(user.Id, Guid.Parse(data.GetProperty("me").GetProperty("id").GetString()!));
        Assert.Equal(user.Name, data.GetProperty("me").GetProperty("name").GetString());
        Assert.Equal(user.Email, data.GetProperty("me").GetProperty("email").GetString());
    }

    [Fact]
    public async Task GetMyTeams_ShouldReturnOnlyUserTeams()
    {
        // Arrange
        var (user1, client1, _) = await _factory.CreateUserAndClient("User 1");
        var (user2, _, _) = await _factory.CreateUserAndClient("User 2");

        var team1 = await _factory.CreateTeam("Team 1", user1.Id);
        var team2 = await _factory.CreateTeam("Team 2", user2.Id); // User 1 is not in Team 2

        // Act
        var query = @"
            query {
                myTeams {
                    id
                    name
                }
            }
        ";

        var data = await ExecuteGraphQLQueryAsync(client1, query);

        // Assert
        var teams = data.GetProperty("myTeams").EnumerateArray().ToList();
        Assert.Single(teams);
        Assert.Equal(team1.Id, Guid.Parse(teams[0].GetProperty("id").GetString()!));
    }

    [Fact]
    public async Task GetTeam_ShouldReturnTeam_WhenUserIsMember()
    {
        // Arrange
        var (admin, _, _) = await _factory.CreateUserAndClient("Team Admin");
        var (member, clientMember, _) = await _factory.CreateUserAndClient("Team Member");

        var team = await _factory.CreateTeam("My Team", admin.Id);

        // Add member to team
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<Data.ApplicationDbContext>();
            db.TeamUsers.Add(new TeamUser { TeamId = team.Id, UserId = member.Id });
            await db.SaveChangesAsync();
        }

        // Act
        var query = @"
            query GetTeam($id: UUID!) {
                team(id: $id) {
                    id
                    name
                }
            }
        ";

        var data = await ExecuteGraphQLQueryAsync(clientMember, query, new { id = team.Id });

        // Assert
        Assert.Equal(team.Id, Guid.Parse(data.GetProperty("team").GetProperty("id").GetString()!));
    }

    [Fact]
    public async Task GetTeam_ShouldReturnNull_WhenUserIsNotMember()
    {
        // Arrange
        var (admin, _, _) = await _factory.CreateUserAndClient("Admin Other Team");
        var (outsider, clientOutsider, _) = await _factory.CreateUserAndClient("Outsider");

        var team = await _factory.CreateTeam("Secret Team", admin.Id);

        // Act
        var query = @"
            query GetTeam($id: UUID!) {
                team(id: $id) {
                    id
                }
            }
        ";

        var data = await ExecuteGraphQLQueryAsync(clientOutsider, query, new { id = team.Id });

        // Assert
        Assert.Equal(JsonValueKind.Null, data.GetProperty("team").ValueKind);
    }

    [Fact]
    public async Task AddMoodEntry_ShouldCreateEntry_WhenValid()
    {
        // Arrange
        var (user, client, _) = await _factory.CreateUserAndClient("Mood User");
        var team = await _factory.CreateTeam("Mood Team", user.Id);

        // Create a sprint
        Guid sprintId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<Data.ApplicationDbContext>();
            var sprint = new Sprint
            {
                Id = Guid.NewGuid(),
                Name = "Sprint 1",
                TeamId = team.Id,
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(5)
            };
            db.Sprints.Add(sprint);
            await db.SaveChangesAsync();
            sprintId = sprint.Id;
        }

        // Act
        var mutation = @"
            mutation AddMoodEntry($input: CreateMoodEntryDtoInput!) {
                addMoodEntry(input: $input) {
                    id
                    mood
                    date
                }
            }
        ";

        var variables = new
        {
            input = new
            {
                userId = user.Id,
                sprintId = sprintId,
                mood = "HAPPY",
                timezoneOffset = 0
            }
        };

        var data = await ExecuteGraphQLQueryAsync(client, mutation, variables);

        // Assert
        var entry = data.GetProperty("addMoodEntry");
        Assert.NotNull(entry.GetProperty("id").GetString());
        Assert.Equal("HAPPY", entry.GetProperty("mood").GetString());
    }

    [Fact]
    public async Task AddMoodEntry_ShouldFail_WhenDateIsInFuture()
    {
        // Arrange
        var (user, client, _) = await _factory.CreateUserAndClient("Future User");
        var team = await _factory.CreateTeam("Future Team", user.Id);

        // Create a sprint
        Guid sprintId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<Data.ApplicationDbContext>();
            var sprint = new Sprint
            {
                Id = Guid.NewGuid(),
                Name = "Sprint Future",
                TeamId = team.Id,
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(10)
            };
            db.Sprints.Add(sprint);
            await db.SaveChangesAsync();
            sprintId = sprint.Id;
        }

        // Act
        var mutation = @"
            mutation AddMoodEntry($input: CreateMoodEntryDtoInput!) {
                addMoodEntry(input: $input) {
                    id
                }
            }
        ";

        var futureDate = DateTime.UtcNow.AddDays(2);
        var variables = new
        {
            input = new
            {
                userId = user.Id,
                sprintId = sprintId,
                mood = "NEUTRAL",
                date = futureDate,
                timezoneOffset = 0
            }
        };

        // Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => ExecuteGraphQLQueryAsync(client, mutation, variables));
        Assert.Contains("future", exception.Message.ToLower());
    }

    [Fact]
    public async Task CreateTeam_ShouldCreateTeam_WhenSuperAdmin()
    {
        // Arrange
        var (superAdmin, client, _) = await _factory.CreateUserAndClient("Super Admin", isSuperAdmin: true);

        // Act
        var mutation = @"
            mutation CreateTeam($input: CreateTeamDtoInput!) {
                createTeam(input: $input) {
                    id
                    name
                }
            }
        ";

        var variables = new
        {
            input = new
            {
                name = "New Team via GraphQL"
            }
        };

        var data = await ExecuteGraphQLQueryAsync(client, mutation, variables);

        // Assert
        Assert.Equal("New Team via GraphQL", data.GetProperty("createTeam").GetProperty("name").GetString());
    }

    [Fact]
    public async Task CreateSprint_ShouldCreateSprint_WhenTeamAdmin()
    {
        // Arrange
        var (admin, client, _) = await _factory.CreateUserAndClient("Sprint Admin");
        var team = await _factory.CreateTeam("Sprint Team", admin.Id);

        // Act
        var mutation = @"
            mutation CreateSprint($input: CreateSprintDtoInput!) {
                createSprint(input: $input) {
                    id
                    name
                }
            }
        ";

        var variables = new
        {
            input = new
            {
                name = "Sprint GraphQL",
                startDate = DateTime.UtcNow,
                endDate = DateTime.UtcNow.AddDays(7),
                teamId = team.Id
            }
        };

        var data = await ExecuteGraphQLQueryAsync(client, mutation, variables);

        // Assert
        Assert.Equal("Sprint GraphQL", data.GetProperty("createSprint").GetProperty("name").GetString());
    }
}