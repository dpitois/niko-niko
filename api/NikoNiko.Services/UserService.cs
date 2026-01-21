using System.Security.Cryptography;
using System.Text;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using NikoNiko.Core.DTOs.User;
using NikoNiko.Core.DTOs.User.Export;
using NikoNiko.Core.Interfaces;
using NikoNiko.Core.Models;
using NikoNiko.Data;

namespace NikoNiko.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserService> _logger;

    public UserService(ApplicationDbContext context, ILogger<UserService> logger)
    {
        _context = context;
        _logger = logger;
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

    public async Task<IEnumerable<UserDto>> GetUsersAsync(Guid userId, bool isSuperAdmin)
    {
        IQueryable<User> query;

        if (isSuperAdmin)
        {
            query = _context.Users;
        }
        else
        {
            var userTeamIds = await _context.TeamUsers
                .Where(tu => tu.UserId == userId)
                .Select(tu => tu.TeamId)
                .ToListAsync();

            query = _context.Users
                .Where(u => u.TeamUsers.Any(tu => userTeamIds.Contains(tu.TeamId)));
        }

        return await query
            .Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email,
                Name = u.Name,
                AvatarUrl = u.AvatarUrl,
                Provider = u.Provider,
                CreatedAt = u.CreatedAt
            })
            .Distinct()
            .ToListAsync();
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid userId)
    {
        return await _context.Users
            .Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email,
                Name = u.Name,
                AvatarUrl = u.AvatarUrl,
                Provider = u.Provider,
                CreatedAt = u.CreatedAt
            })
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task DeleteUserAsync(Guid userId, Guid authenticatedUserId, bool isSuperAdmin)
    {
        // Safety check: Cannot delete self via this method if not 'me' endpoint (controller handles 'me' vs 'id' route logic, but service needs safety)
        // Actually, logic is: SuperAdmin deletes OTHER user. User deletes SELF.
        // If isSuperAdmin is true, userId can be anything EXCEPT authenticatedUserId (handled by controller usually, but safe to check here).

        if (isSuperAdmin && userId == authenticatedUserId)
        {
            throw new ArgumentException("You cannot delete your own account via administrative action.");
        }

        // If not super admin, user can ONLY delete themselves.
        if (!isSuperAdmin && userId != authenticatedUserId)
        {
            throw new UnauthorizedAccessException("You are not authorized to delete this user.");
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        var teamsAsAdmin = await _context.Teams
            .Include(t => t.TeamUsers)
            .Where(t => t.AdminId == userId)
            .ToListAsync();

        if (teamsAsAdmin.Any())
        {
            foreach (var team in teamsAsAdmin)
            {
                var otherMembersCount = team.TeamUsers.Count(tu => tu.UserId != userId);

                if (otherMembersCount > 0)
                {
                    throw new InvalidOperationException($"Cannot delete user because they are the admin of team '{team.Name}' which has other members. Please transfer ownership or remove members first.");
                }

                _context.Teams.Remove(team);
            }
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
    }

    public async Task<User?> SubmitConsentAsync(Guid userId, UserConsentDto consentDto)
    {
        var user = await _context.Users
            .Include(u => u.TeamUsers)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return null;
        }

        user.IsOnboarded = true;
        user.ConsentAt = DateTime.UtcNow;
        user.ConsentVersion = consentDto.ConsentVersion;

        await _context.SaveChangesAsync();
        return user;
    }

    private static string NormalizeId(Guid id)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(id.ToString()));
        return BitConverter.ToString(hashBytes, 0, 8).Replace("-", "").ToLowerInvariant();
    }
}