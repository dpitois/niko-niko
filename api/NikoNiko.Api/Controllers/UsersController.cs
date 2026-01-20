using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using NikoNiko.Core.DTOs.User;
using NikoNiko.Data;
using NikoNiko.Services;

namespace NikoNiko.Api.Controllers;

/// <summary>
/// Controller for managing users.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly IUserService _userService;

    public UsersController(ApplicationDbContext context, ITokenService tokenService, IUserService userService)
    {
        _context = context;
        _tokenService = tokenService;
        _userService = userService;
    }

    /// <summary>
    /// Updates the user's onboarding status (consent).
    /// Returns a new JWT token reflecting the updated status.
    /// </summary>
    /// <param name="consentDto">The consent details.</param>
    /// <returns>An object containing the new JWT token.</returns>
    [HttpPost("consent")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<object>> SubmitConsent(UserConsentDto consentDto)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized();
        }

        var user = await _context.Users
            .Include(u => u.TeamUsers)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return NotFound();
        }

        user.IsOnboarded = true;
        user.ConsentAt = DateTime.UtcNow;
        user.ConsentVersion = consentDto.ConsentVersion;

        await _context.SaveChangesAsync();

        var newToken = _tokenService.CreateToken(user);

        return Ok(new { token = newToken });
    }

    /// <summary>
    /// Exports the current user's data (Profile, Teams, Mood History) in JSON format.
    /// </summary>
    /// <returns>A JSON file download.</returns>
    [HttpGet("me/export")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExportData()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized();
        }

        var exportDto = await _userService.GetExportDataAsync(userId);
        if (exportDto == null)
        {
            return NotFound();
        }

        var jsonBytes = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(exportDto, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        var fileName = $"nikoniko-export-{DateTime.UtcNow:yyyyMMdd}.json";

        return File(jsonBytes, "application/json", fileName);
    }

    /// <summary>
    /// Gets a list of all users.
    /// Super-admins get all users. Regular users get users from teams they are a member of.
    /// </summary>
    /// <returns>A list of UserDto objects.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized();
        }

        IQueryable<Core.Models.User> query;

        if (User.HasClaim("is_super_admin", "true"))
        {
            query = _context.Users;
        }
        else
        {
            // Get teams the current user is in
            var userTeamIds = await _context.TeamUsers
                .Where(tu => tu.UserId == userId)
                .Select(tu => tu.TeamId)
                .ToListAsync();

            // Get all users who are in those teams
            query = _context.Users
                .Where(u => u.TeamUsers.Any(tu => userTeamIds.Contains(tu.TeamId)));
        }

        var users = await query
            .Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email,
                Name = u.Name,
                AvatarUrl = u.AvatarUrl,
                Provider = u.Provider,
                CreatedAt = u.CreatedAt
            })
            .Distinct()
            .ToListAsync();

        return Ok(users);
    }

    /// <summary>
    /// Gets a specific user by their ID.
    /// </summary>
    /// <param name="id">The ID of the user.</param>
    /// <returns>The UserDto object, or NotFound if the user does not exist.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetUser(Guid id)
    {
        var user = await _context.Users
            .Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email,
                Name = u.Name,
                AvatarUrl = u.AvatarUrl,
                Provider = u.Provider,
                CreatedAt = u.CreatedAt
            })
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    /// <summary>
    /// Deletes the current user's account.
    /// </summary>
    /// <returns>NoContent if successful, or an error response.</returns>
    [HttpDelete("me")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteMe()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized();
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        // Check if user is admin of any teams
        var teamsAsAdmin = await _context.Teams
            .Include(t => t.TeamUsers)
            .Where(t => t.AdminId == userId)
            .ToListAsync();

        if (teamsAsAdmin.Any())
        {
            foreach (var team in teamsAsAdmin)
            {
                var otherMembersCount = team.TeamUsers.Count(tu => tu.UserId != userId);

                if (otherMembersCount > 0)
                {
                    return Conflict($"Cannot delete your account because you are the admin of team '{team.Name}' which has other members. Please transfer ownership or remove members first.");
                }

                _context.Teams.Remove(team);
            }
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Deletes a specific user account. Accessible only by super-admins.
    /// A user cannot delete their own account.
    /// </summary>
    /// <param name="id">The ID of the user to delete.</param>
    /// <returns>NoContent if successful, or an error response.</returns>
    [HttpDelete("{id}")]
    [Authorize(Policy = "SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var currentUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(currentUserIdString, out var currentUserId) && id == currentUserId)
        {
            return BadRequest("You cannot delete your own account.");
        }

        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        // Check if user is admin of any teams
        var teamsAsAdmin = await _context.Teams
            .Include(t => t.TeamUsers)
            .Where(t => t.AdminId == id)
            .ToListAsync();

        if (teamsAsAdmin.Any())
        {
            foreach (var team in teamsAsAdmin)
            {
                // If team has other members besides the admin (or just multiple members if admin is included in TeamUsers)
                // Note: Admin is usually in TeamUsers too.
                var otherMembersCount = team.TeamUsers.Count(tu => tu.UserId != id);

                if (otherMembersCount > 0)
                {
                    return Conflict($"Cannot delete user because they are the admin of team '{team.Name}' which has other members. Please transfer ownership or remove members first.");
                }

                // If no other members, we can safely delete the team
                _context.Teams.Remove(team);
            }
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}