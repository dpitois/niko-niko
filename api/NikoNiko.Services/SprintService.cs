using Microsoft.EntityFrameworkCore;

using NikoNiko.Core.DTOs.Sprint;
using NikoNiko.Core.Interfaces;
using NikoNiko.Core.Models;
using NikoNiko.Data;

namespace NikoNiko.Services;

public class SprintService : ISprintService
{
    private readonly ApplicationDbContext _context;

    public SprintService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SprintDto>> GetSprintsAsync(Guid? teamId, Guid userId, bool isSuperAdmin)
    {
        var query = _context.Sprints.AsQueryable();

        if (teamId.HasValue)
        {
            query = query.Where(s => s.TeamId == teamId.Value);
        }

        if (!isSuperAdmin)
        {
            query = query.Where(s => s.Team.AdminId == userId || s.Team.TeamUsers.Any(tu => tu.UserId == userId));
        }

        var sprints = await query.ToListAsync();

        return sprints.Select(s => new SprintDto
        {
            Id = s.Id,
            Name = s.Name,
            StartDate = DateOnly.FromDateTime(s.StartDate),
            EndDate = DateOnly.FromDateTime(s.EndDate),
            TeamId = s.TeamId
        });
    }

    public async Task<SprintDto?> GetSprintByIdAsync(Guid sprintId)
    {
        var sprint = await _context.Sprints.FindAsync(sprintId);

        if (sprint == null)
        {
            return null;
        }

        return new SprintDto
        {
            Id = sprint.Id,
            Name = sprint.Name,
            StartDate = DateOnly.FromDateTime(sprint.StartDate),
            EndDate = DateOnly.FromDateTime(sprint.EndDate),
            TeamId = sprint.TeamId
        };
    }

    public async Task<SprintDto> CreateSprintAsync(CreateSprintDto createSprintDto, Guid userId, bool isSuperAdmin)
    {
        var team = await _context.Teams.FindAsync(createSprintDto.TeamId);
        if (team == null)
        {
            throw new KeyNotFoundException("Team not found.");
        }

        var isTeamAdmin = team.AdminId == userId;

        if (!isSuperAdmin && !isTeamAdmin)
        {
            throw new UnauthorizedAccessException("Only team admins or super admins can create sprints.");
        }

        var sprint = new Sprint
        {
            Name = createSprintDto.Name,
            StartDate = createSprintDto.StartDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            EndDate = createSprintDto.EndDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            TeamId = createSprintDto.TeamId
        };

        _context.Sprints.Add(sprint);
        await _context.SaveChangesAsync();

        return new SprintDto
        {
            Id = sprint.Id,
            Name = sprint.Name,
            StartDate = DateOnly.FromDateTime(sprint.StartDate),
            EndDate = DateOnly.FromDateTime(sprint.EndDate),
            TeamId = sprint.TeamId
        };
    }

    public async Task UpdateSprintAsync(Guid sprintId, UpdateSprintDto updateSprintDto)
    {
        var sprint = await _context.Sprints
            .Include(s => s.MoodEntries)
            .FirstOrDefaultAsync(s => s.Id == sprintId);

        if (sprint == null)
        {
            throw new KeyNotFoundException("Sprint not found.");
        }

        var newStart = updateSprintDto.StartDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var newEnd = updateSprintDto.EndDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        // Validate date range against existing mood entries
        if (sprint.MoodEntries.Any())
        {
            var minMoodDate = sprint.MoodEntries.Min(m => m.Date);
            var maxMoodDate = sprint.MoodEntries.Max(m => m.Date);

            if (newStart > minMoodDate)
            {
                throw new InvalidOperationException($"Cannot set start date after {minMoodDate:yyyy-MM-dd} because there are existing mood entries.");
            }

            if (newEnd < maxMoodDate)
            {
                throw new InvalidOperationException($"Cannot set end date before {maxMoodDate:yyyy-MM-dd} because there are existing mood entries.");
            }
        }

        sprint.Name = updateSprintDto.Name;
        sprint.StartDate = newStart;
        sprint.EndDate = newEnd;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteSprintAsync(Guid sprintId)
    {
        var sprint = await _context.Sprints.FindAsync(sprintId);
        if (sprint == null)
        {
            throw new KeyNotFoundException("Sprint not found.");
        }

        _context.Sprints.Remove(sprint);
        await _context.SaveChangesAsync();
    }
}