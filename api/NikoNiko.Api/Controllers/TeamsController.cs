using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using NikoNiko.Data; // Updated using directive
using NikoNiko.Core.DTOs.Sprint; // Updated using directive
using NikoNiko.Core.DTOs.Team; // Updated using directive
using NikoNiko.Core.Models; // Updated using directive

namespace NikoNiko.Api.Controllers;

/// <summary>
/// Contrôleur pour la gestion des équipes.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TeamsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TeamsController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Récupère la liste de toutes les équipes.
    /// </summary>
    /// <returns>Une liste d'objets TeamDto.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TeamWithSprintsDto>>> GetTeams()
    {
        var teams = await _context.Teams
            .Include(t => t.Sprints) // Include sprints
            .Select(t => new TeamWithSprintsDto // Use new DTO
            {
                Id = t.Id,
                Name = t.Name,
                AdminId = t.AdminId,
                CreatedAt = t.CreatedAt,
                Sprints = t.Sprints.Select(s => new SprintDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    StartDate = s.StartDate.ToUniversalTime(), // Convert to UTC
                    EndDate = s.EndDate.ToUniversalTime(),     // Convert to UTC
                    TeamId = s.TeamId
                }).ToList()
            })
            .ToListAsync();

        return Ok(teams);
    }

    /// <summary>
    /// Récupère une équipe spécifique par son ID.
    /// </summary>
    /// <param name="id">L'ID de l'équipe.</param>
    /// <returns>L'objet TeamDto correspondant à l'ID, ou NotFound si l'équipe n'existe pas.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TeamWithSprintsDto>> GetTeam(Guid id) // Use new DTO
    {
        var team = await _context.Teams
            .Include(t => t.Sprints) // Include sprints
            .Select(t => new TeamWithSprintsDto // Use new DTO
            {
                Id = t.Id,
                Name = t.Name,
                AdminId = t.AdminId,
                CreatedAt = t.CreatedAt,
                Sprints = t.Sprints.Select(s => new SprintDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    StartDate = s.StartDate.ToUniversalTime(), // Convert to UTC
                    EndDate = s.EndDate.ToUniversalTime(),     // Convert to UTC
                    TeamId = s.TeamId
                }).ToList()
            })
            .FirstOrDefaultAsync(t => t.Id == id);

        if (team == null)
        {
            return NotFound();
        }

        return Ok(team);
    }

    /// <summary>
    /// Crée une nouvelle équipe.
    /// </summary>
    /// <param name="createTeamDto">Les données nécessaires pour créer une équipe.</param>
    /// <returns>L'objet TeamDto de l'équipe créée.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TeamDto>> CreateTeam(CreateTeamDto createTeamDto)
    {
        // Note: In a real app, you'd validate that the AdminId corresponds to an existing user.
        // We will add this logic later when authentication is in place.
        var team = new Team
        {
            Name = createTeamDto.Name,
            AdminId = createTeamDto.AdminId
        };

        _context.Teams.Add(team);
        await _context.SaveChangesAsync();

        var teamDto = new TeamDto
        {
            Id = team.Id,
            Name = team.Name,
            AdminId = team.AdminId,
            CreatedAt = team.CreatedAt
        };

        return CreatedAtAction(nameof(GetTeam), new { id = team.Id }, teamDto);
    }
}