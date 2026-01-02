using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NikoNiko.Core.DTOs.Team.Invitation;
using NikoNiko.Services;
using System.Security.Claims;
using System.Collections.Generic;

namespace NikoNiko.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TeamInvitationsController : ControllerBase
    {
        private readonly ITeamInvitationService _teamInvitationService;

        public TeamInvitationsController(ITeamInvitationService teamInvitationService)
        {
            _teamInvitationService = teamInvitationService;
        }

        /// <summary>
        /// Crée une nouvelle invitation d'équipe. Accessible uniquement par les administrateurs d'équipe.
        /// </summary>
        /// <param name="createDto">Les données nécessaires pour créer une invitation.</param>
        /// <returns>L'objet TeamInvitationDto de l'invitation créée.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateTeamInvitation([FromBody] CreateTeamInvitationDto createDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            try
            {
                var invitation = await _teamInvitationService.CreateTeamInvitationAsync(createDto.TeamId, Guid.Parse(userId), createDto);
                // The GetTeamInvitations method is on this controller now, so we can use it for CreatedAtAction
                return CreatedAtAction(nameof(GetTeamInvitations), new { teamId = invitation.TeamId }, invitation);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Accepte une invitation d'équipe. Accessible par tout utilisateur authentifié.
        /// </summary>
        /// <param name="token">Le jeton d'invitation.</param>
        /// <returns>L'objet TeamInvitationDto de l'invitation acceptée.</returns>
        [AllowAnonymous] // Anyone with the token can attempt to accept, but they must be authenticated.
        [HttpPost("{token}/accept")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AcceptTeamInvitation(string token)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                // This scenario should be handled by [Authorize] if applied globally or by a specific policy.
                // For AllowAnonymous, we explicitly check if user is authenticated to get their ID.
                return Unauthorized("Authentication is required to accept an invitation.");
            }

            try
            {
                var acceptedInvitation = await _teamInvitationService.AcceptTeamInvitationAsync(token, Guid.Parse(userId));
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
        /// Récupère toutes les invitations pour une équipe spécifique. Accessible uniquement par les administrateurs d'équipe.
        /// </summary>
        /// <param name="teamId">L'ID de l'équipe.</param>
        /// <returns>Une liste d'objets TeamInvitationDto.</returns>
        [HttpGet("/api/teams/{teamId}/invitations")] // Specific route to align with team-related retrieval
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<TeamInvitationDto>>> GetTeamInvitations(Guid teamId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            try
            {
                // This check needs to be done within the service or explicitly here to ensure only admins can view.
                // The service method should ideally throw UnauthorizedAccessException if user is not admin.
                var teamInvitations = await _teamInvitationService.GetTeamInvitationsAsync(teamId, Guid.Parse(userId));
                // Additional check if needed, or rely on service to enforce admin rights for fetching.
                // For now, assuming GetTeamInvitationsAsync internally checks admin rights.
                return Ok(teamInvitations);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex) // Catch if service throws this for non-admin
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Supprime une invitation d'équipe spécifique. Accessible uniquement par les administrateurs d'équipe.
        /// </summary>
        /// <param name="invitationId">L'ID de l'invitation à supprimer.</param>
        [HttpDelete("{invitationId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteTeamInvitation(Guid invitationId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            try
            {
                await _teamInvitationService.DeleteTeamInvitationAsync(invitationId, Guid.Parse(userId));
                return NoContent(); // 204 No Content
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}