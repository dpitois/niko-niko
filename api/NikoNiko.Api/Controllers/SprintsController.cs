using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using NikoNiko.Core.DTOs.Sprint; // Updated using directive
using NikoNiko.Core.Models; // Updated using directive
using NikoNiko.Data; // Updated using directive

namespace NikoNiko.Api.Controllers;

/// <summary>
/// Controller for managing sprints.
/// </summary>
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
    /// Gets a list of all sprints.
    /// </summary>
    /// <returns>A list of SprintDto objects.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SprintDto>>> GetSprints()
    {
        var sprints = await _context.Sprints
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
    /// </summary>
    /// <param name="id">The ID of the sprint.</param>
    /// <returns>The SprintDto object, or NotFound if the sprint does not exist.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SprintDto>> GetSprint(Guid id)
    {
        var sprint = await _context.Sprints
            .Select(s => new SprintDto
            {
                Id = s.Id,
                Name = s.Name,
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                TeamId = s.TeamId
            })
            .FirstOrDefaultAsync(s => s.Id == id);

        if (sprint == null)
        {
            return NotFound();
        }

        return Ok(sprint);
    }

    /// <summary>
    /// Creates a new sprint.
    /// </summary>
    /// <param name="createSprintDto">The data needed to create a sprint.</param>
    /// <returns>The SprintDto of the created sprint.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SprintDto>> CreateSprint(CreateSprintDto createSprintDto)
    {
        if (createSprintDto.EndDate <= createSprintDto.StartDate)
        {
            ModelState.AddModelError(nameof(createSprintDto.EndDate), "End date must be after start date.");
            return BadRequest(ModelState);
        }

        var sprint = new Sprint
        {
            Name = createSprintDto.Name,
            StartDate = createSprintDto.StartDate.ToUniversalTime(), // Convert to UTC
            EndDate = createSprintDto.EndDate.ToUniversalTime(),     // Convert to UTC
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

        return CreatedAtAction(nameof(GetSprint), new { id = sprint.Id }, sprintDto);
    }
}