using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using NikoNiko.Core.DTOs.Sprint;
using NikoNiko.Core.DTOs.Team;
using NikoNiko.Core.DTOs.User;
using NikoNiko.Core.Models; // Ensure this is explicitly used
using NikoNiko.Data;
using NikoNiko.Services;

namespace NikoNiko.Api.Controllers;

/// <summary>
/// Controller for managing teams.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TeamsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public TeamsController(ApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    /// <summary>
    /// Retrieves a list of all teams.
    /// For regular users, returns only teams where they are members or administrators.
    /// For super-admins, returns all teams.
    /// </summary>
    /// <returns>A list of TeamWithSprintsDto objects.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<TeamWithSprintsDto>>> GetTeams()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized("User ID not found or invalid.");
        }

        var isSuperAdmin = User.HasClaim("is_super_admin", "true");

        IQueryable<Team> baseQuery = _context.Teams
            .Include(t => t.Admin)
            .Include(t => t.Sprints)
            .Include(t => t.TeamUsers)
            .ThenInclude(tu => tu.User);

        if (!isSuperAdmin)
        {
            // For regular users, filter teams to only those they are an admin or member of
            baseQuery = baseQuery.Where(t => t.AdminId == userId || t.TeamUsers.Any(tu => tu.UserId == userId));
        }

        var teams = await baseQuery.Select(t => new TeamWithSprintsDto
        {
            Id = t.Id,
            Name = t.Name,
            AdminId = t.AdminId,
            AdminName = t.Admin.Name ?? t.Admin.Email,
            CreatedAt = t.CreatedAt,
            Sprints = t.Sprints.Select(s => new SprintDto
            {
                Id = s.Id,
                Name = s.Name,
                StartDate = s.StartDate.ToUniversalTime(),
                EndDate = s.EndDate.ToUniversalTime(),
                TeamId = s.TeamId
            }).ToList(),
            Members = t.TeamUsers.Select(tu => new UserDto
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
    /// Retrieves a specific team by its ID.
    /// A user must be a team member or a super-admin to access it.
    /// </summary>
    /// <param name="teamId">The ID of the team.</param>
    /// <returns>The TeamWithSprintsDto object corresponding to the ID, or NotFound if the team does not exist.</returns>
    [HttpGet("{teamId}")]
    [Authorize(Policy = "IsTeamMember")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TeamWithSprintsDto>> GetTeam(Guid teamId)
    {
        var team = await _context.Teams
            .Include(t => t.Admin)
            .Include(t => t.Sprints)
            .Include(t => t.TeamUsers)
            .ThenInclude(tu => tu.User)
            .Select(t => new TeamWithSprintsDto
            {
                Id = t.Id,
                Name = t.Name,
                AdminId = t.AdminId,
                AdminName = t.Admin.Name ?? t.Admin.Email,
                CreatedAt = t.CreatedAt,
                Sprints = t.Sprints.Select(s => new SprintDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    StartDate = s.StartDate.ToUniversalTime(),
                    EndDate = s.EndDate.ToUniversalTime(),
                    TeamId = s.TeamId
                }).ToList(),
                Members = t.TeamUsers.Select(tu => new UserDto
                {
                    Id = tu.User.Id,
                    Email = tu.User.Email,
                    Name = tu.User.Name,
                    AvatarUrl = tu.User.AvatarUrl,
                    CreatedAt = tu.User.CreatedAt
                }).ToList()
            })
            .FirstOrDefaultAsync(t => t.Id == teamId);

        if (team == null)
        {
            return NotFound();
        }

        return Ok(team);
    }

    /// <summary>
    /// Updates the details of a team.
    /// Only the team's admin or a super-admin can update a team.
    /// </summary>
    /// <param name="teamId">The ID of the team to update.</param>
    /// <param name="updateTeamDto">The updated team data.</param>
    /// <returns>NoContent if successful, or an error response.</returns>
    [HttpPut("{teamId}")]
    [Authorize(Policy = "IsTeamAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTeam(Guid teamId, UpdateTeamDto updateTeamDto)
    {
        var team = await _context.Teams.FindAsync(teamId);

        if (team == null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(updateTeamDto.Name))
        {
            return BadRequest("Team name cannot be empty.");
        }

        team.Name = updateTeamDto.Name;
        await _context.SaveChangesAsync();

        await _notificationService.NotifyTeamRenamedAsync(team.Id, team.Name);

        return NoContent();
    }

    /// <summary>
    /// Deletes a team.
    /// Only the team's admin or a super-admin can delete a team.
    /// </summary>
    /// <param name="teamId">The ID of the team to delete.</param>
    /// <returns>NoContent if successful, or an error response.</returns>
    [HttpDelete("{teamId}")]
    [Authorize(Policy = "IsTeamAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTeam(Guid teamId)
    {
        var team = await _context.Teams.FindAsync(teamId);

        if (team == null)
        {
            return NotFound();
        }

        _context.Teams.Remove(team);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Creates a new team.
    /// Accessible only by a Super-admin.
    /// </summary>
    /// <param name="createTeamDto">The data needed to create a team.</param>
    /// <returns>The TeamDto object of the created team.</returns>
    [HttpPost]
    [Authorize(Policy = "SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TeamDto>> CreateTeam(CreateTeamDto createTeamDto)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized("User ID not found or invalid.");
        }

        var team = new Team
        {
            Name = createTeamDto.Name,
            AdminId = userId
        };

        var teamUser = new TeamUser
        {
            Team = team,
            UserId = userId
        };

        _context.Teams.Add(team);
        _context.TeamUsers.Add(teamUser);
        await _context.SaveChangesAsync();

        // Load admin to get the name
        await _context.Entry(team).Reference(t => t.Admin).LoadAsync();

        var teamDto = new TeamDto
        {
            Id = team.Id,
            Name = team.Name,
            AdminId = team.AdminId,
            AdminName = team.Admin.Name ?? team.Admin.Email,
            CreatedAt = team.CreatedAt
        };

        return CreatedAtAction(nameof(GetTeam), new { teamId = team.Id }, teamDto);
    }

    /// <summary>
    /// Removes a user from a specific team.
    /// Only the team's admin or a super-admin can remove users.
    /// A team admin cannot remove themselves.
    /// </summary>
    /// <param name="teamId">The ID of the team.</param>
    /// <param name="userId">The ID of the user to remove.</param>
    /// <returns>NoContent if successful, or an error response.</returns>
    [HttpDelete("{teamId}/users/{userId}")]
    [Authorize(Policy = "IsTeamAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RemoveUserFromTeam(Guid teamId, Guid userId)
    {
        var team = await _context.Teams.FindAsync(teamId);
        if (team == null)
        {
            return NotFound("Team not found.");
        }

        var userToRemove = await _context.Users.FindAsync(userId);
        if (userToRemove == null)
        {
            return NotFound("User not found.");
        }

        // Prevent admin from removing themselves via this endpoint (they should delete the team if they want to leave as admin)
        if (team.AdminId == userId)
        {
            return BadRequest("Cannot remove the team's administrator through this endpoint.");
        }

        var teamUser = await _context.TeamUsers
            .FirstOrDefaultAsync(tu => tu.TeamId == teamId && tu.UserId == userId);

        if (teamUser == null)
        {
            return NotFound("User is not a member of this team.");
        }

        _context.TeamUsers.Remove(teamUser);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}