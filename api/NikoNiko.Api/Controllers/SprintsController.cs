using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using NikoNiko.Core.DTOs.Sprint;
using NikoNiko.Core.Interfaces;

namespace NikoNiko.Api.Controllers;

/// <summary>
/// Controller for managing sprints.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SprintsController : ControllerBase
{
    private readonly ISprintService _sprintService;

    public SprintsController(ISprintService sprintService)
    {
        _sprintService = sprintService;
    }

    /// <summary>
    /// Gets a list of all sprints, optionally filtered by teamId.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<SprintDto>>> GetSprints([FromQuery] Guid? teamId)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized("User ID not found or invalid.");
        }

        var isSuperAdmin = User.HasClaim("is_super_admin", "true");
        var sprints = await _sprintService.GetSprintsAsync(teamId, userId, isSuperAdmin);
        return Ok(sprints);
    }

    /// <summary>
    /// Gets a specific sprint by its ID.
    /// </summary>
    [HttpGet("{sprintId}")]
    [Authorize(Policy = "IsTeamMember")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SprintDto>> GetSprint(Guid sprintId)
    {
        var sprint = await _sprintService.GetSprintByIdAsync(sprintId);
        if (sprint == null)
        {
            return NotFound();
        }

        return Ok(sprint);
    }

    /// <summary>
    /// Creates a new sprint.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SprintDto>> CreateSprint(CreateSprintDto createSprintDto)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized("User ID not found or invalid.");
        }

        var isSuperAdmin = User.HasClaim("is_super_admin", "true");

        try
        {
            var sprintDto = await _sprintService.CreateSprintAsync(createSprintDto, userId, isSuperAdmin);
            return CreatedAtAction(nameof(GetSprint), new { sprintId = sprintDto.Id }, sprintDto);
        }
        catch (KeyNotFoundException ex)
        {
            ModelState.AddModelError(nameof(createSprintDto.TeamId), ex.Message);
            return BadRequest(ModelState);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(nameof(createSprintDto.EndDate), ex.Message);
            return BadRequest(ModelState);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Updates a sprint.
    /// </summary>
    [HttpPut("{sprintId}")]
    [Authorize(Policy = "IsTeamAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSprint(Guid sprintId, UpdateSprintDto updateSprintDto)
    {
        try
        {
            await _sprintService.UpdateSprintAsync(sprintId, updateSprintDto);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(nameof(updateSprintDto.EndDate), ex.Message);
            return BadRequest(ModelState);
        }
        catch (InvalidOperationException ex)
        {
            // Map specific validation errors to ModelState for consistency with original controller
            if (ex.Message.Contains("start date"))
            {
                ModelState.AddModelError(nameof(updateSprintDto.StartDate), ex.Message);
            }
            else if (ex.Message.Contains("end date"))
            {
                ModelState.AddModelError(nameof(updateSprintDto.EndDate), ex.Message);
            }
            else
            {
                return BadRequest(ex.Message);
            }
            return BadRequest(ModelState);
        }
    }

    /// <summary>
    /// Deletes a sprint.
    /// </summary>
    [HttpDelete("{sprintId}")]
    [Authorize(Policy = "IsTeamAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSprint(Guid sprintId)
    {
        try
        {
            await _sprintService.DeleteSprintAsync(sprintId);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}