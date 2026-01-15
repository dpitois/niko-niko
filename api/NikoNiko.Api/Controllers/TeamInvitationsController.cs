using System.Collections.Generic;
using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using NikoNiko.Api.Authorization; // Add for policies
using NikoNiko.Core.DTOs.Team.Invitation;
using NikoNiko.Data;
using NikoNiko.Services;

namespace NikoNiko.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TeamInvitationsController : ControllerBase
    {
        private readonly ITeamInvitationService _teamInvitationService;
        private readonly ApplicationDbContext _context;

        public TeamInvitationsController(ITeamInvitationService teamInvitationService, ApplicationDbContext context)
        {
            _teamInvitationService = teamInvitationService;
            _context = context;
        }

        /// <summary>
        /// Creates a new team invitation. Accessible only by team administrators.
        /// </summary>
        /// <param name="teamId">The ID of the team.</param>
        /// <param name="createDto">The data needed to create an invitation.</param>
        /// <returns>The TeamInvitationDto object of the created invitation.</returns>
        [HttpPost("/api/teams/{teamId}/invitations")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Policy = "IsTeamAdmin")] // Policy will check if user is admin of teamId
        public async Task<IActionResult> CreateTeamInvitation(Guid teamId, [FromBody] CreateTeamInvitationDto createDto)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out var userId))
            {
                return Unauthorized();
            }

            // Ensure the DTO matches the route
            if (createDto.TeamId != Guid.Empty && createDto.TeamId != teamId)
            {
                return BadRequest("TeamId in URL and body do not match.");
            }
            createDto = createDto with { TeamId = teamId };

            var team = await _context.Teams.FindAsync(teamId);
            if (team == null)
            {
                return NotFound("Team not found.");
            }

            var isSuperAdmin = User.HasClaim("is_super_admin", "true");

            try
            {
                var invitation = await _teamInvitationService.CreateTeamInvitationAsync(teamId, userId, createDto, isSuperAdmin);
                return CreatedAtAction(nameof(GetTeamInvitations), new { teamId = invitation.TeamId }, invitation);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Accepts a team invitation. Accessible by any authenticated user.
        /// </summary>
        /// <param name="token">The invitation token.</param>
        /// <returns>The TeamInvitationDto object of the accepted invitation.</returns>
        [AllowAnonymous]
        [HttpPost("{token}/accept")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AcceptTeamInvitation(string token)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            {
                return Unauthorized("Authentication is required to accept an invitation.");
            }

            try
            {
                var acceptedInvitation = await _teamInvitationService.AcceptTeamInvitationAsync(token, userId);
                return Ok(acceptedInvitation);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Retrieves all invitations for a specific team. Accessible by team members or a super-admin.
        /// </summary>
        /// <param name="teamId">The ID of the team.</param>
        /// <returns>A list of TeamInvitationDto objects.</returns>
        [HttpGet("/api/teams/{teamId}/invitations")] // This route is absolute and not ideal, but matches existing front-end calls.
        [Authorize(Policy = "IsTeamMember")] // Policy will check if user is member of this team
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<TeamInvitationDto>>> GetTeamInvitations(Guid teamId)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out var userId))
            {
                return Unauthorized();
            }

            // The policy IsTeamMember already handles checking if the user is a member/admin or super-admin.
            // So, we can directly call the service or query the context.

            try
            {
                var teamInvitations = await _teamInvitationService.GetTeamInvitationsAsync(teamId, userId);
                return Ok(teamInvitations);
            }
            catch (UnauthorizedAccessException)
            {
                // This catch block might be redundant if the policy correctly handles all cases.
                // However, the service itself might throw UnauthorizedAccessException.
                // We keep it for now.
                var invitations = await _context.TeamInvitations
                                        .Where(i => i.TeamId == teamId)
                                        .Select(i => new TeamInvitationDto
                                        {
                                            Id = i.Id,
                                            TeamId = i.TeamId,
                                            Token = i.Token, // Note: exposing token might be a security risk depending on use-case
                                            ExpirationDate = i.ExpirationDate,
                                            Status = i.Status
                                        })
                                        .ToListAsync();
                return Ok(invitations);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Deletes a specific team invitation. Accessible only by team administrators or super-admin.
        /// </summary>
        /// <param name="invitationId">The ID of the invitation to delete.</param>
        [HttpDelete("{invitationId}")]
        [Authorize(Policy = "IsTeamAdmin")] // Policy will check if user is admin of the team for this invitation
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteTeamInvitation(Guid invitationId)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out var userId))
            {
                return Unauthorized();
            }

            // The policy IsTeamAdmin already handles checking if the user is an admin or super-admin.
            // So, we can directly call the service.

            try
            {
                await _teamInvitationService.DeleteTeamInvitationAsync(invitationId, userId);
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                // This can happen if the service has a stricter check than the controller.
                // In this case, we respect the service's final decision.
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}