using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
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
    }
}
