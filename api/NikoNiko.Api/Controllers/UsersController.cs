using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using NikoNiko.Core.DTOs.User;
using NikoNiko.Core.Interfaces;

namespace NikoNiko.Api.Controllers;

/// <summary>
/// Controller for managing users.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ITokenService _tokenService;

    public UsersController(IUserService userService, ITokenService tokenService)
    {
        _userService = userService;
        _tokenService = tokenService;
    }

    /// <summary>
    /// Updates the user's onboarding status (consent).
    /// </summary>
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

        var user = await _userService.SubmitConsentAsync(userId, consentDto);
        if (user == null)
        {
            return NotFound();
        }

        var newToken = _tokenService.CreateToken(user);
        return Ok(new { token = newToken });
    }

    /// <summary>
    /// Exports the current user's data.
    /// </summary>
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
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized();
        }

        var isSuperAdmin = User.HasClaim("is_super_admin", "true");
        var users = await _userService.GetUsersAsync(userId, isSuperAdmin);
        return Ok(users);
    }

    /// <summary>
    /// Gets a specific user by their ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetUser(Guid id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    /// <summary>
    /// Deletes the current user's account.
    /// </summary>
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

        try
        {
            await _userService.DeleteUserAsync(userId, userId, false);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    /// <summary>
    /// Deletes a specific user account. Accessible only by super-admins.
    /// </summary>
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
        if (!Guid.TryParse(currentUserIdString, out var currentUserId))
        {
            return Unauthorized();
        }

        if (id == currentUserId)
        {
            return BadRequest("You cannot delete your own account.");
        }

        try
        {
            await _userService.DeleteUserAsync(id, currentUserId, true);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }
}