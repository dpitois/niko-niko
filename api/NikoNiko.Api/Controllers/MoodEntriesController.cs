using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using NikoNiko.Core.DTOs;
using NikoNiko.Core.DTOs.Mood;
using NikoNiko.Core.Interfaces;

namespace NikoNiko.Api.Controllers;

/// <summary>
/// Controller for managing mood entries.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MoodEntriesController : ControllerBase
{
    private readonly IMoodService _moodService;

    public MoodEntriesController(IMoodService moodService)
    {
        _moodService = moodService;
    }

    /// <summary>
    /// Gets a list of mood entries.
    /// </summary>
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

        var isSuperAdmin = User.HasClaim("is_super_admin", "true");
        var moodEntries = await _moodService.GetMoodEntriesAsync(userId, isSuperAdmin);

        if (!isSuperAdmin && !moodEntries.Any())
        {
            return StatusCode(StatusCodes.Status403Forbidden, "You are not a member of any team with mood entries.");
        }

        return Ok(moodEntries);
    }

    /// <summary>
    /// Gets the current user's mood history with pagination.
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResult<MoodEntryDto>>> GetMyMoodEntries([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized("User ID not found or invalid.");
        }

        var result = await _moodService.GetMyMoodEntriesAsync(userId, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Gets a specific mood entry by its ID.
    /// </summary>
    [HttpGet("{moodEntryId}")]
    [Authorize(Policy = "IsTeamMember")] // teamId will be resolved from sprintId
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<MoodEntryDto>> GetMoodEntry(Guid moodEntryId)
    {
        var moodEntry = await _moodService.GetMoodEntryByIdAsync(moodEntryId);
        if (moodEntry == null)
        {
            return NotFound();
        }

        return Ok(moodEntry);
    }

    /// <summary>
    /// Creates a new mood entry.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status200OK)]
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

        try
        {
            var (moodEntryDto, isCreated) = await _moodService.CreateOrUpdateMoodEntryAsync(createMoodEntryDto, authenticatedUserId);

            if (isCreated)
            {
                return CreatedAtAction(nameof(GetMoodEntry), new { moodEntryId = moodEntryDto.Id }, moodEntryDto);
            }
            else
            {
                return Ok(moodEntryDto);
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Gets mood entries for a specific sprint, optionally filtered by user ID and date.
    /// </summary>
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

        try
        {
            var moodEntries = await _moodService.GetMoodEntriesBySprintAsync(sprintId, userId, date, authenticatedUserId, isSuperAdmin);

            if (!moodEntries.Any() && (userId.HasValue || date.HasValue))
            {
                return NotFound($"No mood entries found for sprint {sprintId} with the given criteria.");
            }

            return Ok(moodEntries);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ex.Message);
        }
    }
}