using System.IdentityModel.Tokens.Jwt;

using Microsoft.Extensions.Configuration;

using NikoNiko.Core.Interfaces;
using NikoNiko.Core.Models;
using NikoNiko.Services;

using Xunit;

namespace NikoNiko.Api.IntegrationTests;

public class TokenServiceTests
{
    private readonly IConfiguration _configuration;
    private readonly TokenService _tokenService;

    public TokenServiceTests()
    {
        var inMemorySettings = new Dictionary<string, string> {
            {"Authentication:Jwt:Key", "a_very_long_and_secure_secret_key_for_testing_1234567890"},
            {"Authentication:Jwt:Issuer", "NikoNiko.Api.Test"},
            {"Authentication:Jwt:Audience", "NikoNiko.App.Test"},
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();

        _tokenService = new TokenService(_configuration);
    }

    [Fact]
    public void CreateToken_ShouldNotThrow_WhenEmailAndNameAreNull()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            OAuthId = "discord-123",
            Email = null,
            Name = null!, // Name is [Required] in model but we test the service robustness
            AvatarUrl = null
        };

        // Act
        var token = _tokenService.CreateToken(user);

        // Assert
        Assert.NotNull(token);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        Assert.Equal(user.Id.ToString(), jwtToken.Subject);
        Assert.DoesNotContain(jwtToken.Claims, c => c.Type == JwtRegisteredClaimNames.Email);
        Assert.DoesNotContain(jwtToken.Claims, c => c.Type == JwtRegisteredClaimNames.Name);
    }

    [Fact]
    public void CreateToken_ShouldIncludeClaims_WhenDataIsPresent()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            OAuthId = "google-456",
            Email = "test@example.com",
            Name = "Test User",
            AvatarUrl = "http://avatar.com/img.png"
        };

        // Act
        var token = _tokenService.CreateToken(user);

        // Assert
        Assert.NotNull(token);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        Assert.Equal(user.Id.ToString(), jwtToken.Subject);
        Assert.Equal(user.Email, jwtToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value);
        Assert.Equal(user.Name, jwtToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Name).Value);
        Assert.Equal(user.AvatarUrl, jwtToken.Claims.First(c => c.Type == "avatar_url").Value);
        Assert.Equal("false", jwtToken.Claims.First(c => c.Type == "is_onboarded").Value);
    }

    [Fact]
    public void CreateToken_ShouldIncludeTeamIdClaims_WhenUserHasTeams()
    {
        // Arrange
        var teamId1 = Guid.NewGuid();
        var teamId2 = Guid.NewGuid();
        var user = new User
        {
            Id = Guid.NewGuid(),
            OAuthId = "github-789",
            Email = "test@example.com",
            Name = "Team User",
            TeamUsers = new List<TeamUser>
            {
                new TeamUser { TeamId = teamId1 },
                new TeamUser { TeamId = teamId2 }
            }
        };

        // Act
        var token = _tokenService.CreateToken(user);

        // Assert
        Assert.NotNull(token);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var teamIdClaims = jwtToken.Claims.Where(c => c.Type == "team_id").Select(c => c.Value).ToList();
        Assert.Equal(2, teamIdClaims.Count);
        Assert.Contains(teamId1.ToString(), teamIdClaims);
        Assert.Contains(teamId2.ToString(), teamIdClaims);
    }
}