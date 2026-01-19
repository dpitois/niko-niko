using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using NikoNiko.Core.DTOs.Mood;
using NikoNiko.Core.Models;
using NikoNiko.Data;
using NikoNiko.Services;

namespace NikoNiko.Api.Controllers;

/// <summary>
/// Controller for managing mood entries.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MoodEntriesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public MoodEntriesController(ApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    /// <summary>
    /// Gets a list of mood entries.
    /// Super-admins get all mood entries. Regular users get mood entries from sprints in teams they are a member of.
    /// </summary>
    /// <returns>A list of MoodEntryDto objects.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<MoodEntryDto>>> GetMoodEntries()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized("User ID not found or invalid.");
        }

        IQueryable<Core.Models.MoodEntry> query;

        if (User.HasClaim("is_super_admin", "true"))
        {
            query = _context.MoodEntries;
        }
        else
        {
            // Get sprints for teams the current user is in
            var userSprintsIds = await _context.Sprints
                .Where(s => s.Team.AdminId == userId || s.Team.TeamUsers.Any(tu => tu.UserId == userId))
                .Select(s => s.Id)
                .ToListAsync();

            // If not a super admin and no sprints are found for the user, return Forbidden
            if (!userSprintsIds.Any())
            {
                return StatusCode(StatusCodes.Status403Forbidden, "You are not a member of any team with mood entries.");
            }

            // Get mood entries for those sprints
            query = _context.MoodEntries
                .Where(me => userSprintsIds.Contains(me.SprintId));
        }

        var moodEntries = await query
            .Select(me => new MoodEntryDto
            {
                Id = me.Id,
                UserId = me.UserId,
                SprintId = me.SprintId,
                Date = me.Date.ToUniversalTime(),
                Mood = me.Mood
            })
            .ToListAsync();

        return Ok(moodEntries);
    }

    /// <summary>
    /// Gets a specific mood entry by its ID.
    /// A user must be a member of the sprint's team, or a super-admin.
    /// </summary>
    /// <param name="moodEntryId">The ID of the mood entry.</param>
    /// <returns>The MoodEntryDto object, or NotFound if the mood entry does not exist.</returns>
    [HttpGet("{moodEntryId}")]
    [Authorize(Policy = "IsTeamMember")] // teamId will be resolved from sprintId
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<MoodEntryDto>> GetMoodEntry(Guid moodEntryId)
    {
        var moodEntry = await _context.MoodEntries
            .Select(me => new MoodEntryDto
            {
                Id = me.Id,
                UserId = me.UserId,
                SprintId = me.SprintId,
                Date = me.Date.ToUniversalTime(),
                Mood = me.Mood
            })
            .FirstOrDefaultAsync(me => me.Id == moodEntryId);

        if (moodEntry == null)
        {
            return NotFound();
        }

        return Ok(moodEntry);
    }


    /// <summary>
    /// Creates a new mood entry.
    /// A user can only create a mood entry for themselves.
    /// </summary>
    /// <param name="createMoodEntryDto">The data needed to create a mood entry.</param>
    /// <returns>The MoodEntryDto of the created mood entry.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<MoodEntryDto>> CreateMoodEntry(CreateMoodEntryDto createMoodEntryDto)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var authenticatedUserId))
        {
            return Unauthorized("User ID not found or invalid.");
        }

        if (createMoodEntryDto.UserId != authenticatedUserId)
        {
            return StatusCode(403, "You can only create mood entries for yourself.");
        }

        var sprint = await _context.Sprints.FindAsync(createMoodEntryDto.SprintId);
        if (sprint == null)
        {
            return BadRequest("Sprint not found.");
        }

        var teamId = sprint.TeamId;
        // Check strict membership (TeamUser or Team Admin)
        var isMember = await _context.TeamUsers
            .AnyAsync(tu => tu.TeamId == teamId && tu.UserId == authenticatedUserId);

        var isTeamAdmin = await _context.Teams
            .AnyAsync(t => t.Id == teamId && t.AdminId == authenticatedUserId);

        if (!isMember && !isTeamAdmin)
        {
            return StatusCode(StatusCodes.Status403Forbidden, "You must be a member of the team to submit a mood entry.");
        }

        var entryDate = createMoodEntryDto.Date?.ToUniversalTime().Date ?? DateTime.UtcNow.Date;

        // Calculate the user's local date based on the provided timezone offset.
        // We add the offset (in minutes) to UtcNow to get the user's local time.
        // For example, Tokyo (UTC+9) has an offset of +540. UtcNow + 540 minutes = Local Time.
        var userLocalNow = DateTime.UtcNow.AddMinutes(createMoodEntryDto.TimezoneOffset);

        if (entryDate > userLocalNow.Date)
        {
            return BadRequest("Mood entry date cannot be in the future (relative to your local time).");
        }

        if (entryDate < sprint.StartDate.Date)
        {
            return BadRequest("Mood entry date cannot be before the sprint start date.");
        }

        if (entryDate > sprint.EndDate.Date)
        {
            return BadRequest("Mood entry date cannot be after the sprint end date.");
        }

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

            var updatedEntryDto = new MoodEntryDto
            {
                Id = existingEntry.Id,
                UserId = existingEntry.UserId,
                SprintId = existingEntry.SprintId,
                Date = existingEntry.Date,
                Mood = existingEntry.Mood
            };
            return Ok(updatedEntryDto);
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

            var moodEntryDto = new MoodEntryDto
            {
                Id = moodEntry.Id,
                UserId = moodEntry.UserId,
                SprintId = moodEntry.SprintId,
                Date = moodEntry.Date,
                Mood = moodEntry.Mood
            };

            return CreatedAtAction(nameof(GetMoodEntry), new { moodEntryId = moodEntry.Id }, moodEntryDto);
        }
    }

    /// <summary>
    /// Gets mood entries for a specific sprint, optionally filtered by user ID and date.
    /// A user must be a member of the sprint's team, or a super-admin.
    /// </summary>
    /// <param name="sprintId">The ID of the sprint.</param>
    /// <param name="userId">Optional: The ID of the user.</param>
    /// <param name="date">Optional: The specific date for the mood entry (YYYY-MM-DD).</param>
    /// <returns>A list of MoodEntryDto objects.</returns>
    [HttpGet("bysprint/{sprintId}")]
    [Authorize(Policy = "IsTeamMember")] // Policy will check if user is member of the team associated with sprintId
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<MoodEntryDto>>> GetMoodEntriesBySprint(
        Guid sprintId,
        [FromQuery] Guid? userId,
        [FromQuery] DateTime? date)
    {
        var authenticatedUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(authenticatedUserIdString, out var authenticatedUserId))
        {
            return Unauthorized("Authenticated User ID not found or invalid.");
        }

        var isSuperAdmin = User.HasClaim("is_super_admin", "true");

        var query = _context.MoodEntries.Where(me => me.SprintId == sprintId);

        // If a specific userId is requested, ensure the requesting user has permission to see that user's moods
        if (userId.HasValue && !isSuperAdmin && userId.Value != authenticatedUserId)
        {
            // Verify if the requested userId is a member of the same team as the authenticated user
            var sprintTeamId = await _context.Sprints
                .Where(s => s.Id == sprintId)
                .Select(s => s.TeamId)
                .FirstOrDefaultAsync();

            var isRequestedUserMember = await _context.TeamUsers
                .AnyAsync(tu => tu.TeamId == sprintTeamId && tu.UserId == userId.Value);

            if (!isRequestedUserMember)
            {
                return StatusCode(403, "You can only view moods for users within your teams.");
            }
        }

        if (userId.HasValue)
        {
            query = query.Where(me => me.UserId == userId.Value);
        }

        if (date.HasValue)
        {
            var utcDate = date.Value.ToUniversalTime().Date;
            query = query.Where(me => me.Date.Date == utcDate);
        }

        var moodEntries = await query
            .Select(me => new MoodEntryDto
            {
                Id = me.Id,
                UserId = me.UserId,
                SprintId = me.SprintId,
                Date = me.Date.ToUniversalTime(),
                Mood = me.Mood
            })
            .ToListAsync();

        if (!moodEntries.Any() && (userId.HasValue || date.HasValue))
        {
            return NotFound($"No mood entries found for sprint {sprintId} with the given criteria.");
        }

        return Ok(moodEntries);
    }
}