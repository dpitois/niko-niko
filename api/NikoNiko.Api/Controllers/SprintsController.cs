using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NikoNiko.Core.DTOs.Sprint;
using NikoNiko.Core.Models;
using NikoNiko.Data;

namespace NikoNiko.Api.Controllers;

/// <summary>
/// Controller for managing sprints.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SprintsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public SprintsController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets a list of all sprints, optionally filtered by teamId.
    /// A regular user can only see sprints for teams they are a member of.
    /// A super-admin can see all sprints.
    /// </summary>
    /// <returns>A list of SprintDto objects.</returns>
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

        var query = _context.Sprints.AsQueryable();

        if (teamId.HasValue)
        {
            query = query.Where(s => s.TeamId == teamId.Value);
        }

        if (!isSuperAdmin)
        {
            query = query.Where(s => s.Team.AdminId == userId || s.Team.TeamUsers.Any(tu => tu.UserId == userId));
        }

        var sprints = await query
            .Select(s => new SprintDto
            {
                Id = s.Id,
                Name = s.Name,
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                TeamId = s.TeamId
            })
            .ToListAsync();

        return Ok(sprints);
    }

    /// <summary>
    /// Gets a specific sprint by its ID.
    /// A user must be a member of the sprint's team, or a super-admin.
    /// </summary>
    /// <param name="sprintId">The ID of the sprint.</param>
    /// <returns>The SprintDto object, or NotFound if the sprint does not exist.</returns>
    [HttpGet("{sprintId}")]
    [Authorize(Policy = "IsTeamMember")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SprintDto>> GetSprint(Guid sprintId)
    {
        var sprint = await _context.Sprints
            .Where(s => s.Id == sprintId)
            .Select(s => new SprintDto
            {
                Id = s.Id,
                Name = s.Name,
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                TeamId = s.TeamId
            })
            .FirstOrDefaultAsync();

        if (sprint == null)
        {
            return NotFound();
        }

        return Ok(sprint);
    }

    /// <summary>
    /// Creates a new sprint.
    /// Only a team-admin or super-admin can create a sprint.
    /// </summary>
    /// <param name="createSprintDto">The data needed to create a sprint.</param>
    /// <returns>The SprintDto of the created sprint.</returns>
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

        var team = await _context.Teams.FindAsync(createSprintDto.TeamId);
        if (team == null)
        {
            ModelState.AddModelError(nameof(createSprintDto.TeamId), "Team not found.");
            return BadRequest(ModelState);
        }

        var isSuperAdmin = User.HasClaim("is_super_admin", "true");
        var isTeamAdmin = team.AdminId == userId;

        if (!isSuperAdmin && !isTeamAdmin)
        {
            return Forbid();
        }

        
        if (createSprintDto.EndDate <= createSprintDto.StartDate)
        {
            ModelState.AddModelError(nameof(createSprintDto.EndDate), "End date must be after start date.");
            return BadRequest(ModelState);
        }

        var sprint = new Sprint
        {
            Name = createSprintDto.Name,
            StartDate = createSprintDto.StartDate.ToUniversalTime(),
            EndDate = createSprintDto.EndDate.ToUniversalTime(),
            TeamId = createSprintDto.TeamId
        };

        _context.Sprints.Add(sprint);
        await _context.SaveChangesAsync();

        var sprintDto = new SprintDto
        {
            Id = sprint.Id,
            Name = sprint.Name,
            StartDate = sprint.StartDate,
            EndDate = sprint.EndDate,
            TeamId = sprint.TeamId
        };

        return CreatedAtAction(nameof(GetSprint), new { sprintId = sprint.Id }, sprintDto);
    }
    
    /// <summary>
    /// Deletes a sprint.
    /// Only the team's admin or a super-admin can delete a sprint.
    /// </summary>
    /// <param name="sprintId">The ID of the sprint to delete.</param>
    /// <returns>NoContent if successful, or an error response.</returns>
    [HttpDelete("{sprintId}")]
    [Authorize(Policy = "IsTeamAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteSprint(Guid sprintId)
    {
        var sprint = await _context.Sprints.Include(s => s.Team).FirstOrDefaultAsync(s => s.Id == sprintId);
        if (sprint == null)
        {
            return NotFound();
        }
        
        _context.Sprints.Remove(sprint);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}