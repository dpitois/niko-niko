using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using NikoNiko.Core.DTOs.Sprint;
using NikoNiko.Core.DTOs.Team;
using NikoNiko.Core.DTOs.User;
using NikoNiko.Core.Interfaces;
using NikoNiko.Core.Models;
using NikoNiko.Data;

namespace NikoNiko.Services;

public class TeamService : ITeamService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TeamService> _logger;
    private readonly INotificationService _notificationService;

    public TeamService(ApplicationDbContext context, ILogger<TeamService> logger, INotificationService notificationService)
    {
        _context = context;
        _logger = logger;
        _notificationService = notificationService;
    }

    public async Task<IEnumerable<TeamWithSprintsDto>> GetTeamsAsync(Guid userId, bool isSuperAdmin)
    {
        IQueryable<Team> baseQuery = _context.Teams
            .Include(t => t.Admin)
            .Include(t => t.Sprints)
            .Include(t => t.TeamUsers)
            .ThenInclude(tu => tu.User);

        if (!isSuperAdmin)
        {
            baseQuery = baseQuery.Where(t => t.AdminId == userId || t.TeamUsers.Any(tu => tu.UserId == userId));
        }

        return await baseQuery.Select(t => new TeamWithSprintsDto
        {
            Id = t.Id,
            Name = t.Name,
            AdminId = t.AdminId,
            AdminName = t.Admin.Name ?? t.Admin.Email,
            CreatedAt = t.CreatedAt,
            DefaultSprintDuration = t.DefaultSprintDuration,
            Sprints = t.Sprints.Select(s => new SprintDto
            {
                Id = s.Id,
                Name = s.Name,
                StartDate = DateOnly.FromDateTime(s.StartDate),
                EndDate = DateOnly.FromDateTime(s.EndDate),
                TeamId = s.TeamId
            }).ToList(),
            Members = t.TeamUsers.Select(tu => new UserDto
            {
                Id = tu.User.Id,
                Email = tu.User.Email,
                Name = tu.User.Name,
                AvatarUrl = tu.User.AvatarUrl,
                CreatedAt = tu.User.CreatedAt
            }).ToList()
        }).ToListAsync();
    }

    public async Task<TeamWithSprintsDto?> GetTeamByIdAsync(Guid teamId)
    {
        return await _context.Teams
            .Include(t => t.Admin)
            .Include(t => t.Sprints)
            .Include(t => t.TeamUsers)
            .ThenInclude(tu => tu.User)
            .Select(t => new TeamWithSprintsDto
            {
                Id = t.Id,
                Name = t.Name,
                AdminId = t.AdminId,
                AdminName = t.Admin.Name ?? t.Admin.Email,
                CreatedAt = t.CreatedAt,
                DefaultSprintDuration = t.DefaultSprintDuration,
                Sprints = t.Sprints.Select(s => new SprintDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    StartDate = DateOnly.FromDateTime(s.StartDate),
                    EndDate = DateOnly.FromDateTime(s.EndDate),
                    TeamId = s.TeamId
                }).ToList(),
                Members = t.TeamUsers.Select(tu => new UserDto
                {
                    Id = tu.User.Id,
                    Email = tu.User.Email,
                    Name = tu.User.Name,
                    AvatarUrl = tu.User.AvatarUrl,
                    CreatedAt = tu.User.CreatedAt
                }).ToList()
            })
            .FirstOrDefaultAsync(t => t.Id == teamId);
    }

    public async Task<TeamDto> CreateTeamAsync(CreateTeamDto createTeamDto, Guid adminId)
    {
        var team = new Team
        {
            Name = createTeamDto.Name,
            AdminId = adminId,
            DefaultSprintDuration = createTeamDto.DefaultSprintDuration
        };

        var teamUser = new TeamUser
        {
            Team = team,
            UserId = adminId
        };

        _context.Teams.Add(team);
        _context.TeamUsers.Add(teamUser);
        await _context.SaveChangesAsync();

        await _context.Entry(team).Reference(t => t.Admin).LoadAsync();

        return new TeamDto
        {
            Id = team.Id,
            Name = team.Name,
            AdminId = team.AdminId,
            AdminName = team.Admin.Name ?? team.Admin.Email,
            CreatedAt = team.CreatedAt,
            DefaultSprintDuration = team.DefaultSprintDuration
        };
    }

    public async Task UpdateTeamAsync(Guid teamId, UpdateTeamDto updateTeamDto)
    {
        var team = await _context.Teams.FindAsync(teamId);
        if (team == null) throw new KeyNotFoundException("Team not found.");

        team.Name = updateTeamDto.Name;
        team.DefaultSprintDuration = updateTeamDto.DefaultSprintDuration;
        await _context.SaveChangesAsync();
        await _notificationService.NotifyTeamRenamedAsync(team.Id, team.Name);
    }

    public async Task TransferAdminAsync(Guid teamId, Guid newAdminId)
    {
        var team = await _context.Teams
            .Include(t => t.TeamUsers)
            .FirstOrDefaultAsync(t => t.Id == teamId);

        if (team == null) throw new KeyNotFoundException("Team not found.");

        if (!team.TeamUsers.Any(tu => tu.UserId == newAdminId))
        {
            throw new InvalidOperationException("The new administrator must be a member of the team.");
        }

        var oldAdminId = team.AdminId;
        team.AdminId = newAdminId;

        if (!team.TeamUsers.Any(tu => tu.UserId == oldAdminId))
        {
            _context.TeamUsers.Add(new TeamUser { TeamId = teamId, UserId = oldAdminId });
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteTeamAsync(Guid teamId)
    {
        var team = await _context.Teams.FindAsync(teamId);
        if (team == null) throw new KeyNotFoundException("Team not found.");

        _context.Teams.Remove(team);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveUserFromTeamAsync(Guid teamId, Guid userId)
    {
        var team = await _context.Teams.FindAsync(teamId);
        if (team == null) throw new KeyNotFoundException("Team not found.");

        var userToRemove = await _context.Users.FindAsync(userId);
        if (userToRemove == null) throw new KeyNotFoundException("User not found.");

        if (team.AdminId == userId)
        {
            throw new InvalidOperationException("Cannot remove the team's administrator.");
        }

        var teamUser = await _context.TeamUsers
            .FirstOrDefaultAsync(tu => tu.TeamId == teamId && tu.UserId == userId);

        if (teamUser == null) throw new KeyNotFoundException("User is not a member of this team.");

        _context.TeamUsers.Remove(teamUser);

        var teamSprintIds = await _context.Sprints
            .Where(s => s.TeamId == teamId)
            .Select(s => s.Id)
            .ToListAsync();

        if (teamSprintIds.Any())
        {
            var moodsToDelete = _context.MoodEntries
                .Where(m => m.UserId == userId && teamSprintIds.Contains(m.SprintId));
            _context.MoodEntries.RemoveRange(moodsToDelete);
        }

        await _context.SaveChangesAsync();
        await _notificationService.UpdateUserGroupAsync(userId.ToString(), teamId, false);

        var remainingTeamsCount = await _context.TeamUsers.CountAsync(tu => tu.UserId == userId);
        if (remainingTeamsCount == 0)
        {
            await CreateDefaultTeamForUserAsync(userToRemove);
        }
    }

    public async Task<Team> CreateDefaultTeamForUserAsync(User user)
    {
        var teamName = $"{(string.IsNullOrWhiteSpace(user.Name) ? "My" : user.Name)}'s Team";
        var newTeam = new Team
        {
            Name = teamName,
            AdminId = user.Id
        };

        var teamUser = new TeamUser
        {
            Team = newTeam,
            UserId = user.Id
        };

        _context.Teams.Add(newTeam);
        _context.TeamUsers.Add(teamUser);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Auto-created team {TeamName} for user {UserId}.", teamName, user.Id);

        return newTeam;
    }
}