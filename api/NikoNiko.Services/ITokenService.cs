using System.Security.Claims; // Add this using directive

using NikoNiko.Core.Models;

namespace NikoNiko.Services;

public interface ITokenService
{
    string CreateToken(User user);
    RefreshToken GenerateRefreshToken(string? ipAddress);
    string GenerateToken(Claim[] claims); // New method for test flexibility
}