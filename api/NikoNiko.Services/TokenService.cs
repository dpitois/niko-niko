using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

using Microsoft.Extensions.Configuration; // Assuming IConfiguration is still needed and not moved
using Microsoft.IdentityModel.Tokens;

using NikoNiko.Core.Interfaces;
using NikoNiko.Core.Models; // Updated namespace for User

namespace NikoNiko.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _config;

    public TokenService(IConfiguration config)
    {
        _config = config;
    }

    public string CreateToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString())
        };

        if (!string.IsNullOrEmpty(user.Email))
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
        }

        if (!string.IsNullOrEmpty(user.Name))
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Name, user.Name));
        }

        if (!string.IsNullOrEmpty(user.AvatarUrl))
        {
            claims.Add(new Claim("avatar_url", user.AvatarUrl));
        }

        if (user.IsSuperAdmin)
        {
            claims.Add(new Claim("is_super_admin", "true"));
        }

        claims.Add(new Claim("is_onboarded", user.IsOnboarded ? "true" : "false"));

        if (user.TeamUsers != null)
        {
            foreach (var teamUser in user.TeamUsers)
            {
                claims.Add(new Claim("team_id", teamUser.TeamId.ToString()));
            }
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Authentication:Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(15), // Short lived token
            Issuer = _config["Authentication:Jwt:Issuer"],
            Audience = _config["Authentication:Jwt:Audience"],
            SigningCredentials = creds
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    public RefreshToken GenerateRefreshToken(string? ipAddress)
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return new RefreshToken
        {
            Token = Convert.ToBase64String(randomNumber),
            Expires = DateTime.UtcNow.AddDays(7),
            Created = DateTime.UtcNow,
            CreatedByIp = ipAddress
        };
    }

    // New method for testing flexibility
    public string GenerateToken(Claim[] claims)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Authentication:Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7),
            Issuer = _config["Authentication:Jwt:Issuer"],
            Audience = _config["Authentication:Jwt:Audience"],
            SigningCredentials = creds
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}