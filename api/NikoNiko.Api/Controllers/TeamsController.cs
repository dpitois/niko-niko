using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using NikoNiko.Core.DTOs.Team;
using NikoNiko.Core.Interfaces;

namespace NikoNiko.Api.Controllers;

/// <summary>
/// Controller for managing teams.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TeamsController : ControllerBase
{
    private readonly ITeamService _teamService;

    public TeamsController(ITeamService teamService)
    {
        _teamService = teamService;
    }

    /// <summary>
    /// Retrieves a list of all teams.
    /// </summary>
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
        var teams = await _teamService.GetTeamsAsync(userId, isSuperAdmin);
        return Ok(teams);
    }

    /// <summary>
    /// Retrieves a specific team by its ID.
    /// </summary>
    [HttpGet("{teamId}")]
    [Authorize(Policy = "IsTeamMember")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TeamWithSprintsDto>> GetTeam(Guid teamId)
    {
        var team = await _teamService.GetTeamByIdAsync(teamId);
        if (team == null)
        {
            return NotFound();
        }

        return Ok(team);
    }

    /// <summary>
    /// Updates the details of a team.
    /// </summary>
    [HttpPut("{teamId}")]
    [Authorize(Policy = "IsTeamAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTeam(Guid teamId, UpdateTeamDto updateTeamDto)
    {
        if (string.IsNullOrWhiteSpace(updateTeamDto.Name))
        {
            return BadRequest("Team name cannot be empty.");
        }

        try
        {
            await _teamService.UpdateTeamAsync(teamId, updateTeamDto);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Transfers the administration of a team to another user.
    /// </summary>
    [HttpPut("{teamId}/admin")]
    [Authorize(Policy = "IsTeamAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTeamAdmin(Guid teamId, [FromBody] UpdateTeamAdminDto updateTeamAdminDto)
    {
        try
        {
            await _teamService.TransferAdminAsync(teamId, updateTeamAdminDto.NewAdminId);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Deletes a team.
    /// </summary>
    [HttpDelete("{teamId}")]
    [Authorize(Policy = "IsTeamAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTeam(Guid teamId)
    {
        try
        {
            await _teamService.DeleteTeamAsync(teamId);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Creates a new team.
    /// </summary>
    [HttpPost]
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

        var isSuperAdmin = User.HasClaim("is_super_admin", "true");

        try
        {
            var teamDto = await _teamService.CreateTeamAsync(createTeamDto, userId, isSuperAdmin);
            return CreatedAtAction(nameof(GetTeam), new { teamId = teamDto.Id }, teamDto);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Removes a user from a specific team.
    /// </summary>
    [HttpDelete("{teamId}/users/{userId}")]
    [Authorize(Policy = "IsTeamAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RemoveUserFromTeam(Guid teamId, Guid userId)
    {
        try
        {
            await _teamService.RemoveUserFromTeamAsync(teamId, userId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}