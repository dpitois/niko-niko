using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using NikoNiko.Core.DTOs.Mood; // Updated using directive
using NikoNiko.Core.Models; // Updated using directive
using NikoNiko.Data; // Updated using directive
using NikoNiko.Services; // Updated using directive

namespace NikoNiko.Api.Controllers;

/// <summary>
/// Controller for managing mood entries.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class MoodEntriesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService; // Inject INotificationService

    public MoodEntriesController(ApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService; // Assign injected service
    }

    /// <summary>
    /// Gets a list of all mood entries.
    /// </summary>
    /// <returns>A list of MoodEntryDto objects.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<MoodEntryDto>>> GetMoodEntries()
    {
        var moodEntries = await _context.MoodEntries
            .Select(me => new MoodEntryDto
            {
                Id = me.Id,
                UserId = me.UserId,
                SprintId = me.SprintId,
                Date = me.Date.ToUniversalTime(), // Convert to UTC
                Mood = me.Mood
            })
            .ToListAsync();

        return Ok(moodEntries);
    }

    /// <summary>
    /// Gets a specific mood entry by its ID.
    /// </summary>
    /// <param name="id">The ID of the mood entry.</param>
    /// <returns>The MoodEntryDto object, or NotFound if the mood entry does not exist.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MoodEntryDto>> GetMoodEntry(Guid id)
    {
        var moodEntry = await _context.MoodEntries
            .Select(me => new MoodEntryDto
            {
                Id = me.Id,
                UserId = me.UserId,
                SprintId = me.SprintId,
                Date = me.Date.ToUniversalTime(), // Convert to UTC
                Mood = me.Mood
            })
            .FirstOrDefaultAsync(me => me.Id == id);

        if (moodEntry == null)
        {
            return NotFound();
        }

        return Ok(moodEntry);
    }


    /// <summary>
    /// Creates a new mood entry.
    /// </summary>
    /// <param name="createMoodEntryDto">The data needed to create a mood entry.</param>
    /// <returns>The MoodEntryDto of the created mood entry.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MoodEntryDto>> CreateMoodEntry(CreateMoodEntryDto createMoodEntryDto)
    {
        var sprint = await _context.Sprints.FindAsync(createMoodEntryDto.SprintId);
        if (sprint == null)
        {
            return BadRequest("Sprint not found.");
        }

        var entryDate = createMoodEntryDto.Date?.ToUniversalTime().Date ?? DateTime.UtcNow.Date;

        if (entryDate > DateTime.UtcNow.Date)
        {
            return BadRequest("Mood entry date cannot be in the future.");
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

            await _notificationService.SendMoodNotificationAsync(userEmail, notificationMessage, createMoodEntryDto.UserId.ToString());

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

            await _notificationService.SendMoodNotificationAsync(userEmail, notificationMessage, createMoodEntryDto.UserId.ToString());

            var moodEntryDto = new MoodEntryDto
            {
                Id = moodEntry.Id,
                UserId = moodEntry.UserId,
                SprintId = moodEntry.SprintId,
                Date = moodEntry.Date,
                Mood = moodEntry.Mood
            };

            return CreatedAtAction(nameof(GetMoodEntry), new { id = moodEntry.Id }, moodEntryDto);
        }
    }

    /// <summary>
    /// Gets mood entries for a specific sprint, optionally filtered by user ID and date.
    /// </summary>
    /// <param name="sprintId">The ID of the sprint.</param>
    /// <param name="userId">Optional: The ID of the user.</param>
    /// <param name="date">Optional: The specific date for the mood entry (YYYY-MM-DD).</param>
    /// <returns>A list of MoodEntryDto objects.</returns>
    [HttpGet("bysprint/{sprintId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<MoodEntryDto>>> GetMoodEntriesBySprint(
        Guid sprintId,
        [FromQuery] Guid? userId,
        [FromQuery] DateTime? date)
    {
        var query = _context.MoodEntries.Where(me => me.SprintId == sprintId);

        if (userId.HasValue)
        {
            query = query.Where(me => me.UserId == userId.Value);
        }

        if (date.HasValue)
        {
            // Convert the incoming date parameter to UTC for comparison
            var utcDate = date.Value.ToUniversalTime().Date; // .Date to compare only the date part
            query = query.Where(me => me.Date.Date == utcDate);
        }

        var moodEntries = await query
            .Select(me => new MoodEntryDto
            {
                Id = me.Id,
                UserId = me.UserId,
                SprintId = me.SprintId,
                Date = me.Date.ToUniversalTime(), // Convert to UTC
                Mood = me.Mood
            })
            .ToListAsync();

        if (!moodEntries.Any() && (userId.HasValue || date.HasValue))
        {
            // If specific filters are applied and no entries found, return NotFound
            // If just by sprintId, return empty list
            return NotFound($"No mood entries found for sprint {sprintId} with the given criteria.");
        }

        return Ok(moodEntries);
    }
}