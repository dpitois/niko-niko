using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using NikoNiko.Core.Interfaces;
using NikoNiko.Core.DTOs.User.Export;
using NikoNiko.Data;

namespace NikoNiko.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;

    public UserService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserExportDto?> GetExportDataAsync(Guid userId)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return null;
        }

        // 1. Fetch Teams (Id, Name, MemberCount)
        var teamsData = await _context.Teams
            .AsNoTracking()
            .Where(t => t.TeamUsers.Any(tu => tu.UserId == userId))
            .Select(t => new
            {
                t.Id,
                t.Name,
                MemberCount = t.TeamUsers.Count
            })
            .ToListAsync();

        // 2. Fetch Mood History
        var moodHistory = await _context.MoodEntries
            .AsNoTracking()
            .Include(m => m.Sprint)
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.Date)
            .Select(m => new
            {
                m.Date,
                m.Mood,
                TeamId = m.Sprint.TeamId
            })
            .ToListAsync();

        // 3. Assemble DTO
        return new UserExportDto
        {
            Identity = new IdentityExportDto
            {
                OpenId = user.OAuthId,
                Username = user.Name,
                Provider = user.Provider,
                JoinedAt = user.CreatedAt
            },
            Teams = teamsData.Select(t => new TeamExportDto
            {
                Id = NormalizeId(t.Id),
                Name = t.Name,
                MemberCount = t.MemberCount
            }).ToList(),
            History = moodHistory.Select(m => new MoodExportDto
            {
                Date = m.Date,
                Mood = m.Mood.ToString(),
                TeamId = NormalizeId(m.TeamId)
            }).ToList()
        };
    }

    private static string NormalizeId(Guid id)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(id.ToString()));
        // Take first 8 bytes and convert to hex to keep it short but stable
        return BitConverter.ToString(hashBytes, 0, 8).Replace("-", "").ToLowerInvariant();
    }
}
