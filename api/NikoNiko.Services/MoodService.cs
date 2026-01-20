using Microsoft.EntityFrameworkCore;
using NikoNiko.Core.DTOs;
using NikoNiko.Core.DTOs.Mood;
using NikoNiko.Core.Interfaces;
using NikoNiko.Core.Models;
using NikoNiko.Data;

namespace NikoNiko.Services;

public class MoodService : IMoodService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public MoodService(ApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<IEnumerable<MoodEntryDto>> GetMoodEntriesAsync(Guid userId, bool isSuperAdmin)
    {
        IQueryable<MoodEntry> query;

        if (isSuperAdmin)
        {
            query = _context.MoodEntries;
        }
        else
        {
            var userSprintsIds = await _context.Sprints
                .Where(s => s.Team.AdminId == userId || s.Team.TeamUsers.Any(tu => tu.UserId == userId))
                .Select(s => s.Id)
                .ToListAsync();

            if (!userSprintsIds.Any())
            {
                return Enumerable.Empty<MoodEntryDto>();
            }

            query = _context.MoodEntries.Where(me => userSprintsIds.Contains(me.SprintId));
        }

        return await query
            .Select(me => new MoodEntryDto
            {
                Id = me.Id,
                UserId = me.UserId,
                SprintId = me.SprintId,
                Date = me.Date.ToUniversalTime(),
                Mood = me.Mood
            })
            .ToListAsync();
    }

    public async Task<PagedResult<MoodEntryDto>> GetMyMoodEntriesAsync(Guid userId, int page, int pageSize)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        var query = _context.MoodEntries.Where(me => me.UserId == userId);
        var totalCount = await query.CountAsync();

        var moodEntries = await query
            .OrderByDescending(me => me.Date)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(me => new MoodEntryDto
            {
                Id = me.Id,
                UserId = me.UserId,
                SprintId = me.SprintId,
                Date = me.Date.ToUniversalTime(),
                Mood = me.Mood
            })
            .ToListAsync();

        return new PagedResult<MoodEntryDto>
        {
            Items = moodEntries,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<MoodEntryDto?> GetMoodEntryByIdAsync(Guid moodEntryId)
    {
        return await _context.MoodEntries
            .Select(me => new MoodEntryDto
            {
                Id = me.Id,
                UserId = me.UserId,
                SprintId = me.SprintId,
                Date = me.Date.ToUniversalTime(),
                Mood = me.Mood
            })
            .FirstOrDefaultAsync(me => me.Id == moodEntryId);
    }

    public async Task<(MoodEntryDto dto, bool isCreated)> CreateOrUpdateMoodEntryAsync(CreateMoodEntryDto createMoodEntryDto, Guid authenticatedUserId)
    {
        if (createMoodEntryDto.UserId != authenticatedUserId)
        {
            throw new UnauthorizedAccessException("You can only create mood entries for yourself.");
        }

        var sprint = await _context.Sprints.FindAsync(createMoodEntryDto.SprintId);
        if (sprint == null) throw new KeyNotFoundException("Sprint not found.");

        var teamId = sprint.TeamId;
        var isMember = await _context.TeamUsers.AnyAsync(tu => tu.TeamId == teamId && tu.UserId == authenticatedUserId)
                    || await _context.Teams.AnyAsync(t => t.Id == teamId && t.AdminId == authenticatedUserId);

        if (!isMember)
        {
            throw new UnauthorizedAccessException("You must be a member of the team to submit a mood entry.");
        }

        var entryDate = createMoodEntryDto.Date?.ToUniversalTime().Date ?? DateTime.UtcNow.Date;
        var userLocalNow = DateTime.UtcNow.AddMinutes(createMoodEntryDto.TimezoneOffset);

        if (entryDate > userLocalNow.Date) throw new ArgumentException("Mood entry date cannot be in the future (relative to your local time).");
        if (entryDate < sprint.StartDate.Date) throw new ArgumentException("Mood entry date cannot be before the sprint start date.");
        if (entryDate > sprint.EndDate.Date) throw new ArgumentException("Mood entry date cannot be after the sprint end date.");

        var existingEntry = await _context.MoodEntries.FirstOrDefaultAsync(me =>
            me.UserId == createMoodEntryDto.UserId &&
            me.SprintId == createMoodEntryDto.SprintId &&
            me.Date.Date == entryDate);

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == createMoodEntryDto.UserId);
        var userEmail = user?.Email ?? "Unknown User";
        var notificationMessage = $"L'utilisateur {userEmail} vient de renseigner son humeur!";

        if (existingEntry != null)
        {
            existingEntry.Mood = createMoodEntryDto.Mood;
            _context.MoodEntries.Update(existingEntry);
            await _context.SaveChangesAsync();
            await _notificationService.SendMoodNotificationAsync(userEmail, notificationMessage, createMoodEntryDto.UserId.ToString(), teamId);

            return (new MoodEntryDto
            {
                Id = existingEntry.Id,
                UserId = existingEntry.UserId,
                SprintId = existingEntry.SprintId,
                Date = existingEntry.Date,
                Mood = existingEntry.Mood
            }, false);
        }
        else
        {
            var moodEntry = new MoodEntry
            {
                UserId = createMoodEntryDto.UserId,
                SprintId = createMoodEntryDto.SprintId,
                Mood = createMoodEntryDto.Mood,
                Date = entryDate
            };

            _context.MoodEntries.Add(moodEntry);
            await _context.SaveChangesAsync();
            await _notificationService.SendMoodNotificationAsync(userEmail, notificationMessage, createMoodEntryDto.UserId.ToString(), teamId);

            return (new MoodEntryDto
            {
                Id = moodEntry.Id,
                UserId = moodEntry.UserId,
                SprintId = moodEntry.SprintId,
                Date = moodEntry.Date,
                Mood = moodEntry.Mood
            }, true);
        }
    }

    public async Task<IEnumerable<MoodEntryDto>> GetMoodEntriesBySprintAsync(Guid sprintId, Guid? userId, DateTime? date, Guid authenticatedUserId, bool isSuperAdmin)
    {
        var query = _context.MoodEntries.Where(me => me.SprintId == sprintId);

        if (userId.HasValue && !isSuperAdmin && userId.Value != authenticatedUserId)
        {
            var sprintTeamId = await _context.Sprints
                .Where(s => s.Id == sprintId)
                .Select(s => s.TeamId)
                .FirstOrDefaultAsync();

            var isRequestedUserMember = await _context.TeamUsers
                .AnyAsync(tu => tu.TeamId == sprintTeamId && tu.UserId == userId.Value);

            if (!isRequestedUserMember)
            {
                throw new UnauthorizedAccessException("You can only view moods for users within your teams.");
            }
        }

        if (userId.HasValue) query = query.Where(me => me.UserId == userId.Value);
        if (date.HasValue)
        {
            var utcDate = date.Value.ToUniversalTime().Date;
            query = query.Where(me => me.Date.Date == utcDate);
        }

        return await query
            .Select(me => new MoodEntryDto
            {
                Id = me.Id,
                UserId = me.UserId,
                SprintId = me.SprintId,
                Date = me.Date.ToUniversalTime(),
                Mood = me.Mood
            })
            .ToListAsync();
    }
}
