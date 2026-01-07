using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using NikoNiko.Core.DTOs.Sprint;
using NikoNiko.Core.DTOs.Team;
using NikoNiko.Core.DTOs.User; // New using directive
using NikoNiko.Core.Models;
using NikoNiko.Data;

namespace NikoNiko.Api.Controllers;

/// <summary>
/// Contrôleur pour la gestion des équipes.
/// </summary>
[Authorize]
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
        // 1. Récupérer l'ID de l'utilisateur authentifié
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized("User ID not found or invalid.");
        }

        // 2. Build the base query
        var query = _context.Teams
            .Include(t => t.Sprints)
            .Include(t => t.TeamUsers)
            .ThenInclude(tu => tu.User)
            .AsQueryable();

        // 3. Conditionally filter the query
        var isSuperAdmin = User.HasClaim("is_super_admin", "true");
        if (!isSuperAdmin)
        {
            query = query.Where(t => t.AdminId == userId || t.TeamUsers.Any(tu => tu.UserId == userId));
        }

        // 4. Execute the query
        var teams = await query.Select(t => new TeamWithSprintsDto
            {
                Id = t.Id,
                Name = t.Name,
                AdminId = t.AdminId,
                CreatedAt = t.CreatedAt,
                Sprints = t.Sprints.Select(s => new SprintDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    StartDate = s.StartDate.ToUniversalTime(),
                    EndDate = s.EndDate.ToUniversalTime(),
                    TeamId = s.TeamId
                }).ToList(),
                Members = t.TeamUsers.Select(tu => new UserDto // Map TeamUsers to Members
                {
                    Id = tu.User.Id,
                    Email = tu.User.Email,
                    Name = tu.User.Name,
                    AvatarUrl = tu.User.AvatarUrl,
                    CreatedAt = tu.User.CreatedAt
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
    public async Task<ActionResult<TeamWithSprintsDto>> GetTeam(Guid id)
    {
        var team = await _context.Teams
            .Include(t => t.Sprints)
            .Include(t => t.TeamUsers) // Include TeamUsers
            .ThenInclude(tu => tu.User) // Include the User for each TeamUser
            .Select(t => new TeamWithSprintsDto
            {
                Id = t.Id,
                Name = t.Name,
                AdminId = t.AdminId,
                CreatedAt = t.CreatedAt,
                Sprints = t.Sprints.Select(s => new SprintDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    StartDate = s.StartDate.ToUniversalTime(),
                    EndDate = s.EndDate.ToUniversalTime(),
                    TeamId = s.TeamId
                }).ToList(),
                Members = t.TeamUsers.Select(tu => new UserDto // Map TeamUsers to Members
                {
                    Id = tu.User.Id,
                    Email = tu.User.Email,
                    Name = tu.User.Name,
                    AvatarUrl = tu.User.AvatarUrl,
                    CreatedAt = tu.User.CreatedAt
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
    /// Deletes a team.
    /// Only the team's admin or a super-admin can delete a team.
    /// </summary>
    /// <param name="id">The ID of the team to delete.</param>
    /// <returns>NoContent if successful, or an error response.</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTeam(Guid id)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized("User ID not found or invalid.");
        }

        var team = await _context.Teams.FindAsync(id);

        if (team == null)
        {
            return NotFound();
        }

        var isSuperAdmin = User.HasClaim("is_super_admin", "true");
        var isTeamAdmin = team.AdminId == userId;

        if (!isSuperAdmin && !isTeamAdmin)
        {
            return Forbid();
        }

        _context.Teams.Remove(team);
        await _context.SaveChangesAsync();

        return NoContent();
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